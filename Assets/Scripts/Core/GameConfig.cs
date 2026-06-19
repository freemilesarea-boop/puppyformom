using UnityEngine;

namespace PuppyForMom.Core
{
    /// <summary>
    /// All gameplay tuning lives here so designers can balance the game in one place.
    /// Plain static values keep the MVP simple (no ScriptableObject asset wiring required).
    /// </summary>
    public static class GameConfig
    {
        // ---- World / movement ----
        // The puppy auto-runs: the world scrolls left at this speed (units/sec).
        public const float StartScrollSpeed = 6f;
        public const float MaxScrollSpeed = 14f;
        // Speed gained per metre travelled. Keeps difficulty climbing slowly.
        public const float SpeedGainPerMeter = 0.0006f;

        // 1 world unit == this many in-game metres for the distance counter.
        public const float MetersPerWorldUnit = 1.4f;

        // ---- Player jump ----
        public const float Gravity = 3.2f;
        public const float JumpVelocity = 11f;
        // Extra upward force applied while the player keeps holding (variable jump height).
        public const float HoldJumpForce = 26f;
        public const float MaxHoldTime = 0.28f;
        public const float GroundY = -3.2f;        // resting Y of the puppy
        public const float CoyoteTime = 0.08f;     // grace window to still jump after leaving ground

        // ---- Spawning ----
        public const float SpawnXOffset = 7f;      // how far right of the camera things appear
        public const float DespawnX = -9f;         // left edge where things get recycled/destroyed
        public const float MinSpawnGap = 1.1f;     // seconds between spawns (hard)
        public const float StartSpawnInterval = 1.7f;
        public const float MinSpawnInterval = 0.85f;
        public const float CollectibleChance = 0.55f; // chance a spawn slot is a treat instead of an obstacle

        // ---- Scoring ----
        public const int BoneScoreValue = 10;
        public const int MeterScoreDivisor = 2; // score also gets distanceMeters / divisor

        // ---- Story milestones (metres) ----
        public const int Milestone1 = 500;    // "엄마 어디 갔지?"
        public const int Milestone2 = 1500;   // "배고파..."
        public const int Milestone3 = 3000;   // "엄마 냄새가 나!"
        public const int CutsceneDistance = 5000;  // first cutscene
        public const int EndingDistance = 10000;   // mom found ending

        // ---- Monetization ----
        public const int InterstitialEveryNGames = 3; // show interstitial once per 3 games
        public const int ReviveBonusInvincibleFrames = 90; // ~1.5s invincibility after revive

        public static string MilestoneMessage(int meters)
        {
            if (meters >= EndingDistance) return "엄마 발견!";
            if (meters >= CutsceneDistance) return "거의 다 왔어!";
            if (meters >= Milestone3) return "엄마 냄새가 나!";
            if (meters >= Milestone2) return "배고파...";
            if (meters >= Milestone1) return "엄마 어디 갔지?";
            return string.Empty;
        }

        // ---- Palette (warm pastel healing vibe) ----
        public static readonly Color SkyTop = new Color(0.99f, 0.90f, 0.80f);
        public static readonly Color SkyBottom = new Color(1.00f, 0.97f, 0.90f);
        public static readonly Color GroundColor = new Color(0.78f, 0.86f, 0.62f);
        public static readonly Color GroundShadow = new Color(0.66f, 0.77f, 0.50f);
        public static readonly Color PuppyCream = new Color(0.98f, 0.89f, 0.74f);
        public static readonly Color PuppyBlack = new Color(0.25f, 0.24f, 0.27f);
        public static readonly Color BoneColor = new Color(0.98f, 0.97f, 0.92f);
        public static readonly Color SmellColor = new Color(0.65f, 0.85f, 0.98f);
        public static readonly Color PhotoColor = new Color(0.98f, 0.80f, 0.55f);
        public static readonly Color ObstacleCar = new Color(0.92f, 0.45f, 0.45f);
        public static readonly Color ObstacleFence = new Color(0.80f, 0.62f, 0.42f);
        public static readonly Color ObstacleBin = new Color(0.55f, 0.62f, 0.70f);
        public static readonly Color ObstaclePuddle = new Color(0.55f, 0.72f, 0.92f);
        public static readonly Color ObstacleCone = new Color(0.97f, 0.62f, 0.30f);
    }
}
