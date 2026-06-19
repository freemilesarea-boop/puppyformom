using System.Collections.Generic;
using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// Builds the layered, scrolling backdrop: sky (static), then clouds / city / trees as
    /// horizontally-tiled parallax layers moving at increasing speeds for depth.
    ///
    /// Each layer uses a real PNG from <see cref="AssetLoader"/> when available, and falls back
    /// to a procedural <see cref="SpriteFactory"/> strip otherwise — so it always renders and
    /// real art drops in with no code changes.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private float spanLeft = -9f;
        [SerializeField] private float spanRight = 9f;

        private struct Layer
        {
            public List<Transform> tiles;
            public float factor;     // fraction of world speed
            public float tileWidth;
            public float leftLimit;
            public float span;
        }

        private readonly List<Layer> _layers = new List<Layer>();

        private void Start()
        {
            BuildSky();
            // bottomY anchors the strip's base; clouds float high in the sky.
            BuildTiledLayer(AssetLoader.Get(ArtKeys.BgClouds, SpriteFactory.CloudStrip), bottomY: 2.4f, worldHeight: 2.4f, factor: 0.10f, order: -16);
            BuildTiledLayer(AssetLoader.Get(ArtKeys.BgCity, SpriteFactory.CityStrip), bottomY: GameConfig.GroundY, worldHeight: 3.2f, factor: 0.25f, order: -12);
            BuildTiledLayer(AssetLoader.Get(ArtKeys.BgTrees, SpriteFactory.TreeStrip), bottomY: GameConfig.GroundY, worldHeight: 2.3f, factor: 0.45f, order: -8);
        }

        private void BuildSky()
        {
            var sky = AssetLoader.Get(ArtKeys.BgSky,
                () => SpriteFactory.VerticalGradient(GameConfig.SkyTop, GameConfig.SkyBottom));
            if (sky == null) return;

            var go = NewSprite("Sky", sky, -20);
            go.transform.position = new Vector3(0f, 1.5f, 0f);
            // stretch to comfortably cover a portrait camera regardless of source size
            float sx = (spanRight - spanLeft + 8f) / sky.bounds.size.x;
            float sy = 18f / sky.bounds.size.y;
            go.transform.localScale = new Vector3(sx, sy, 1f);
        }

        /// <summary>Tiles a single sprite across the view and registers it for parallax scrolling.</summary>
        private void BuildTiledLayer(Sprite sprite, float bottomY, float worldHeight, float factor, int order)
        {
            if (sprite == null) return;

            float scale = worldHeight / sprite.bounds.size.y;
            float tileW = sprite.bounds.size.x * scale;
            if (tileW < 0.01f) return;

            float yCenter = bottomY + worldHeight * 0.5f;
            int count = Mathf.CeilToInt((spanRight - spanLeft) / tileW) + 2;

            var root = new GameObject($"Layer_{order}").transform;
            root.SetParent(transform, false);

            var tiles = new List<Transform>(count);
            for (int i = 0; i < count; i++)
            {
                var go = NewSprite("tile", sprite, order);
                go.transform.SetParent(root, false);
                go.transform.localScale = new Vector3(scale, scale, 1f);
                go.transform.position = new Vector3(spanLeft + i * tileW, yCenter, 0f);
                tiles.Add(go.transform);
            }

            _layers.Add(new Layer
            {
                tiles = tiles,
                factor = factor,
                tileWidth = tileW,
                leftLimit = spanLeft - tileW,
                span = tileW * count
            });
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            float speed = ServiceLocator.Get<DistanceManager>()?.CurrentSpeed ?? GameConfig.StartScrollSpeed;

            foreach (var layer in _layers)
            {
                float dx = speed * layer.factor * Time.deltaTime;
                foreach (var t in layer.tiles)
                {
                    var p = t.position;
                    p.x -= dx;
                    if (p.x < layer.leftLimit) p.x += layer.span;
                    t.position = p;
                }
            }
        }

        private GameObject NewSprite(string name, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return go;
        }
    }
}
