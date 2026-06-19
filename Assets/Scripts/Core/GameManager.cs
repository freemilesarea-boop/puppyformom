using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using PuppyForMom.Systems;

namespace PuppyForMom.Core
{
    /// <summary>
    /// The brain of the game. Persists across scenes, owns the <see cref="GameState"/>
    /// machine and coordinates the run lifecycle: ready -> playing -> (cutscene) -> game over / ending.
    /// Gameplay objects register themselves each time the Gameplay scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public bool EndlessMode { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnMilestone;       // metres
        public event Action OnRunStarted;
        public event Action OnGameOver;

        private DistanceManager _distance;
        private ScoreManager _score;
        private DistanceManager Distance => _distance ??= ServiceLocator.Get<DistanceManager>();
        private ScoreManager Score => _score ??= ServiceLocator.Get<ScoreManager>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        // ---------------- State helpers ----------------
        private void SetState(GameState s)
        {
            if (State == s) return;
            State = s;
            OnStateChanged?.Invoke(s);
        }

        public bool IsPlaying => State == GameState.Playing;

        // ---------------- Scene flow ----------------
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        public void StartGameplay(bool endless = false)
        {
            EndlessMode = endless;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.Gameplay);
        }

        /// <summary>Called by GameplayBootstrap once the world exists. Wires milestone events.</summary>
        public void PrepareRun()
        {
            Time.timeScale = 1f;
            Score?.ResetRun();
            Distance?.ResetRun();

            if (Distance != null)
            {
                Distance.OnMilestoneReached -= HandleMilestone;
                Distance.OnMilestoneReached += HandleMilestone;
            }
            SetState(GameState.Ready);
        }

        /// <summary>First tap converts Ready -> Playing.</summary>
        public void BeginPlayingIfReady()
        {
            if (State != GameState.Ready) return;
            Distance?.SetRunning(true);
            SetState(GameState.Playing);
            OnRunStarted?.Invoke();
        }

        // ---------------- Pause ----------------
        public void TogglePause()
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            Time.timeScale = 0f;
            Distance?.SetRunning(false);
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            Distance?.SetRunning(true);
            SetState(GameState.Playing);
        }

        // ---------------- Cutscene ----------------
        private void HandleMilestone(int meters)
        {
            OnMilestone?.Invoke(meters);

            if (meters >= GameConfig.EndingDistance && !EndlessMode)
            {
                ReachEnding();
            }
            else if (meters == GameConfig.CutsceneDistance && !EndlessMode)
            {
                // The HUD shows an inline cutscene card; we briefly pause the run.
                Distance?.SetRunning(false);
                SetState(GameState.Cutscene);
            }
        }

        public void ResumeFromCutscene()
        {
            if (State != GameState.Cutscene) return;
            Distance?.SetRunning(true);
            SetState(GameState.Playing);
        }

        // ---------------- Game over / revive ----------------
        public void PlayerDied()
        {
            if (State == GameState.GameOver || State == GameState.Ending) return;
            Distance?.SetRunning(false);
            SetState(GameState.GameOver);

            var save = ServiceLocator.Get<SaveManager>();
            if (save != null && Score != null && Distance != null)
            {
                save.RecordRun(Score.Score, Distance.Meters, Score.Bones, Score.PhotoPieces);
            }
            OnGameOver?.Invoke();
        }

        public void Revive()
        {
            if (State != GameState.GameOver) return;
            Distance?.SetRunning(true);
            SetState(GameState.Playing);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.Gameplay);
        }

        // ---------------- Ending ----------------
        private void ReachEnding()
        {
            SetState(GameState.Ending);
            var save = ServiceLocator.Get<SaveManager>();
            if (save != null && Score != null && Distance != null)
            {
                save.RecordRun(Score.Score, Distance.Meters, Score.Bones, Score.PhotoPieces);
                save.SetEndingSeen();
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.Ending);
        }
    }
}
