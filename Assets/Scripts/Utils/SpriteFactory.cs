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

        // ---------- full-body side-view puppy (placeholder, replaced by real PNGs) ----------

        private static void DrawLeg(Texture2D tex, int cx, int yBottom, int yTop, Color c)
        {
            FillRoundedRect(tex, cx - 8, yBottom, 16, yTop - yBottom, 6, c);
            FillCircle(tex, cx, yBottom, 8, c); // paw
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color c, int thick = 2)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                FillCircle(tex, x0, y0, thick, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>
        /// A cute full-body Pomeranian in side view (facing right), with legs/ears/tail and a
        /// big expressive eye. Pose changes the legs / expression for idle, 2-frame run, jump, hit.
        /// Drawn so the paws sit at the very bottom of the texture (feet on the ground).
        /// </summary>
        public static Sprite PuppyBody(Color body, PuppyPose pose)
        {
            int W = 200, H = 170;
            var tex = NewTex(W, H);
            Color dark = GameConfig.PuppyBlack;
            Color legC = Color.Lerp(body, dark, 0.18f);
            Color ear = Color.Lerp(body, dark, 0.28f);
            Color fluff = Color.Lerp(body, Color.white, 0.30f);

            bool jump = pose == PuppyPose.Jump;
            bool hit = pose == PuppyPose.Hit;

            // tail (fluffy, at the back/left)
            FillCircle(tex, 42, 100, 20, body);
            FillCircle(tex, 36, 112, 13, fluff);

            // legs: run frames swing them, jump tucks them up
            int legTop = 76;
            int legBot = jump ? 44 : 12;
            int swing = pose == PuppyPose.Run1 ? 16 : pose == PuppyPose.Run2 ? -16 : 0;
            DrawLeg(tex, 74 - swing, legBot, legTop, legC);   // back leg
            DrawLeg(tex, 126 + swing, legBot, legTop, legC);  // front leg

            // body
            FillRoundedRect(tex, 38, 62, 118, 66, 30, body);
            FillCircle(tex, 96, 96, 12, fluff); // chest fluff hint

            // head (front/right)
            FillCircle(tex, 152, 104, 42, body);
            // ear
            FillTriangle(tex, new Vector2(142, 142), new Vector2(124, 100), new Vector2(164, 116), ear);
            // cheek fluff
            FillCircle(tex, 150, 80, 17, fluff);

            // eye + expression
            if (hit)
            {
                DrawLine(tex, 160, 100, 172, 112, dark);
                DrawLine(tex, 172, 100, 160, 112, dark);
            }
            else
            {
                FillCircle(tex, 167, 106, 10, dark);
                FillCircle(tex, 170, 109, 3, Color.white);
            }
            // nose + snout
            FillCircle(tex, 190, 96, 7, dark);

            return FromTexture(tex);
        }

        // ---------- parallax layer strips (seamless-ish placeholders) ----------

        /// <summary>Wide transparent strip with soft clouds, for the cloud parallax layer.</summary>
        public static Sprite CloudStrip()
        {
            int W = 512, H = 200; var tex = NewTex(W, H);
            Color c = new Color(1f, 1f, 1f, 0.9f);
            int[] cx = { 70, 200, 340, 460 };
            int[] cy = { 130, 70, 150, 90 };
            foreach (var i in new[] { 0, 1, 2, 3 })
            {
                FillCircle(tex, cx[i], cy[i], 34, c);
                FillCircle(tex, cx[i] + 34, cy[i] - 8, 26, c);
                FillCircle(tex, cx[i] - 30, cy[i] - 6, 22, c);
            }
            return FromTexture(tex);
        }

        /// <summary>Wide strip of pastel building silhouettes, bottom-aligned, for the city layer.</summary>
        public static Sprite CityStrip()
        {
            int W = 512, H = 200; var tex = NewTex(W, H);
            Color[] cols =
            {
                new Color(0.80f, 0.82f, 0.92f), new Color(0.86f, 0.80f, 0.90f),
                new Color(0.78f, 0.86f, 0.90f), new Color(0.88f, 0.85f, 0.80f)
            };
            int x = 6;
            int idx = 0;
            while (x < W - 10)
            {
                int bw = 48 + (idx * 17) % 40;
                int bh = 70 + (idx * 37) % 95;
                FillRoundedRect(tex, x, 0, bw, bh, 4, cols[idx % cols.Length]);
                // windows
                Color win = new Color(1f, 1f, 1f, 0.5f);
                for (int wy = 14; wy < bh - 12; wy += 18)
                    for (int wx = x + 8; wx < x + bw - 8; wx += 16)
                        FillRoundedRect(tex, wx, wy, 7, 9, 1, win);
                x += bw + 8;
                idx++;
            }
            return FromTexture(tex);
        }

        /// <summary>Wide strip of round pastel trees, bottom-aligned, for the tree layer.</summary>
        public static Sprite TreeStrip()
        {
            int W = 512, H = 170; var tex = NewTex(W, H);
            Color trunk = new Color(0.66f, 0.52f, 0.40f);
            Color leaf1 = new Color(0.70f, 0.84f, 0.58f);
            Color leaf2 = new Color(0.62f, 0.78f, 0.52f);
            int x = 30;
            int idx = 0;
            while (x < W - 20)
            {
                int th = 38 + (idx * 13) % 26;       // trunk height
                int r = 34 + (idx * 11) % 18;        // canopy radius
                FillRoundedRect(tex, x - 7, 0, 14, th, 4, trunk);
                FillCircle(tex, x, th + r - 8, r, (idx % 2 == 0) ? leaf1 : leaf2);
                FillCircle(tex, x - r / 2, th + r - 16, r - 10, Color.Lerp(leaf1, leaf2, 0.5f));
                x += 70 + (idx * 9) % 30;
                idx++;
            }
            return FromTexture(tex);
        }

        /// <summary>Tileable road surface strip (warm grey path) for the ground layer.</summary>
        public static Sprite RoadStrip()
        {
            int W = 256, H = 120; var tex = NewTex(W, H);
            Color road = new Color(0.74f, 0.70f, 0.64f);
            Color dash = new Color(0.92f, 0.90f, 0.84f);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++) tex.SetPixel(x, y, road);
            // dashed center line near the top edge
            for (int x = 10; x < W - 10; x += 64)
                FillRoundedRect(tex, x, H - 26, 36, 8, 3, dash);
            return FromTexture(tex);
        }
    }

    /// <summary>Animation poses for the placeholder/real puppy sprite swap.</summary>
    public enum PuppyPose { Idle, Run1, Run2, Jump, Hit }
}
