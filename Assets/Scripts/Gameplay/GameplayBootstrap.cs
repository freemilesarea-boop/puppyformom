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
        private const float PlayerX = -3.2f;

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
            cam.orthographicSize = 6f;          // portrait: ~12 units tall
            cam.backgroundColor = GameConfig.SkyBottom;
            cam.transform.position = new Vector3(0f, 0.5f, -10f);
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
            var save = ServiceLocator.Get<SaveManager>();
            string skin = save != null ? save.SelectedSkin : "Loui";
            Color color = skin == "Ver" ? GameConfig.PuppyBlack : GameConfig.PuppyCream;

            var go = new GameObject("Puppy");
            go.transform.position = new Vector3(PlayerX, GameConfig.GroundY, 0f);
            go.transform.localScale = Vector3.one * 1.1f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Puppy(color);
            sr.sortingOrder = 10;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.42f;
            col.isTrigger = false;

            go.AddComponent<PlayerController>();
        }
    }
}
