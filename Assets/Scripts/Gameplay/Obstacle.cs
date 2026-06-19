using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;

namespace PuppyForMom.Gameplay
{
    public enum ObstacleType { Car, Puddle, Bin, Fence, Cone, HighBar }

    /// <summary>
    /// A hazard that scrolls left toward the puppy. Touching it ends the run
    /// (handled in PlayerController). Recycled/destroyed once off-screen.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public ObstacleType Type { get; private set; }

        public void Init(ObstacleType type) => Type = type;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            float speed = ServiceLocator.Get<DistanceManager>()?.CurrentSpeed ?? GameConfig.StartScrollSpeed;
            transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
            if (transform.position.x < GameConfig.DespawnX)
                Destroy(gameObject);
        }
    }
}
