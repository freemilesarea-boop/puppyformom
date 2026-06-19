using System.Collections.Generic;
using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// The road / ground layer, scrolling at full world speed under the puppy.
    /// Uses a real road PNG (<c>bg_road</c>) tiled across the screen when available; otherwise
    /// falls back to a solid colour band plus scrolling grass tufts so it always reads as motion.
    /// </summary>
    public class GroundScroller : MonoBehaviour
    {
        [SerializeField] private float spanLeft = -9f;
        [SerializeField] private float spanRight = 9f;
        [SerializeField] private float tuftSpacing = 1.6f;

        private readonly List<Transform> _scrollers = new List<Transform>();
        private float _width;
        private float _wrapSpan;
        private float _leftLimit;

        private void Start()
        {
            _width = spanRight - spanLeft;

            // The road surface sits just below the ground line; the puppy's feet rest at GroundY.
            var road = AssetLoader.Get(ArtKeys.BgRoad, () => (Sprite)null);
            if (road != null)
                BuildRoadTiles(road);
            else
                BuildColorGroundWithTufts();
        }

        private void BuildRoadTiles(Sprite road)
        {
            float worldHeight = 3.4f;
            float scale = worldHeight / road.bounds.size.y;
            float tileW = road.bounds.size.x * scale;
            float yCenter = GameConfig.GroundY - worldHeight * 0.5f + 0.15f;
            int count = Mathf.CeilToInt(_width / tileW) + 2;

            _wrapSpan = tileW * count;
            _leftLimit = spanLeft - tileW;

            for (int i = 0; i < count; i++)
            {
                var t = NewSprite("Road", road, sortingOrder: 1).transform;
                t.localScale = new Vector3(scale, scale, 1f);
                t.position = new Vector3(spanLeft + i * tileW, yCenter, 0f);
                _scrollers.Add(t);
            }
        }

        private void BuildColorGroundWithTufts()
        {
            // main band
            var band = NewSprite("GroundBand", SpriteFactory.SolidRounded(GameConfig.GroundColor, 64, 64, 0), 1);
            band.transform.position = new Vector3(0f, GameConfig.GroundY - 2.5f, 0f);
            band.transform.localScale = new Vector3(_width + 4f, 5.2f, 1f);

            // darker shadow strip just under the ground line
            var strip = NewSprite("GroundLine", SpriteFactory.SolidRounded(GameConfig.GroundShadow, 64, 64, 0), 2);
            strip.transform.position = new Vector3(0f, GameConfig.GroundY - 0.12f, 0f);
            strip.transform.localScale = new Vector3(_width + 4f, 0.22f, 1f);

            _wrapSpan = _width;
            _leftLimit = spanLeft;

            var tuftSprite = SpriteFactory.SolidRounded(GameConfig.GroundShadow, 40, 26, 12);
            for (float x = spanLeft; x <= spanRight; x += tuftSpacing)
            {
                var t = NewSprite("Tuft", tuftSprite, 3).transform;
                t.position = new Vector3(x, GameConfig.GroundY - 0.05f, 0f);
                t.localScale = new Vector3(Random.Range(0.8f, 1.3f), Random.Range(0.7f, 1.1f), 1f);
                _scrollers.Add(t);
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            float speed = ServiceLocator.Get<DistanceManager>()?.CurrentSpeed ?? GameConfig.StartScrollSpeed;

            float dx = speed * Time.deltaTime;
            foreach (var t in _scrollers)
            {
                var p = t.position;
                p.x -= dx;
                if (p.x < _leftLimit) p.x += _wrapSpan;
                t.position = p;
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
