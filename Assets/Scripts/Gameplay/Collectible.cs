using System.Collections;
using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;

namespace PuppyForMom.Gameplay
{
    public enum CollectibleType { Bone, Smell, Photo }

    /// <summary>
    /// A treat that scrolls left. Bones add score, the scent advances the story flavour,
    /// photo pieces count toward the ending. Collected on contact with the puppy.
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        public CollectibleType Type { get; private set; }
        private bool _collected;

        public void Init(CollectibleType type) => Type = type;

        private void Update()
        {
            if (_collected) return;
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            float speed = ServiceLocator.Get<DistanceManager>()?.CurrentSpeed ?? GameConfig.StartScrollSpeed;
            transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
            // little floating bob
            var p = transform.position;
            p.y += Mathf.Sin(Time.time * 4f + p.x) * 0.003f;
            transform.position = p;

            if (transform.position.x < GameConfig.DespawnX)
                Destroy(gameObject);
        }

        public void Collect()
        {
            if (_collected) return;
            _collected = true;

            var score = ServiceLocator.Get<ScoreManager>();
            switch (Type)
            {
                case CollectibleType.Bone: score?.AddBones(1); break;
                case CollectibleType.Smell: score?.AddSmell(); break;
                case CollectibleType.Photo: score?.AddPhotoPiece(); break;
            }
            ServiceLocator.Get<AudioManager>()?.Play(Sfx.Collect);

            // disable collider immediately, then a quick pop-out before destroy
            foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
            StartCoroutine(PopAndDie());
        }

        private IEnumerator PopAndDie()
        {
            float t = 0f;
            Vector3 start = transform.localScale;
            while (t < 0.12f)
            {
                t += Time.deltaTime;
                float k = 1f + t / 0.12f * 0.6f;
                transform.localScale = start * k;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
