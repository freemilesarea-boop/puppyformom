using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PuppyForMom.Utils
{
    /// <summary>
    /// Helper for building uGUI screens entirely in code (no prefabs needed for the MVP).
    /// Uses the built-in legacy font so no font asset import is required.
    /// </summary>
    public static class UIBuilder
    {
        private static Font _font;
        public static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _font;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            UnityEngine.Object.DontDestroyOnLoad(es);
        }

        public static RectTransform AddRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Image CreatePanel(Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var rt = AddRect(parent, "Panel");
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text CreateText(Transform parent, string content, int fontSize, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var rt = AddRect(parent, "Text");
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = content;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Creates a labelled, anchored text element positioned with explicit pixel layout.</summary>
        public static Text CreateLabel(Transform parent, string content, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var rt = AddRect(parent, "Label");
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = content;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button CreateButton(Transform parent, string label, Color bg, Color textColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Action onClick, int fontSize = 48)
        {
            var rt = AddRect(parent, $"Button_{label}");
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = rt.gameObject.AddComponent<Image>();
            // Use ui_button.png (9-sliced) when available; the color still tints it. Solid otherwise.
            var btnSprite = AssetLoader.Get(ArtKeys.UiButton, () => (Sprite)null);
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; }
            img.color = bg;
            var btn = rt.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(bg, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(bg, Color.black, 0.12f);
            btn.colors = colors;

            var txt = CreateText(rt, label, fontSize, textColor);
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
