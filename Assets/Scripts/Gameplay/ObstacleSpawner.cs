using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// Difficulty-driven pattern spawner. Instead of one random obstacle at a time, it picks a
    /// PATTERN (ground / low / high / collectible-line / mixed / safe-gap) whose weights shift with
    /// the current level (distance / 100 m). Patterns are spaced by time so the player always has a
    /// fair reaction window, and "mixed" jump→duck combos are spaced by a full jump arc so they are
    /// never an instant-death.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        private enum Pattern { Ground, Low, High, CollectibleLine, Mixed, SafeGap }

        private float _timer;
        private float _nextDelay;

        // Base world heights (set by GameplayBootstrap.Configure; defaults keep it usable standalone).
        private float _obstacleH = GameConfig.ObstacleWorldHeight;
        private float _collectibleH = GameConfig.CollectibleWorldHeight;

        // cached sprites
        private Sprite _car, _puddle, _bin, _fence, _cone, _highBar, _bone, _smell, _photo;
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

        // ---- per-type height multipliers (ground obstacles keep distinct silhouettes) ----
        private static float ObstacleMultiplier(ObstacleType t) => t switch
        {
            ObstacleType.Car => 0.90f,
            ObstacleType.Puddle => 0.42f,
            ObstacleType.Bin => 0.90f,
            ObstacleType.Fence => 1.10f,
            ObstacleType.Cone => 1.00f,
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
            _car = AssetLoader.Get(ArtKeys.ObstacleCar, () => SpriteFactory.SolidRounded(GameConfig.ObstacleCar, 180, 95, 20));
            _puddle = AssetLoader.Get(ArtKeys.ObstaclePuddle, () => SpriteFactory.SolidRounded(GameConfig.ObstaclePuddle, 150, 38, 18));
            _bin = AssetLoader.Get(ArtKeys.ObstacleTrashBin, () => SpriteFactory.SolidRounded(GameConfig.ObstacleBin, 70, 100, 12));
            _fence = AssetLoader.Get(ArtKeys.ObstacleFence, () => SpriteFactory.SolidRounded(GameConfig.ObstacleFence, 95, 115, 8));
            _cone = AssetLoader.Get(ArtKeys.ObstacleCone, () => SpriteFactory.SolidRounded(GameConfig.ObstacleCone, 65, 95, 14));
            _highBar = AssetLoader.Get(ArtKeys.ObstacleHighBar, () => SpriteFactory.HighBar());
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

            var dist = ServiceLocator.Get<DistanceManager>();
            int level = dist != null ? dist.Level : 0;
            float speed = dist != null ? dist.CurrentSpeed : GameConfig.StartScrollSpeed;

            Pattern pattern = SelectPattern(level);
            float worldLength = SpawnPattern(pattern, level, speed);

            // Always leave a fair reaction window; SafeGap adds an extra breather.
            float gap = ReactionGap(level);
            if (pattern == Pattern.SafeGap) gap += 1.0f;
            _nextDelay = worldLength / Mathf.Max(1f, speed) + gap;
        }

        private static float ReactionGap(int level)
        {
            float t = Mathf.Clamp01((float)level / GameConfig.ReactionGapMaxLevel);
            return Mathf.Lerp(GameConfig.ReactionGapEasy, GameConfig.ReactionGapHard, t);
        }

        // ---- pattern selection: weights shift with the level ----
        private Pattern SelectPattern(int level)
        {
            // weights: ground, low, high, line, mixed, safegap
            float wGround = 3f;
            float wLow = 2f;
            float wLine = level >= 1 ? 3f : 0f;                 // 100 m+
            float wHigh = level >= 2 ? Mathf.Min(3f, level - 1) : 0f; // 200 m+
            float wMixed = level >= 3 ? Mathf.Min(4f, level - 2) : 0f; // 300 m+ (rises)
            float wSafe = Mathf.Max(1f, 4f - level);            // frequent early, rare later

            float total = wGround + wLow + wHigh + wLine + wMixed + wSafe;
            float r = Random.value * total;

            if ((r -= wGround) < 0) return Pattern.Ground;
            if ((r -= wLow) < 0) return Pattern.Low;
            if ((r -= wLine) < 0) return Pattern.CollectibleLine;
            if ((r -= wHigh) < 0) return Pattern.High;
            if ((r -= wMixed) < 0) return Pattern.Mixed;
            return Pattern.SafeGap;
        }

        /// <summary>Spawns the pattern and returns the world length it occupies (for spacing).</summary>
        private float SpawnPattern(Pattern pattern, int level, float speed)
        {
            switch (pattern)
            {
                case Pattern.Ground: return SpawnGround();
                case Pattern.Low: return SpawnLow();
                case Pattern.High: return SpawnHigh();
                case Pattern.CollectibleLine: return SpawnCollectibleLine(level);
                case Pattern.Mixed: return SpawnMixed(speed);
                default: return SpawnSafeGap();
            }
        }

        // ---------------- patterns ----------------
        private float SpawnGround()
        {
            ObstacleType type = PickGroundType();
            float h = _obstacleH * ObstacleMultiplier(type);
            return SpawnObstacleAt(type, SpriteFor(type), h, GameConfig.GroundY, 0f);
        }

        private float SpawnLow()
        {
            // a short, easy-to-clear ground hazard
            ObstacleType type = Random.value < 0.5f ? ObstacleType.Puddle : ObstacleType.Cone;
            float h = GameConfig.LowObstacleWorldHeight * (type == ObstacleType.Puddle ? 0.8f : 1f);
            return SpawnObstacleAt(type, SpriteFor(type), h, GameConfig.GroundY, 0f);
        }

        private float SpawnHigh()
        {
            // head-height bar: hits a standing dog, cleared by ducking
            float bottom = GameConfig.GroundY + GameConfig.HighObstacleClearance;
            return SpawnObstacleAt(ObstacleType.HighBar, _highBar, GameConfig.HighObstacleWorldHeight, bottom, 0f);
        }

        private float SpawnMixed(float speed)
        {
            // jump the ground obstacle, then duck the high bar — spaced by a full jump arc so the
            // dog has time to land before needing to duck (never an instant-death).
            ObstacleType groundType = PickGroundType();
            float gh = _obstacleH * ObstacleMultiplier(groundType);
            SpawnObstacleAt(groundType, SpriteFor(groundType), gh, GameConfig.GroundY, 0f);

            float jumpArc = 2f * GameConfig.JumpVelocity / (GameConfig.Gravity * 9.81f); // seconds airborne
            float spacing = jumpArc * speed + 2.2f;                                      // + reaction margin

            float bottom = GameConfig.GroundY + GameConfig.HighObstacleClearance;
            SpawnObstacleAt(ObstacleType.HighBar, _highBar, GameConfig.HighObstacleWorldHeight, bottom, spacing);
            return spacing + 1.5f;
        }

        private float SpawnCollectibleLine(int level)
        {
            int count = Mathf.Clamp(4 + level, 4, 8);
            const float step = 0.9f;
            float y = GameConfig.GroundY + 0.9f; // mid-run height, collected while running
            for (int i = 0; i < count; i++)
            {
                CollectibleType type = RollCollectibleType();
                SpawnCollectibleAt(type, SpriteForCollectible(type), GameConfig.SpawnXOffset + i * step, y);
            }
            return (count - 1) * step;
        }

        private float SpawnSafeGap()
        {
            // a calm breather with a couple of easy treats as a reward
            float y = GameConfig.GroundY + 0.9f;
            SpawnCollectibleAt(CollectibleType.Bone, _bone, GameConfig.SpawnXOffset, y);
            SpawnCollectibleAt(CollectibleType.Bone, _bone, GameConfig.SpawnXOffset + 0.9f, y);
            return 0.9f;
        }

        // ---------------- spawn helpers ----------------
        private float SpawnObstacleAt(ObstacleType type, Sprite sprite, float worldHeight, float bottomYWorld, float xOffset)
        {
            var go = NewSpriteObject($"Obstacle_{type}", sprite, sortingOrder: 5);
            NormalizeHeight(go.transform, sprite, worldHeight);
            float worldWidth = sprite.bounds.size.x * go.transform.localScale.x;
            go.transform.position = new Vector3(GameConfig.SpawnXOffset + xOffset, bottomYWorld + worldHeight * 0.5f, 0f);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = sprite.bounds.size * GameConfig.ObstacleColliderFactor; // local; ×scale = gameplay size
            col.offset = sprite.bounds.center;

            go.AddComponent<Obstacle>().Init(type);
            return worldWidth;
        }

        private void SpawnCollectibleAt(CollectibleType type, Sprite sprite, float x, float y)
        {
            var go = NewSpriteObject($"Treat_{type}", sprite, sortingOrder: 4);
            float h = _collectibleH * CollectibleMultiplier(type);
            NormalizeHeight(go.transform, sprite, h);
            go.transform.position = new Vector3(x, y, 0f);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = sprite.bounds.extents.y * GameConfig.CollectibleColliderFactor;
            col.offset = sprite.bounds.center;

            go.AddComponent<Collectible>().Init(type);
        }

        private static ObstacleType PickGroundType()
        {
            // ground obstacles only (no puddle-as-special, no high bar)
            switch (Random.Range(0, 4))
            {
                case 0: return ObstacleType.Car;
                case 1: return ObstacleType.Bin;
                case 2: return ObstacleType.Fence;
                default: return ObstacleType.Cone;
            }
        }

        private static CollectibleType RollCollectibleType()
        {
            float r = Random.value;
            if (r < 0.06f) return CollectibleType.Photo;  // rare, unlocks ending
            if (r < 0.18f) return CollectibleType.Smell;  // story flavour
            return CollectibleType.Bone;                   // common score
        }

        private Sprite SpriteFor(ObstacleType t) => t switch
        {
            ObstacleType.Car => _car,
            ObstacleType.Puddle => _puddle,
            ObstacleType.Bin => _bin,
            ObstacleType.Fence => _fence,
            ObstacleType.HighBar => _highBar,
            _ => _cone
        };

        private Sprite SpriteForCollectible(CollectibleType t) => t switch
        {
            CollectibleType.Smell => _smell,
            CollectibleType.Photo => _photo,
            _ => _bone
        };

        /// <summary>Uniformly scales so the sprite's WORLD height matches the target (resolution-independent).</summary>
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
