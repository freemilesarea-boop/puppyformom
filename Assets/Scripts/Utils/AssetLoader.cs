using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuppyForMom.Utils
{
    /// <summary>
    /// Loads real art (PNG sprites) by name from a Resources folder, with a procedural
    /// <see cref="SpriteFactory"/> fallback so the game ALWAYS renders — even with no art yet.
    ///
    /// Drop PNGs into <c>Assets/Resources/Art/&lt;category&gt;/...</c> and they are picked up
    /// automatically. The loader is resilient to import settings:
    ///   1) tries <see cref="Resources.Load{T}"/> as a <see cref="Sprite"/> (Texture Type = Sprite),
    ///   2) if that fails, loads the raw <see cref="Texture2D"/> and builds a Sprite at runtime
    ///      (so it works even if the PNG was imported as a plain Texture),
    ///   3) otherwise falls back to the procedural placeholder.
    ///
    /// We use Resources (not Assets/Art directly) because <see cref="Resources.Load"/> is the
    /// only built-in way to load by string path at runtime, and it requires a "Resources" folder.
    /// </summary>
    public static class AssetLoader
    {
        public const string ResourcesRoot = "Art";
        private const float DefaultPixelsPerUnit = 100f;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Returns the real sprite at <paramref name="key"/> if present (imported as a Sprite OR
        /// recovered from a raw Texture), otherwise the result of <paramref name="placeholder"/>.
        /// Logs a warning describing the exact path when the real art can't be loaded.
        /// </summary>
        public static Sprite Get(string key, Func<Sprite> placeholder)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            string path = $"{ResourcesRoot}/{key}";
            Sprite sprite = LoadReal(path, warnIfMissing: true);

            if (sprite == null && placeholder != null)
                sprite = placeholder();

            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Like <see cref="Get"/> but for genuinely-optional art (e.g. road / logo): returns the
        /// real sprite or <c>null</c> with no "missing" warning (a missing optional file is normal).
        /// </summary>
        public static Sprite GetOptional(string key)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            Sprite sprite = LoadReal($"{ResourcesRoot}/{key}", warnIfMissing: false);
            if (sprite != null) Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Core load: Sprite -> Texture2D(Sprite.Create) -> null, with diagnostics.</summary>
        private static Sprite LoadReal(string path, bool warnIfMissing)
        {
            // 1) Imported as a Sprite (preferred).
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return sprite;

            // 2) Imported as a plain Texture (Texture Type != Sprite, or Sprite Mode = Multiple):
            //    build a full-texture Sprite at runtime so the image still shows.
            Texture2D tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                Debug.LogWarning($"[AssetLoader] '{path}' is not imported as a Sprite — building one " +
                    "at runtime from the Texture. For best results set Texture Type = " +
                    "'Sprite (2D and UI)' in the import settings.");
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), DefaultPixelsPerUnit);
            }

            // 3) Nothing on disk at this path.
            if (warnIfMissing)
            {
                Debug.LogWarning($"[AssetLoader] No art found at 'Resources/{path}' " +
                    "(.png with this exact name under a 'Resources' folder). Using placeholder.");
            }
            return null;
        }

        /// <summary>True if a real sprite/texture exists for this key (either import style).</summary>
        public static bool Has(string key)
        {
            string path = $"{ResourcesRoot}/{key}";
            return Resources.Load<Sprite>(path) != null || Resources.Load<Texture2D>(path) != null;
        }

        /// <summary>Clears the runtime cache (e.g. if you swap art and re-enter Play).</summary>
        public static void ClearCache() => Cache.Clear();
    }

    /// <summary>
    /// Central registry of art slot paths (relative to <c>Resources/Art/</c>, no extension).
    /// Keeping them here means filenames live in exactly one place.
    /// </summary>
    public static class ArtKeys
    {
        // Characters: skin folder is capitalised ("Loui"/"Ver"), filename lowercase.
        // states: idle, run_01, run_02, jump, hit
        public static string Puppy(string skin, string state)
            => $"Characters/{skin}/puppy_{skin.ToLowerInvariant()}_{state}";

        public const string StateIdle = "idle";
        public const string StateRun01 = "run_01";
        public const string StateRun02 = "run_02";
        public const string StateJump = "jump";
        public const string StateHit = "hit";
        public const string StateDuck = "duck";

        // Obstacles
        public const string ObstacleCar = "Obstacles/obstacle_car";
        public const string ObstaclePuddle = "Obstacles/obstacle_puddle";
        public const string ObstacleTrashBin = "Obstacles/obstacle_trash_bin";
        public const string ObstacleFence = "Obstacles/obstacle_fence";
        public const string ObstacleCone = "Obstacles/obstacle_cone";
        public const string ObstacleHighBar = "Obstacles/obstacle_high_bar"; // head-height, duck under

        // Collectibles
        public const string CollectibleBone = "Collectibles/collectible_bone";
        public const string CollectibleScent = "Collectibles/collectible_scent";
        public const string CollectiblePhotoPiece = "Collectibles/collectible_photo_piece";

        // Backgrounds (parallax layers)
        public const string BgSky = "Backgrounds/bg_sky";
        public const string BgClouds = "Backgrounds/bg_clouds";
        public const string BgCity = "Backgrounds/bg_city";
        public const string BgTrees = "Backgrounds/bg_trees";
        public const string BgRoad = "Backgrounds/bg_road";

        // UI
        public const string UiButton = "UI/ui_button";
        public const string UiPanel = "UI/ui_panel";
        public const string UiLogo = "UI/ui_logo";
        public const string UiBoneIcon = "UI/ui_bone_icon";

        // Effects (optional; reserved slots — not loaded by the current MVP code)
        public const string FxJumpDust = "Effects/fx_jump_dust";
        public const string FxCollectSparkle = "Effects/fx_collect_sparkle";
        public const string FxScentTrail = "Effects/fx_scent_trail";
        public const string FxHeart = "Effects/fx_heart";
    }
}
