using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using PuppyForMom.Systems;

namespace PuppyForMom.Core
{
    /// <summary>
    /// Lives in the Boot scene. Spawns the persistent service objects exactly once,
    /// applies global settings (portrait, frame rate) and hands off to the Main Menu.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private float minBootSeconds = 0.6f;

        private static bool _servicesCreated;

        /// <summary>
        /// Safety net so the game is testable when you press Play from ANY scene
        /// (e.g. opening Gameplay directly in the Editor), not only from Boot.
        /// Runs before the first scene loads and creates the persistent services once.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            ApplyGlobalSettings();
            EnsureServices();
        }

        private IEnumerator Start()
        {
            ApplyGlobalSettings();
            EnsureServices();

            float t = 0f;
            while (t < minBootSeconds) { t += Time.unscaledDeltaTime; yield return null; }

            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        private static void ApplyGlobalSettings()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private static void EnsureServices()
        {
            if (_servicesCreated) return;
            _servicesCreated = true;

            var root = new GameObject("~GameServices");
            DontDestroyOnLoad(root);

            // Order matters a little: save first, then systems that read it.
            root.AddComponent<SaveManager>();
            root.AddComponent<AudioManager>();
            root.AddComponent<AdManager>();
            root.AddComponent<ScoreManager>();
            root.AddComponent<DistanceManager>();
            root.AddComponent<GameManager>();
        }
    }
}
