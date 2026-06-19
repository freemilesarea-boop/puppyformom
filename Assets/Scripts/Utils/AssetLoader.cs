using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuppyForMom.Utils
{
    /// <summary>
    /// Loads real art (PNG sprites) by name from a Resources folder, with a procedural
    /// <see cref="SpriteFactory"/> fallback so the game ALWAYS renders — even with no art yet.
    ///
    /// Drop PNGs into <c>Assets/Resources/Art/&lt;category&gt;/...</c> (Texture Type = Sprite)
    /// and they are picked up automatically; until then the placeholder is used.
    ///
    /// We use Resources (not Assets/Art directly) because <see cref="Resources.Load"/> is the
    /// only built-in way to load a sprite by string path at runtime without extra packages,
    /// and it requires the files to live under a folder named "Resources".
    /// </summary>
    public static class AssetLoader
    {
        public const string ResourcesRoot = "Art";
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Returns the real sprite at <paramref name="key"/> if present, otherwise the result of
        /// <paramref name="placeholder"/> (which is run once and cached).
        /// </summary>
        public static Sprite Get(string key, Func<Sprite> placeholder)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            Sprite sprite = Resources.Load<Sprite>($"{ResourcesRoot}/{key}");
            if (sprite == null && placeholder != null) sprite = placeholder();
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>True if a real (imported) sprite exists for this key.</summary>
        public static bool Has(string key) => Resources.Load<Sprite>($"{ResourcesRoot}/{key}") != null;

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

        // Obstacles
        public const string ObstacleCar = "Obstacles/obstacle_car";
        public const string ObstaclePuddle = "Obstacles/obstacle_puddle";
        public const string ObstacleTrashBin = "Obstacles/obstacle_trash_bin";
        public const string ObstacleFence = "Obstacles/obstacle_fence";
        public const string ObstacleCone = "Obstacles/obstacle_cone";

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
    }
}
