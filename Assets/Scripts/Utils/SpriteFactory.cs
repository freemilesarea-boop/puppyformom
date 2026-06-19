using UnityEngine;
using PuppyForMom.Core;

namespace PuppyForMom.Utils
{
    /// <summary>
    /// Generates all placeholder art at runtime as <see cref="Sprite"/>s so the MVP needs
    /// zero imported image files. Everything is simple shapes in the warm pastel palette.
    /// Swap these calls for real imported sprites when art is ready.
    /// </summary>
    public static class SpriteFactory
    {
        private const int PPU = 100; // pixels-per-unit for generated sprites

        private static Sprite FromTexture(Texture2D tex)
        {
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), PPU);
        }

        private static Texture2D NewTex(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var clear = new Color(0, 0, 0, 0);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            tex.SetPixels(px);
            return tex;
        }

        // ---------- primitive draw helpers ----------
        private static void FillRoundedRect(Texture2D tex, int x0, int y0, int w, int h, int radius, Color c)
        {
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (InsideRounded(x, y, w, h, radius))
                    tex.SetPixel(x0 + x, y0 + y, c);
            }
        }

        private static bool InsideRounded(int x, int y, int w, int h, int r)
        {
            if (r <= 0) return true;
            int cxL = r, cxR = w - 1 - r, cyB = r, cyT = h - 1 - r;
            int dx = 0, dy = 0;
            if (x < cxL) dx = cxL - x; else if (x > cxR) dx = x - cxR;
            if (y < cyB) dy = cyB - y; else if (y > cyT) dy = y - cyT;
            return dx * dx + dy * dy <= r * r;
        }

        private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color c)
        {
            for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y <= r * r)
                    tex.SetPixel(cx + x, cy + y, c);
            }
        }

        private static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 cc, Color col)
        {
            int minX = Mathf.FloorToInt(Mathf.Min(a.x, b.x, cc.x));
            int maxX = Mathf.CeilToInt(Mathf.Max(a.x, b.x, cc.x));
            int minY = Mathf.FloorToInt(Mathf.Min(a.y, b.y, cc.y));
            int maxY = Mathf.CeilToInt(Mathf.Max(a.y, b.y, cc.y));
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (PointInTri(p, a, b, cc)) tex.SetPixel(x, y, col);
            }
        }

        private static bool PointInTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b), d2 = Sign(p, b, c), d3 = Sign(p, c, a);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0;
            bool pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) =>
            (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

        // ---------- public sprite builders ----------
        public static Sprite SolidRounded(Color c, int w, int h, int radius)
        {
            var tex = NewTex(w, h);
            FillRoundedRect(tex, 0, 0, w, h, radius, c);
            return FromTexture(tex);
        }

        public static Sprite Solid(Color c, int w = 8, int h = 8)
        {
            var tex = NewTex(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);
            return FromTexture(tex);
        }

        /// <summary>A cute round puppy face with ears, eyes, nose. ~1.2 world units tall.</summary>
        public static Sprite Puppy(Color body)
        {
            int s = 120;
            var tex = NewTex(s, s);
            Color dark = GameConfig.PuppyBlack;
            Color ear = Color.Lerp(body, dark, 0.25f);

            // ears (rounded triangles)
            FillTriangle(tex, new Vector2(18, 108), new Vector2(8, 60), new Vector2(46, 84), ear);
            FillTriangle(tex, new Vector2(102, 108), new Vector2(112, 60), new Vector2(74, 84), ear);
            // head/body
            FillCircle(tex, s / 2, s / 2 - 6, 46, body);
            // cheeks lighter
            FillCircle(tex, 44, 52, 10, Color.Lerp(body, Color.white, 0.4f));
            FillCircle(tex, 76, 52, 10, Color.Lerp(body, Color.white, 0.4f));
            // eyes
            FillCircle(tex, 48, 70, 7, dark);
            FillCircle(tex, 72, 70, 7, dark);
            FillCircle(tex, 50, 72, 2, Color.white);
            FillCircle(tex, 74, 72, 2, Color.white);
            // nose
            FillCircle(tex, 60, 54, 6, dark);
            return FromTexture(tex);
        }

        /// <summary>A simple bone collectible.</summary>
        public static Sprite Bone()
        {
            int w = 60, h = 32;
            var tex = NewTex(w, h);
            Color c = GameConfig.BoneColor;
            Color edge = Color.Lerp(c, Color.gray, 0.25f);
            FillRoundedRect(tex, 14, 6, 32, 20, 4, c);     // shaft
            FillCircle(tex, 12, 9, 8, c); FillCircle(tex, 12, 23, 8, c);
            FillCircle(tex, 48, 9, 8, c); FillCircle(tex, 48, 23, 8, c);
            // subtle outline dots
            FillCircle(tex, 12, 9, 8, edge); FillCircle(tex, 12, 9, 6, c);
            return FromTexture(tex);
        }

        /// <summary>"Mom's scent" cloud collectible.</summary>
        public static Sprite Smell()
        {
            int s = 56; var tex = NewTex(s, s);
            Color c = GameConfig.SmellColor;
            FillCircle(tex, 20, 24, 12, c);
            FillCircle(tex, 34, 28, 14, c);
            FillCircle(tex, 30, 16, 10, c);
            FillCircle(tex, 24, 30, 9, Color.Lerp(c, Color.white, 0.5f));
            return FromTexture(tex);
        }

        /// <summary>A photo-fragment collectible (unlocks ending).</summary>
        public static Sprite PhotoPiece()
        {
            int s = 48; var tex = NewTex(s, s);
            Color c = GameConfig.PhotoColor;
            FillRoundedRect(tex, 6, 6, s - 12, s - 12, 4, Color.white);
            FillRoundedRect(tex, 10, 10, s - 20, s - 20, 3, c);
            FillCircle(tex, 20, 20, 4, Color.white);
            return FromTexture(tex);
        }

        public static Sprite Circle(Color c, int diameter)
        {
            var tex = NewTex(diameter, diameter);
            FillCircle(tex, diameter / 2, diameter / 2, diameter / 2 - 1, c);
            return FromTexture(tex);
        }

        /// <summary>Vertical gradient sprite used for the sky background.</summary>
        public static Sprite VerticalGradient(Color top, Color bottom, int w = 64, int h = 256)
        {
            var tex = NewTex(w, h);
            for (int y = 0; y < h; y++)
            {
                Color row = Color.Lerp(bottom, top, (float)y / (h - 1));
                for (int x = 0; x < w; x++) tex.SetPixel(x, y, row);
            }
            return FromTexture(tex);
        }

        /// <summary>A fluffy cloud for the parallax background.</summary>
        public static Sprite Cloud()
        {
            int w = 120, h = 60; var tex = NewTex(w, h);
            Color c = new Color(1f, 1f, 1f, 0.9f);
            FillCircle(tex, 36, 26, 20, c);
            FillCircle(tex, 64, 32, 26, c);
            FillCircle(tex, 92, 24, 18, c);
            FillRoundedRect(tex, 20, 8, 80, 18, 8, c);
            return FromTexture(tex);
        }
    }
}
