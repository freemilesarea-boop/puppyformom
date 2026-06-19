using UnityEngine;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Player;
using PuppyForMom.UI;
using PuppyForMom.Utils;

namespace PuppyForMom.Gameplay
{
    /// <summary>
    /// Assembles the entire playable Gameplay scene in code: camera, parallax background,
    /// scrolling ground, the puppy, the spawner and the HUD. Then tells the GameManager to
    /// prepare a fresh run. This is the only authored object the Gameplay scene needs.
    /// </summary>
    public class GameplayBootstrap : MonoBehaviour
    {
        // Left-of-centre so obstacles (coming from the right) give good reaction time,
        // while staying well inside the narrow portrait view.
        private const float PlayerX = -1.6f;

        private void Awake()
        {
            ConfigureCamera();
            BuildWorld();
        }

        private void Start()
        {
            // Defer run prep to Start so all managers/HUD have run their own Start/Awake.
            GameManager.Instance?.PrepareRun();
        }

        private void Update()
        {
            // Android back button / Esc toggles pause.
            if (Input.GetKeyDown(KeyCode.Escape))
                GameManager.Instance?.TogglePause();
        }

        private void ConfigureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 5.6f;        // portrait framing: dog + ground clearly visible
            cam.backgroundColor = GameConfig.SkyBottom;
            cam.transform.position = new Vector3(0f, 0.4f, -10f);
        }

        private void BuildWorld()
        {
            new GameObject("Parallax").AddComponent<ParallaxBackground>();
            new GameObject("Ground").AddComponent<GroundScroller>();
            new GameObject("Spawner").AddComponent<ObstacleSpawner>();

            CreatePlayer();

            new GameObject("UIManager").AddComponent<UIManager>();
        }

        private void CreatePlayer()
        {
            // PlayerController loads its own (real or placeholder) sprites, normalises its size
            // so the full body is visible, and fits the capsule collider to the art.
            var go = new GameObject("Puppy");
            go.transform.position = new Vector3(PlayerX, GameConfig.GroundY, 0f);

            go.AddComponent<SpriteRenderer>();
            go.AddComponent<CapsuleCollider2D>();   // sized by PlayerController
            go.AddComponent<PlayerController>();
        }
    }
}
