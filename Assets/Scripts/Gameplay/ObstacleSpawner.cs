using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// Spawns hazards and treats just off the right edge of the screen while the run is active.
    /// Spawn cadence tightens as the puppy travels further. Builds GameObjects entirely in code
    /// (sprite + trigger collider) using procedurally generated placeholder sprites.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        private float _timer;
        private float _nextDelay;

        // Base world heights (set by GameplayBootstrap.Configure; defaults keep it usable standalone).
        private float _obstacleH = GameConfig.ObstacleWorldHeight;
        private float _collectibleH = GameConfig.CollectibleWorldHeight;

        // cached sprites
        private Sprite _car, _puddle, _bin, _fence, _cone, _bone, _smell, _photo;
        private Transform _container;

        private void Awake()
        {
            _container = new GameObject("Spawned").transform;
            _container.SetParent(transform, false);
            BuildSprites();
            _nextDelay = GameConfig.StartSpawnInterval;
        }

        /// <summary>Injected by GameplayBootstrap so the sizes are tunable from the Scene.</summary>
        public void Configure(float obstacleWorldHeight, float collectibleWorldHeight)
        {
            _obstacleH = obstacleWorldHeight;
            _collectibleH = collectibleWorldHeight;
        }

        // Per-type height multipliers (applied on top of the category world height) so the
        // different objects keep distinct silhouettes while staying resolution-independent.
        private static float ObstacleMultiplier(ObstacleType t) => t switch
        {
            ObstacleType.Car => 0.90f,    // short but wide (~73% of the puppy)
            ObstacleType.Puddle => 0.42f, // flat
            ObstacleType.Bin => 0.90f,    // ~73%
            ObstacleType.Fence => 1.10f,  // tall (~90%)
            ObstacleType.Cone => 1.00f,   // ~82% of the puppy
            _ => 1f
        };

        private static float CollectibleMultiplier(CollectibleType t) => t switch
        {
            CollectibleType.Bone => 1.00f,
            CollectibleType.Smell => 1.10f,
            CollectibleType.Photo => 1.00f,
            _ => 1f
        };

        private void BuildSprites()
        {
            // Real PNG if present in Resources/Art, otherwise the procedural placeholder.
            _car = AssetLoader.Get(ArtKeys.ObstacleCar, () => SpriteFactory.SolidRounded(GameConfig.ObstacleCar, 180, 95, 20));
            _puddle = AssetLoader.Get(ArtKeys.ObstaclePuddle, () => SpriteFactory.SolidRounded(GameConfig.ObstaclePuddle, 150, 38, 18));
            _bin = AssetLoader.Get(ArtKeys.ObstacleTrashBin, () => SpriteFactory.SolidRounded(GameConfig.ObstacleBin, 70, 100, 12));
            _fence = AssetLoader.Get(ArtKeys.ObstacleFence, () => SpriteFactory.SolidRounded(GameConfig.ObstacleFence, 95, 115, 8));
            _cone = AssetLoader.Get(ArtKeys.ObstacleCone, () => SpriteFactory.SolidRounded(GameConfig.ObstacleCone, 65, 95, 14));
            _bone = AssetLoader.Get(ArtKeys.CollectibleBone, () => SpriteFactory.Bone());
            _smell = AssetLoader.Get(ArtKeys.CollectibleScent, () => SpriteFactory.Smell());
            _photo = AssetLoader.Get(ArtKeys.CollectiblePhotoPiece, () => SpriteFactory.PhotoPiece());
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

            _timer += Time.deltaTime;
            if (_timer < _nextDelay) return;
            _timer = 0f;

            // pick next spawn: mostly treats, sometimes hazards
            if (Random.value < GameConfig.CollectibleChance)
                SpawnCollectibleCluster();
            else
                SpawnObstacle();

            ScheduleNext();
        }

        private void ScheduleNext()
        {
            // tighten interval based on distance (difficulty ramp)
            int meters = ServiceLocator_DistanceMeters();
            float t = Mathf.Clamp01(meters / 4000f);
            float baseInterval = Mathf.Lerp(GameConfig.StartSpawnInterval, GameConfig.MinSpawnInterval, t);
            _nextDelay = Mathf.Max(GameConfig.MinSpawnGap, baseInterval * Random.Range(0.8f, 1.3f));
        }

        private static int ServiceLocator_DistanceMeters()
        {
            var dm = Core.ServiceLocator.Get<Systems.DistanceManager>();
            return dm != null ? dm.Meters : 0;
        }

        private void SpawnObstacle()
        {
            ObstacleType type = (ObstacleType)Random.Range(0, 5);
            Sprite sprite = type switch
            {
                ObstacleType.Car => _car,
                ObstacleType.Puddle => _puddle,
                ObstacleType.Bin => _bin,
                ObstacleType.Fence => _fence,
                _ => _cone
            };

            var go = NewSpriteObject($"Obstacle_{type}", sprite, sortingOrder: 5);

            // Normalize to a gameplay world height (independent of the PNG's pixel size).
            float targetH = _obstacleH * ObstacleMultiplier(type);
            NormalizeHeight(go.transform, sprite, targetH);
            go.transform.position = new Vector3(GameConfig.SpawnXOffset, GameConfig.GroundY + targetH * 0.5f, 0f);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            // Collider is sized from the normalized sprite, then shrunk so grazes feel fair.
            col.size = sprite.bounds.size * GameConfig.ObstacleColliderFactor;
            col.offset = sprite.bounds.center;

            go.AddComponent<Obstacle>().Init(type);
        }

        private void SpawnCollectibleCluster()
        {
            int count = Random.Range(1, 4);
            float baseY = GameConfig.GroundY + Random.Range(0.4f, 2.0f);
            for (int i = 0; i < count; i++)
            {
                CollectibleType type = RollCollectibleType();
                Sprite sprite = type switch
                {
                    CollectibleType.Smell => _smell,
                    CollectibleType.Photo => _photo,
                    _ => _bone
                };

                var go = NewSpriteObject($"Treat_{type}", sprite, sortingOrder: 4);

                // Normalize to a gameplay world height (independent of the PNG's pixel size).
                float targetH = _collectibleH * CollectibleMultiplier(type);
                NormalizeHeight(go.transform, sprite, targetH);

                float arc = Mathf.Sin(i / Mathf.Max(1f, count - 1f) * Mathf.PI) * 0.5f;
                go.transform.position = new Vector3(
                    GameConfig.SpawnXOffset + i * 0.7f,
                    Mathf.Clamp(baseY + arc, GameConfig.GroundY + 0.3f, GameConfig.GroundY + 3f),
                    0f);

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                // Radius from the normalized half-height -> consistent pickup size for any PNG.
                col.radius = sprite.bounds.extents.y * GameConfig.CollectibleColliderFactor;
                col.offset = sprite.bounds.center;

                go.AddComponent<Collectible>().Init(type);
            }
        }

        private static CollectibleType RollCollectibleType()
        {
            float r = Random.value;
            if (r < 0.08f) return CollectibleType.Photo;   // rare, unlocks ending
            if (r < 0.20f) return CollectibleType.Smell;   // story flavour
            return CollectibleType.Bone;                    // common score
        }

        /// <summary>
        /// Uniformly scales the object so the sprite's WORLD height equals <paramref name="targetWorldHeight"/>,
        /// preserving aspect ratio. This makes on-screen size independent of the PNG resolution.
        /// </summary>
        private static void NormalizeHeight(Transform t, Sprite sprite, float targetWorldHeight)
        {
            float h = sprite.bounds.size.y;
            float scale = h > 0.0001f ? targetWorldHeight / h : 1f;
            t.localScale = new Vector3(scale, scale, 1f);
        }

        private GameObject NewSpriteObject(string name, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_container, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return go;
        }
    }
}
