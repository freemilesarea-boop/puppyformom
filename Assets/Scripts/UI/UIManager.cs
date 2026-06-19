using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Player;
using PuppyForMom.Utils;

namespace PuppyForMom.UI
{
    /// <summary>
    /// Builds and drives the in-game HUD and overlays (ready prompt, milestone toast,
    /// cutscene card, pause panel, game-over panel with rewarded-ad revive) all in code.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private Text _distanceText, _scoreText, _bonesText, _toastText;
        private GameObject _readyPanel, _toastPanel, _cutscenePanel, _pausePanel, _gameOverPanel;
        private Text _cutsceneText, _gameOverStats;
        private Button _reviveButton;
        private bool _revivedThisRun;
        private Coroutine _toastRoutine;

        private GameManager GM => GameManager.Instance;

        private void Start()
        {
            BuildUI();
            Subscribe();
            RefreshAll();
            // Sync panels to the current state in case PrepareRun ran before we subscribed.
            if (GM != null) OnStateChanged(GM.State);
        }

        private void OnDestroy() => Unsubscribe();

        // ---------------- Build ----------------
        private void BuildUI()
        {
            var canvas = UIBuilder.CreateCanvas("HUD_Canvas").transform;

            // Top HUD bar
            var bar = UIBuilder.CreatePanel(canvas, new Color(0f, 0f, 0f, 0.18f),
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -150), new Vector2(0, 0));
            bar.rectTransform.pivot = new Vector2(0.5f, 1f);

            _distanceText = UIBuilder.CreateLabel(canvas, "0 m", 60, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -78), new Vector2(500, 90));
            _scoreText = UIBuilder.CreateLabel(canvas, "Score 0", 44, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190, -78), new Vector2(360, 80),
                TextAnchor.MiddleLeft);
            _bonesText = UIBuilder.CreateLabel(canvas, "0", 44, new Color(1f, 0.97f, 0.85f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-150, -78), new Vector2(300, 80),
                TextAnchor.MiddleRight);

            // bone icon next to bone count
            var icon = UIBuilder.AddRect(canvas, "BoneIcon");
            icon.anchorMin = icon.anchorMax = new Vector2(1f, 1f);
            icon.pivot = new Vector2(0.5f, 0.5f);
            icon.anchoredPosition = new Vector2(-300, -78);
            icon.sizeDelta = new Vector2(70, 40);
            var iconImg = icon.gameObject.AddComponent<Image>();
            iconImg.sprite = AssetLoader.Get(ArtKeys.UiBoneIcon, () => SpriteFactory.Bone());
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Pause button
            UIBuilder.CreateButton(canvas, "II", new Color(1f, 1f, 1f, 0.35f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-90, -230), new Vector2(120, 120),
                () => { Sfx_Button(); GM?.Pause(); }, 50);

            // Ready prompt
            _readyPanel = BuildCenterCard(canvas, out var readyText, new Color(0, 0, 0, 0.0f));
            readyText.text = "탭하여 시작!\nTap to start";
            readyText.fontSize = 64;

            // Milestone toast
            _toastPanel = BuildToast(canvas, out _toastText);
            _toastPanel.SetActive(false);

            // Cutscene card
            _cutscenePanel = BuildCutscene(canvas, out _cutsceneText);
            _cutscenePanel.SetActive(false);

            // Pause panel
            _pausePanel = BuildPausePanel(canvas);
            _pausePanel.SetActive(false);

            // Game over panel
            _gameOverPanel = BuildGameOverPanel(canvas);
            _gameOverPanel.SetActive(false);
        }

        private GameObject BuildCenterCard(Transform canvas, out Text label, Color bg)
        {
            var panel = UIBuilder.CreatePanel(canvas, bg, Vector2.zero, Vector2.one);
            label = UIBuilder.CreateLabel(panel.transform, "", 64, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(900, 400));
            return panel.gameObject;
        }

        private GameObject BuildToast(Transform canvas, out Text label)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.2f, 0.15f, 0.1f, 0.65f),
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f));
            panel.rectTransform.sizeDelta = new Vector2(800, 150);
            panel.rectTransform.anchoredPosition = Vector2.zero;
            label = UIBuilder.CreateText(panel.transform, "", 52, Color.white);
            return panel.gameObject;
        }

        private GameObject BuildCutscene(Transform canvas, out Text label)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.05f, 0.05f, 0.1f, 0.82f),
                Vector2.zero, Vector2.one);
            label = UIBuilder.CreateLabel(panel.transform,
                "엄마 냄새가 점점 진해져...\n조금만 더 가면 만날 수 있어!", 56, Color.white,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(950, 400));
            UIBuilder.CreateButton(panel.transform, "계속하기", new Color(0.95f, 0.7f, 0.45f), Color.white,
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(520, 150),
                () => { Sfx_Button(); GM?.ResumeFromCutscene(); });
            return panel.gameObject;
        }

        private GameObject BuildPausePanel(Transform canvas)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.05f, 0.05f, 0.1f, 0.75f),
                Vector2.zero, Vector2.one);
            UIBuilder.CreateLabel(panel.transform, "일시정지", 72, Color.white,
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(700, 150));
            UIBuilder.CreateButton(panel.transform, "계속", new Color(0.55f, 0.8f, 0.55f), Color.white,
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(520, 150),
                () => { Sfx_Button(); GM?.Resume(); });
            UIBuilder.CreateButton(panel.transform, "다시하기", new Color(0.95f, 0.7f, 0.45f), Color.white,
                new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(520, 150),
                () => { Sfx_Button(); GM?.Restart(); });
            UIBuilder.CreateButton(panel.transform, "메인 메뉴", new Color(0.7f, 0.7f, 0.8f), Color.white,
                new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.29f), Vector2.zero, new Vector2(520, 150),
                () => { Sfx_Button(); GM?.GoToMainMenu(); });
            return panel.gameObject;
        }

        private GameObject BuildGameOverPanel(Transform canvas)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.05f, 0.05f, 0.1f, 0.8f),
                Vector2.zero, Vector2.one);
            UIBuilder.CreateLabel(panel.transform, "앗, 부딪혔어!", 70, new Color(1f, 0.85f, 0.7f),
                new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(900, 150));
            _gameOverStats = UIBuilder.CreateLabel(panel.transform, "", 46, Color.white,
                new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(900, 250));

            _reviveButton = UIBuilder.CreateButton(panel.transform, "광고 보고 부활 >", new Color(0.95f, 0.55f, 0.55f), Color.white,
                new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(640, 150),
                OnReviveClicked, 42);
            UIBuilder.CreateButton(panel.transform, "다시하기", new Color(0.95f, 0.7f, 0.45f), Color.white,
                new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), Vector2.zero, new Vector2(520, 150),
                () => { Sfx_Button(); GM?.Restart(); });
            UIBuilder.CreateButton(panel.transform, "메인 메뉴", new Color(0.7f, 0.7f, 0.8f), Color.white,
                new Vector2(0.5f, 0.19f), new Vector2(0.5f, 0.19f), Vector2.zero, new Vector2(520, 150),
                () => { Sfx_Button(); GM?.GoToMainMenu(); });
            return panel.gameObject;
        }

        // ---------------- Events ----------------
        // Cache the persistent managers so we can unsubscribe the exact same delegates
        // in OnDestroy. (These managers survive scene loads, so leaking handlers here
        // would accumulate across every Restart.)
        private ScoreManager _score;
        private DistanceManager _distance;

        private void Subscribe()
        {
            if (GM != null)
            {
                GM.OnStateChanged += OnStateChanged;
                GM.OnMilestone += OnMilestone;
            }
            _score = ServiceLocator.Get<ScoreManager>();
            if (_score != null)
            {
                _score.OnScoreChanged += HandleScoreChanged;
                _score.OnBonesChanged += HandleBonesChanged;
            }
            _distance = ServiceLocator.Get<DistanceManager>();
            if (_distance != null)
            {
                _distance.OnMetersChanged += HandleMetersChanged;
                _distance.OnLevelUp += HandleLevelUp;
            }
        }

        private void Unsubscribe()
        {
            if (GM != null)
            {
                GM.OnStateChanged -= OnStateChanged;
                GM.OnMilestone -= OnMilestone;
            }
            if (_score != null)
            {
                _score.OnScoreChanged -= HandleScoreChanged;
                _score.OnBonesChanged -= HandleBonesChanged;
            }
            if (_distance != null)
            {
                _distance.OnMetersChanged -= HandleMetersChanged;
                _distance.OnLevelUp -= HandleLevelUp;
            }
        }

        private void HandleScoreChanged(int s) { if (_scoreText) _scoreText.text = $"Score {s}"; }
        private void HandleBonesChanged(int b) { if (_bonesText) _bonesText.text = b.ToString(); }

        private void HandleMetersChanged(int m)
        {
            if (!_distanceText) return;
            int level = _distance != null ? _distance.Level : 0;
            _distanceText.text = $"{m} m  ·  Lv.{level}";
        }

        private void HandleLevelUp(int level)
        {
            ServiceLocator.Get<AudioManager>()?.Play(Sfx.Milestone);
            ShowToast($"Lv.{level}");
        }

        private void OnStateChanged(GameState s)
        {
            _readyPanel.SetActive(s == GameState.Ready);
            _pausePanel.SetActive(s == GameState.Paused);
            _cutscenePanel.SetActive(s == GameState.Cutscene);
            if (s == GameState.GameOver) ShowGameOver();
            else _gameOverPanel.SetActive(false);
        }

        private void OnMilestone(int meters)
        {
            string msg = GameConfig.MilestoneMessage(meters);
            if (!string.IsNullOrEmpty(msg) && meters < GameConfig.CutsceneDistance)
            {
                ServiceLocator.Get<AudioManager>()?.Play(Sfx.Milestone);
                ShowToast(msg);
            }
        }

        // ---------------- Helpers ----------------
        private void ShowToast(string msg)
        {
            if (_toastRoutine != null) StopCoroutine(_toastRoutine);
            _toastRoutine = StartCoroutine(ToastRoutine(msg));
        }

        private IEnumerator ToastRoutine(string msg)
        {
            _toastText.text = msg;
            _toastPanel.SetActive(true);
            yield return new WaitForSecondsRealtime(2.0f);
            _toastPanel.SetActive(false);
        }

        private void ShowGameOver()
        {
            var score = ServiceLocator.Get<ScoreManager>();
            var dist = ServiceLocator.Get<DistanceManager>();
            var save = ServiceLocator.Get<SaveManager>();
            int s = score != null ? score.Score : 0;
            int m = dist != null ? dist.Meters : 0;
            int best = save != null ? save.HighScore : 0;
            _gameOverStats.text = $"거리  {m} m\n점수  {s}\n최고점수  {best}";

            _reviveButton.gameObject.SetActive(!_revivedThisRun);
            _gameOverPanel.SetActive(true);

            // After showing game over, queue a (mock) interstitial per cadence rules.
            ServiceLocator.Get<AdManager>()?.NotifyGameFinishedAndMaybeShowInterstitial();
        }

        private void OnReviveClicked()
        {
            Sfx_Button();
            var ads = ServiceLocator.Get<AdManager>();
            if (ads == null) return;
            ads.ShowRewardedAd(() =>
            {
                _revivedThisRun = true;
                _gameOverPanel.SetActive(false);
                var player = FindFirstObjectByType<PlayerController>();
                player?.Revive();
                GM?.Revive();
            });
        }

        private void RefreshAll()
        {
            var score = ServiceLocator.Get<ScoreManager>();
            var dist = ServiceLocator.Get<DistanceManager>();
            if (_scoreText) _scoreText.text = $"Score {(score != null ? score.Score : 0)}";
            if (_bonesText) _bonesText.text = (score != null ? score.Bones : 0).ToString();
            if (_distanceText) _distanceText.text = $"{(dist != null ? dist.Meters : 0)} m  ·  Lv.{(dist != null ? dist.Level : 0)}";
        }

        private void Sfx_Button() => ServiceLocator.Get<AudioManager>()?.Play(Sfx.Button);
    }
}
