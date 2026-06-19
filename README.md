# 🐶 Puppy For Mom

A cute, casual **endless runner** for Android & iOS. You play **Loui**, a little
Pomeranian who got separated from Mom on a walk. Run, jump over hazards, collect
bones and follow Mom's scent all the way home.

> This repository is a **playable MVP** built in **Unity 6 (2D)** with C#. Every
> screen and system listed in the design brief is implemented and runnable.

---

## ▶️ How to play

| Input | Action |
|-------|--------|
| **Tap / Click / Space** | Jump |
| **Hold** | Jump higher (variable height) |
| **Pause button / Esc / Android Back** | Pause |

The puppy auto-runs; you only control jumping. Avoid cars, puddles, bins, fences
and cones. Collect:

- 🦴 **Bones** → score
- 💨 **Mom's scent** → story flavour
- 🖼️ **Photo pieces** → unlock the ending

### Progression (distance based, endless)
| Distance | Beat |
|----------|------|
| 500 m | "엄마 어디 갔지?" |
| 1500 m | "배고파..." |
| 3000 m | "엄마 냄새가 나!" |
| 5000 m | First cutscene |
| 10000 m | Mom found — **Ending** |
| After ending | **Endless Mode** unlocked |

---

## 🚀 Opening & running the project

1. Install **Unity 6** (`6000.0.x`) via Unity Hub (the project pins
   `6000.0.32f1` in `ProjectSettings/ProjectVersion.txt`; any 6000.0.x will
   offer to open it).
2. **Add project from disk** in Unity Hub → select this folder.
3. Open the scene **`Assets/Scenes/Boot.unity`** and press **Play**, or just press
   Play (the Boot scene is first in Build Settings and chains to the menu).

The flow is `Boot → MainMenu → Gameplay → Ending`, all four scenes are already in
**Build Settings**.

> **Zero art assets required.** All sprites (puppy, obstacles, treats, clouds,
> backgrounds) and sound effects are generated **procedurally at runtime**, so the
> game renders and plays immediately. Swap them for real art/audio later.

### Build for Android
- `File → Build Settings → Android → Switch Platform`.
- Player Settings are preconfigured: **Portrait** orientation, package id
  `com.goodpupstudio.puppyformom`, min SDK 23.
- `Build` (or `Build And Run` on a device/emulator).

### Build for iOS
- `File → Build Settings → iOS → Switch Platform → Build` → open the generated
  Xcode project, set your signing team, and run.

---

## 🧱 Architecture

Code-driven design: each scene is a thin shell (a camera + one bootstrap
component) and the world/UI is assembled in code. This keeps the project robust
and easy to diff.

```
Assets/Scripts/
├── Core/        BootLoader, GameManager, GameState, GameConfig, ServiceLocator, SceneNames
├── Player/      PlayerController (auto-run, tap/hold jump, collisions)
├── Gameplay/    GameplayBootstrap, ObstacleSpawner, Obstacle, Collectible,
│                GroundScroller, ParallaxBackground
├── Systems/     ScoreManager, DistanceManager, AudioManager, AdManager (mock), SaveManager
├── UI/          UIManager (HUD), MainMenuController, EndingController
└── Utils/       SpriteFactory (procedural art), UIBuilder (in-code uGUI)
```

- **GameManager** owns the `GameState` machine (Ready → Playing → Cutscene →
  GameOver / Ending) and persists across scenes.
- **GameConfig** centralises all tuning (speed, gravity, spawn rates, milestones,
  palette) so the game is easy to balance.
- **Services** (save, audio, ads, score, distance, game manager) are created once
  in the Boot scene and registered in a lightweight `ServiceLocator`.

## 💰 Monetization (no Pay-To-Win)
- **Rewarded ad** → revive once per run + bonus bones (opt-in only).
- **Interstitial** → once every 3 games (skipped if ads removed).
- **IAP** (mock) → Remove Ads, Skin Pack, Puppy Bundle.

`AdManager` is a **mock** that simulates ad playback with logs + delays so the
full flow is testable. Replace the method bodies with AdMob / Unity Ads / LevelPlay
and wire a real IAP SDK before release.

---

## 📝 Notes / next steps
- **Render pipeline:** uses the built-in 2D pipeline for maximum portability. To
  move to **2D URP** as in the brief, install `com.unity.render-pipelines.universal`,
  create a *URP 2D* asset + *2D Renderer*, and assign it in Project Settings →
  Graphics. The gameplay code is pipeline-agnostic.
- **Fonts:** UI uses Unity's built-in dynamic font, which falls back to system
  fonts for Korean on device. For guaranteed Korean glyphs everywhere, add a
  TextMeshPro Korean font asset.
- **Post-launch roadmap:** achievements, ranking/leaderboards, season pass,
  events, friends — scaffolding (save data, score/distance bests) is already here.
