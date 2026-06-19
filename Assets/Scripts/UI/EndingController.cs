using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PuppyForMom.Core;
using PuppyForMom.Systems;
using PuppyForMom.Utils;

namespace PuppyForMom.UI
{
    /// <summary>
    /// The ending: the puppy is reunited with Mom. Plays a short scripted cutscene, shows the
    /// run summary, and confirms that Endless Mode is now unlocked.
    /// </summary>
    public class EndingController : MonoBehaviour
    {
        private Text _storyText;
        private GameObject _buttons;

        private readonly string[] _lines =
        {
            "...킁킁... 이 냄새는...!",
            "저 멀리, 익숙한 실루엣이 보여요.",
            "\"엄마!\"",
            "포메는 있는 힘껏 달려갔어요.",
            "드디어, 엄마를 다시 만났어요."
        };

        private void Start()
        {
            SetupScene();
            BuildUI();
            StartCoroutine(PlayCutscene());
        }

        private void SetupScene()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6f;
                cam.backgroundColor = new Color(1f, 0.85f, 0.7f);
            }

            MakeSprite("Sky",
                AssetLoader.Get(ArtKeys.BgSky, () => SpriteFactory.VerticalGradient(new Color(1f, 0.78f, 0.6f), new Color(1f, 0.93f, 0.82f))),
                new Vector3(0, 0, 0), new Vector3(26, 16, 1), -20);
            MakeSprite("Ground", SpriteFactory.SolidRounded(GameConfig.GroundColor, 64, 64, 0),
                new Vector3(0, -6.5f, 0), new Vector3(28, 6, 1), -5);

            var save = ServiceLocator.Get<SaveManager>();
            string skinName = save != null ? save.SelectedSkin : "Loui";
            Color puppyColor = skinName == "Ver" ? GameConfig.PuppyBlack : GameConfig.PuppyCream;

            // full-body reunion (real PNGs when available)
            MakeSprite("Puppy",
                AssetLoader.Get(ArtKeys.Puppy(skinName, ArtKeys.StateIdle), () => SpriteFactory.PuppyBody(puppyColor, PuppyPose.Idle)),
                new Vector3(-2f, -2.4f, 0), new Vector3(1.8f, 1.8f, 1), 1);
            // "Mom" is a bigger cream puppy
            MakeSprite("Mom",
                AssetLoader.Get(ArtKeys.Puppy("Loui", ArtKeys.StateIdle), () => SpriteFactory.PuppyBody(GameConfig.PuppyCream, PuppyPose.Idle)),
                new Vector3(2f, -2.0f, 0), new Vector3(2.7f, 2.7f, 1), 1);

            for (int i = 0; i < 6; i++)
            {
                MakeSprite("Heart", SpriteFactory.Circle(new Color(1f, 0.5f, 0.6f), 40),
                    new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 4f), 0), Vector3.one * Random.Range(0.3f, 0.7f), 2);
            }
        }

        private void BuildUI()
        {
            var canvas = UIBuilder.CreateCanvas("Ending_Canvas").transform;

            UIBuilder.CreateLabel(canvas, "ENDING", 64, new Color(0.5f, 0.3f, 0.25f),
                new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(800, 120));

            var box = UIBuilder.CreatePanel(canvas, new Color(0f, 0f, 0f, 0.35f),
                new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f));
            box.rectTransform.sizeDelta = new Vector2(1000, 260);
            _storyText = UIBuilder.CreateText(box.transform, "", 48, Color.white);

            _buttons = UIBuilder.CreatePanel(canvas, new Color(0, 0, 0, 0), Vector2.zero, Vector2.one).gameObject;
            _buttons.SetActive(false);

            var save = ServiceLocator.Get<SaveManager>();
            int dist = save != null ? save.BestDistance : 0;
            int score = save != null ? save.HighScore : 0;
            UIBuilder.CreateLabel(_buttons.transform,
                $"여행 거리 {dist} m · 최고 점수 {score}\n무한 모드가 해금되었습니다!", 40, Color.white,
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(1000, 200));

            UIBuilder.CreateButton(_buttons.transform, "무한 모드 GO", new Color(0.6f, 0.78f, 0.95f), Color.white,
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(560, 150),
                () => { Click(); GameManager.Instance?.StartGameplay(true); });
            UIBuilder.CreateButton(_buttons.transform, "메인 메뉴", new Color(0.95f, 0.7f, 0.45f), Color.white,
                new Vector2(0.5f, 0.27f), new Vector2(0.5f, 0.27f), Vector2.zero, new Vector2(560, 150),
                () => { Click(); GameManager.Instance?.GoToMainMenu(); });
        }

        private IEnumerator PlayCutscene()
        {
            ServiceLocator.Get<AudioManager>()?.Play(Sfx.Milestone);
            foreach (var line in _lines)
            {
                _storyText.text = line;
                _storyText.transform.localScale = Vector3.one * 0.9f;
                float t = 0f;
                while (t < 1.6f)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Min(1f, t / 0.25f);
                    _storyText.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, k);
                    yield return null;
                }
            }
            _storyText.text = "축하합니다!";
            _buttons.SetActive(true);
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
