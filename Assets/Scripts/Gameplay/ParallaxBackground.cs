using System.Collections.Generic;
using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// Builds a warm pastel sky and a slow-moving cloud layer for a calm, "healing" backdrop.
    /// Clouds scroll at a fraction of the world speed for depth.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private float spanLeft = -10f;
        [SerializeField] private float spanRight = 10f;
        [SerializeField] private float cloudParallax = 0.15f;

        private readonly List<Transform> _clouds = new List<Transform>();
        private float _width;

        private void Start()
        {
            _width = spanRight - spanLeft;
            CreateSky();
            CreateClouds();
            CreateHills();
        }

        private void CreateSky()
        {
            var sky = NewSprite("Sky",
                SpriteFactory.VerticalGradient(GameConfig.SkyTop, GameConfig.SkyBottom),
                sortingOrder: -20);
            // stretch to comfortably cover a portrait camera
            sky.transform.position = new Vector3(0f, 1.5f, 0f);
            sky.transform.localScale = new Vector3(26f, 16f, 1f);
        }

        private void CreateHills()
        {
            var hillSprite = SpriteFactory.Circle(new Color(0.86f, 0.92f, 0.74f), 64);
            for (int i = 0; i < 5; i++)
            {
                var h = NewSprite("Hill", hillSprite, sortingOrder: -10);
                h.transform.position = new Vector3(spanLeft + i * (_width / 4f), GameConfig.GroundY - 1.5f, 0f);
                h.transform.localScale = new Vector3(Random.Range(5f, 8f), Random.Range(3f, 4.5f), 1f);
            }
        }

        private void CreateClouds()
        {
            var cloudSprite = SpriteFactory.Cloud();
            for (int i = 0; i < 4; i++)
            {
                var c = NewSprite("Cloud", cloudSprite, sortingOrder: -15);
                c.transform.position = new Vector3(
                    spanLeft + i * (_width / 3f),
                    Random.Range(2.5f, 5.5f), 0f);
                c.transform.localScale = Vector3.one * Random.Range(0.8f, 1.6f);
                _clouds.Add(c.transform);
            }
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            float speed = (ServiceLocator.Get<DistanceManager>()?.CurrentSpeed ?? GameConfig.StartScrollSpeed) * cloudParallax;

            foreach (var c in _clouds)
            {
                var p = c.position;
                p.x -= speed * Time.deltaTime;
                if (p.x < spanLeft - 2f) p.x += _width + 4f;
                c.position = p;
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
