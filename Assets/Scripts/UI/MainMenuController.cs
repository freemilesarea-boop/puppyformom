using UnityEngine;
using UnityEngine.UI;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.UI
{
    /// <summary>
    /// Builds the Main Menu: Play / Shop / Collection / Settings, plus a cute decorated
    /// background and best-score readout. Sub-panels (shop/collection/settings) are simple
    /// in-code overlays wired to the mock monetization + save systems.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private GameObject _shopPanel, _collectionPanel, _settingsPanel;

        private void Start()
        {
            SetupSceneBackground();
            BuildMenu();
        }

        private void SetupSceneBackground()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6f;
                cam.backgroundColor = GameConfig.SkyBottom;
            }

            // sky gradient + a big happy puppy + clouds
            MakeSprite("MenuSky", SpriteFactory.VerticalGradient(GameConfig.SkyTop, GameConfig.SkyBottom),
                new Vector3(0, 0, 0), new Vector3(26, 16, 1), -20);
            MakeSprite("MenuGround", SpriteFactory.SolidRounded(GameConfig.GroundColor, 64, 64, 0),
                new Vector3(0, -6.5f, 0), new Vector3(28, 6, 1), -5);
            MakeSprite("MenuPuppy", SpriteFactory.Puppy(GameConfig.PuppyCream),
                new Vector3(0, -2.2f, 0), new Vector3(3.6f, 3.6f, 1), 1);
            MakeSprite("MenuCloud1", SpriteFactory.Cloud(), new Vector3(-4.5f, 3.5f, 0), Vector3.one * 1.4f, -15);
            MakeSprite("MenuCloud2", SpriteFactory.Cloud(), new Vector3(4f, 4.5f, 0), Vector3.one, -15);
        }

        private void BuildMenu()
        {
            var canvas = UIBuilder.CreateCanvas("Menu_Canvas").transform;

            UIBuilder.CreateLabel(canvas, "Puppy For Mom", 96, new Color(0.45f, 0.32f, 0.25f),
                new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), Vector2.zero, new Vector2(1000, 200));
            UIBuilder.CreateLabel(canvas, "엄마를 찾아 떠나는 작은 포메라니안의 여행", 38, new Color(0.5f, 0.4f, 0.32f),
                new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero, new Vector2(1000, 100));

            var save = ServiceLocator.Get<SaveManager>();
            string best = save != null ? $"최고기록  {save.BestDistance} m   ·   최고점수  {save.HighScore}" : "";
            UIBuilder.CreateLabel(canvas, best, 34, new Color(0.5f, 0.42f, 0.35f),
                new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(1000, 80));

            // Primary PLAY button
            UIBuilder.CreateButton(canvas, "PLAY", new Color(0.96f, 0.66f, 0.42f), Color.white,
                new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), Vector2.zero, new Vector2(640, 180),
                () => { Click(); GameManager.Instance?.StartGameplay(false); }, 72);

            // Endless mode (only if unlocked)
            if (save != null && save.EndlessUnlocked)
            {
                UIBuilder.CreateButton(canvas, "무한 모드 GO", new Color(0.6f, 0.78f, 0.95f), Color.white,
                    new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(520, 130),
                    () => { Click(); GameManager.Instance?.StartGameplay(true); }, 48);
            }

            // Secondary buttons row: Shop / Collection / Settings
            UIBuilder.CreateButton(canvas, "상점", new Color(0.7f, 0.82f, 0.6f), Color.white,
                new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(-320, 0), new Vector2(280, 120),
                () => { Click(); _shopPanel.SetActive(true); }, 44);
            UIBuilder.CreateButton(canvas, "도감", new Color(0.7f, 0.82f, 0.6f), Color.white,
                new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(0, 0), new Vector2(280, 120),
                () => { Click(); _collectionPanel.SetActive(true); }, 44);
            UIBuilder.CreateButton(canvas, "설정", new Color(0.7f, 0.82f, 0.6f), Color.white,
                new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(320, 0), new Vector2(280, 120),
                () => { Click(); _settingsPanel.SetActive(true); }, 44);

            _shopPanel = BuildShop(canvas);
            _collectionPanel = BuildCollection(canvas);
            _settingsPanel = BuildSettings(canvas);
            _shopPanel.SetActive(false);
            _collectionPanel.SetActive(false);
            _settingsPanel.SetActive(false);
        }

        private GameObject BuildShop(Transform canvas)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.1f, 0.08f, 0.12f, 0.9f), Vector2.zero, Vector2.one);
            UIBuilder.CreateLabel(panel.transform, "상점", 72, Color.white,
                new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(600, 120));
            UIBuilder.CreateLabel(panel.transform, "Pay-To-Win 없음 · 광고 제거와 스킨만 판매", 32,
                new Color(0.8f, 0.8f, 0.85f),
                new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.83f), Vector2.zero, new Vector2(900, 80));

            var save = ServiceLocator.Get<SaveManager>();
            UIBuilder.CreateButton(panel.transform,
                save != null && save.RemoveAds ? "광고 제거됨 ✓" : "광고 제거 ($2.99)",
                new Color(0.55f, 0.8f, 0.55f), Color.white,
                new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(720, 140),
                () => { Click(); MockPurchaseRemoveAds(); }, 42);
            UIBuilder.CreateButton(panel.transform, "스킨 팩 ($3.99)", new Color(0.8f, 0.7f, 0.95f), Color.white,
                new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.53f), Vector2.zero, new Vector2(720, 140),
                () => { Click(); Debug.Log("[IAP MOCK] Skin Pack purchased."); }, 42);
            UIBuilder.CreateButton(panel.transform, "퍼피 번들 ($6.99)", new Color(0.95f, 0.75f, 0.5f), Color.white,
                new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(720, 140),
                () => { Click(); Debug.Log("[IAP MOCK] Puppy Bundle purchased."); }, 42);

            UIBuilder.CreateButton(panel.transform, "닫기", new Color(0.7f, 0.7f, 0.8f), Color.white,
                new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(420, 130),
                () => { Click(); panel.gameObject.SetActive(false); });
            return panel.gameObject;
        }

        private GameObject BuildCollection(Transform canvas)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.1f, 0.08f, 0.12f, 0.9f), Vector2.zero, Vector2.one);
            UIBuilder.CreateLabel(panel.transform, "강아지 도감", 72, Color.white,
                new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(700, 120));

            var save = ServiceLocator.Get<SaveManager>();
            bool endless = save != null && save.EndlessUnlocked;
            string[] names = { "Loui (크림)", "Ver (블랙)", "Astronaut", "Golden", "Angel", "Rainbow" };
            Color[] cols = { GameConfig.PuppyCream, GameConfig.PuppyBlack, new Color(0.7f,0.75f,0.85f),
                new Color(0.95f,0.82f,0.4f), new Color(0.95f,0.95f,1f), new Color(0.9f,0.6f,0.9f) };
            bool[] unlocked = { true, endless, false, false, false, false };

            for (int i = 0; i < names.Length; i++)
            {
                int row = i / 3, col = i % 3;
                float x = -360 + col * 360;
                float y = 0.62f - row * 0.22f;
                var cardImg = UIBuilder.CreatePanel(panel.transform,
                    unlocked[i] ? new Color(1f, 1f, 1f, 0.12f) : new Color(0f, 0f, 0f, 0.25f),
                    new Vector2(0.5f, y), new Vector2(0.5f, y));
                cardImg.rectTransform.sizeDelta = new Vector2(320, 320);
                cardImg.rectTransform.anchoredPosition = new Vector2(x, 0);

                var puppyImg = UIBuilder.AddRect(cardImg.transform, "P");
                puppyImg.anchorMin = puppyImg.anchorMax = new Vector2(0.5f, 0.6f);
                puppyImg.sizeDelta = new Vector2(180, 180);
                var im = puppyImg.gameObject.AddComponent<Image>();
                im.sprite = SpriteFactory.Puppy(cols[i]);
                im.preserveAspect = true;
                im.color = unlocked[i] ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);

                UIBuilder.CreateLabel(cardImg.transform, unlocked[i] ? names[i] : "??? (잠김)", 30, Color.white,
                    new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(300, 60));
            }

            UIBuilder.CreateButton(panel.transform, "닫기", new Color(0.7f, 0.7f, 0.8f), Color.white,
                new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(420, 130),
                () => { Click(); panel.gameObject.SetActive(false); });
            return panel.gameObject;
        }

        private GameObject BuildSettings(Transform canvas)
        {
            var panel = UIBuilder.CreatePanel(canvas, new Color(0.1f, 0.08f, 0.12f, 0.9f), Vector2.zero, Vector2.one);
            UIBuilder.CreateLabel(panel.transform, "설정", 72, Color.white,
                new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(600, 120));

            var audio = ServiceLocator.Get<AudioManager>();
            Button muteBtn = null;
            muteBtn = UIBuilder.CreateButton(panel.transform, MuteText(audio), new Color(0.6f, 0.78f, 0.95f), Color.white,
                new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(640, 140),
                () =>
                {
                    Click();
                    audio?.ToggleMute();
                    var lbl = muteBtn.GetComponentInChildren<Text>();
                    if (lbl != null) lbl.text = MuteText(audio);
                }, 42);

            UIBuilder.CreateButton(panel.transform, "기록 초기화", new Color(0.95f, 0.6f, 0.55f), Color.white,
                new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), Vector2.zero, new Vector2(640, 140),
                () => { Click(); ServiceLocator.Get<SaveManager>()?.ResetAll(); }, 42);

            UIBuilder.CreateLabel(panel.transform, "Puppy For Mom v0.1.0\nMVP Build · Made with Unity", 30,
                new Color(0.75f, 0.75f, 0.8f),
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(800, 120));

            UIBuilder.CreateButton(panel.transform, "닫기", new Color(0.7f, 0.7f, 0.8f), Color.white,
                new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(420, 130),
                () => { Click(); panel.gameObject.SetActive(false); });
            return panel.gameObject;
        }

        private static string MuteText(AudioManager a) => a != null && a.Muted ? "소리 켜기" : "소리 끄기";

        private void MockPurchaseRemoveAds()
        {
            ServiceLocator.Get<SaveManager>()?.SetRemoveAds(true);
            Debug.Log("[IAP MOCK] Remove Ads purchased.");
            _shopPanel.SetActive(false);
        }

        private static void MakeSprite(string name, Sprite sprite, Vector3 pos, Vector3 scale, int order)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
        }

        private static void Click() => ServiceLocator.Get<AudioManager>()?.Play(Sfx.Button);
    }
}
