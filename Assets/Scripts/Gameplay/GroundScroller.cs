using System.Collections.Generic;
using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// Draws the static ground band and scrolls a row of grass tufts left (wrapping around)
    /// to sell the sense of forward motion under the puppy.
    /// </summary>
    public class GroundScroller : MonoBehaviour
    {
        [SerializeField] private float spanLeft = -9f;
        [SerializeField] private float spanRight = 9f;
        [SerializeField] private float tuftSpacing = 1.6f;

        private readonly List<Transform> _tufts = new List<Transform>();
        private float _width;

        private void Start()
        {
            _width = spanRight - spanLeft;
            CreateGroundBand();
            CreateTufts();
        }

        private void CreateGroundBand()
        {
            // main band
            var band = NewSprite("GroundBand",
                SpriteFactory.SolidRounded(GameConfig.GroundColor, 64, 64, 0),
                sortingOrder: 1);
            float bandHeight = (GameConfig.GroundY + 5f); // from ground line down to below screen
            band.transform.position = new Vector3(0f, GameConfig.GroundY - 2.5f, 0f);
            band.transform.localScale = new Vector3(_width + 4f, 5.2f, 1f);

            // darker shadow strip just under the ground line
            var strip = NewSprite("GroundLine",
                SpriteFactory.SolidRounded(GameConfig.GroundShadow, 64, 64, 0),
                sortingOrder: 2);
            strip.transform.position = new Vector3(0f, GameConfig.GroundY - 0.12f, 0f);
            strip.transform.localScale = new Vector3(_width + 4f, 0.22f, 1f);
        }

        private void CreateTufts()
        {
            var tuftSprite = SpriteFactory.SolidRounded(GameConfig.GroundShadow, 40, 26, 12);
            for (float x = spanLeft; x <= spanRight; x += tuftSpacing)
            {
                var t = NewSprite("Tuft", tuftSprite, sortingOrder: 3);
                t.transform.position = new Vector3(x, GameConfig.GroundY - 0.05f, 0f);
                t.transform.localScale = new Vector3(Random.Range(0.8f, 1.3f), Random.Range(0.7f, 1.1f), 1f);
                _tufts.Add(t.transform);
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            float speed = ServiceLocator.Get<DistanceManager>()?.CurrentSpeed ?? GameConfig.StartScrollSpeed;

            foreach (var t in _tufts)
            {
                var p = t.position;
                p.x -= speed * Time.deltaTime;
                if (p.x < spanLeft) p.x += _width;
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
