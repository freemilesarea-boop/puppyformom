# 🐶 Puppy For Mom

A cute, casual **endless runner** for Android & iOS. You play **Loui**, a little
Pomeranian who got separated from Mom on a walk. Run, jump over hazards, collect
bones and follow Mom's scent all the way home.

> This repository is a **playable MVP** built in **Unity 6 (2D)** with C#. Every
> screen and system listed in the design brief is implemented and runnable.

---

# 🇰🇷 초보자용 로컬 실행 가이드 (한국어)

> 게임을 처음 받아서 Unity로 직접 실행해보는 분을 위한 **단계별 안내**입니다.
> 컴퓨터에 아직 아무것도 안 깔려 있어도 순서대로 따라 하면 됩니다.

## 0. 미리 준비할 것
- **PC (Windows 또는 macOS)** 와 인터넷 연결
- **Unity Hub** (Unity 설치/관리 프로그램)
- **Unity 6 에디터** (`6000.0.x` 버전)
- (선택) GitHub 계정 — 없어도 ZIP 다운로드로 받을 수 있습니다.

---

## 1. GitHub에서 프로젝트 받기

방법은 두 가지입니다. **둘 중 하나만** 하면 됩니다.

### 방법 A — ZIP로 받기 (가장 쉬움, Git 몰라도 됨)
1. 브라우저에서 저장소 페이지로 이동:
   `https://github.com/freemilesarea-boop/puppyformom`
2. 이 작업 브랜치를 보려면 페이지 왼쪽 위 **브랜치 선택 버튼**(보통 `main`이라고
   적힌 곳)을 눌러 `claude/gallant-cori-00h7h7` 를 선택합니다.
   (또는 Pull Request #1 페이지에서 코드 확인 가능)
3. 초록색 **`< > Code`** 버튼 → **Download ZIP** 클릭.
4. 받은 ZIP를 원하는 폴더에 **압축 해제**. 예: `C:\Games\puppyformom` 또는
   `~/Games/puppyformom`.
   - ⚠️ 압축 해제 후 폴더 안에 `Assets`, `Packages`, `ProjectSettings` 폴더가
     **바로 보여야** 합니다. (폴더 안에 폴더가 또 있으면 한 단계 들어가세요.)

### 방법 B — Git으로 클론 (Git 설치되어 있을 때)
```bash
git clone https://github.com/freemilesarea-boop/puppyformom.git
cd puppyformom
git checkout claude/gallant-cori-00h7h7
```

---

## 2. Unity Hub에서 프로젝트 열기
1. **Unity Hub**를 실행합니다. (없으면 https://unity.com/download 에서 설치)
2. 왼쪽 **Projects(프로젝트)** 탭으로 이동.
3. 오른쪽 위 **Add(추가) → Add project from disk(디스크에서 추가)** 클릭.
4. 1단계에서 받은 **프로젝트 폴더**(=`Assets`가 들어 있는 폴더)를 선택.
5. 목록에 `puppyformom`(또는 폴더명)이 나타나면 **클릭하여 엽니다.**
6. 처음 열면 **수동 임포트로 몇 분** 걸립니다. 오른쪽 아래 진행 표시가 멈출
   때까지 기다리세요. (이때 `Library` 폴더가 자동 생성됩니다 — 정상입니다.)

---

## 3. Unity 버전이 다를 때 대응 방법
이 프로젝트는 `ProjectSettings/ProjectVersion.txt`에 **`6000.0.32f1`** 로
적혀 있습니다. 정확히 같은 버전이 없어도 괜찮습니다.

- **추천:** Unity Hub에서 같은 **6000.0.x (Unity 6)** 아무 버전이나 설치해 여세요.
- 프로젝트를 열 때 Hub가 *"이 버전이 없습니다 / 다른 버전으로 열까요?"* 라고
  물으면, 설치된 **6000.0.x** 를 선택하고 **그대로 진행**하면 됩니다.
  (Unity가 자동으로 프로젝트를 해당 버전에 맞게 업그레이드합니다.)
- Unity Hub에서 에디터 설치하는 법:
  **Installs(설치) 탭 → Install Editor → Unity 6 (LTS) 선택 → 설치.**
- 설치 시 **모듈 선택** 화면에서 빌드 테스트를 하려면 아래를 함께 체크하세요:
  - **Android Build Support** (그 안의 *Android SDK & NDK Tools*, *OpenJDK* 포함)
  - (Mac만 해당) **iOS Build Support**
- ❗ Unity 5 이하 / 2022 같은 **구버전으로는 열지 마세요.** 반드시 **Unity 6**.

---

## 4. Boot 씬 여는 방법
1. Unity가 열리면 아래쪽 **Project(프로젝트) 창**을 봅니다.
2. `Assets ▸ Scenes` 폴더로 들어갑니다.
3. **`Boot`** (정식 이름 `Boot.unity`) 파일을 **더블클릭**합니다.
4. 위쪽 **Hierarchy(하이어라키) 창**에 `Main Camera`와 `BootLoader`가 보이면
   씬이 제대로 열린 것입니다.

> 참고: 이 게임은 **어느 씬에서 Play를 눌러도** 동작하도록 만들어져 있습니다.
> 그래도 처음에는 **Boot 씬**에서 시작하는 것을 권장합니다.

---

## 5. Play 버튼을 눌렀을 때 "정상 화면" 기준
화면 맨 위 가운데의 **▶ (Play)** 버튼을 누릅니다. 아래 순서대로 나오면 정상입니다.

1. **부팅 화면**(따뜻한 파스텔 배경)이 잠깐 보임 → 약 0.6초 뒤 자동 전환.
2. **메인 메뉴**: "Puppy For Mom" 제목, 강아지/구름, 큰 **PLAY** 버튼,
   아래에 **상점 / 도감 / 설정** 버튼.
3. **PLAY** 클릭 → 게임 화면. 위쪽에 **거리(m) / 점수 / 뼈다귀 수** 표시,
   "탭하여 시작" 안내가 보임.
4. 화면을 **클릭(또는 스페이스바)** 하면 강아지가 점프하고 달리기 시작.
   - 길게 누르면 더 높이 점프
   - 자동차/웅덩이/쓰레기통/울타리/콘 같은 장애물이 오른쪽에서 다가옴
   - 뼈다귀를 먹으면 숫자가 올라감
5. 장애물에 부딪히면 **게임오버 패널**(다시하기 / 메인 메뉴 / 광고 보고 부활).

> 에디터에서는 마우스 클릭/스페이스바로 점프합니다. 실제 폰에서는 화면 터치입니다.
> 화면이 **세로(Portrait)** 비율이 아니라면, Game 창 위쪽 비율 드롭다운에서
> 세로 해상도(예: `1080x1920` 또는 `Portrait`)를 선택하세요.

---

## 6. 오류가 떴을 때 — 어디를 캡처해서 확인하나
문제가 생기면 아래 위치를 **스크린샷**으로 남겨 확인/문의하면 됩니다.

- **Console(콘솔) 창** ← 가장 중요
  - 메뉴: **Window ▸ General ▸ Console** (단축키: macOS `⌘⇧C`, Windows `Ctrl+Shift+C`)
  - **빨간 아이콘 = 에러**, 노란색 = 경고(대부분 무시 가능).
  - 빨간 줄을 **클릭하면 아래에 자세한 내용**이 나옵니다. 이 전체를 캡처하세요.
  - 특히 **`error CS####`** 라고 적힌 줄이 있으면 컴파일 에러입니다.
- **에디터 우측 하단 상태바**: 빨간 에러 메시지가 잠깐 뜨는 곳.
- **로그 파일**(콘솔을 못 봤을 때):
  - Windows: `C:\Users\<사용자>\AppData\Local\Unity\Editor\Editor.log`
  - macOS: `~/Library/Logs/Unity/Editor.log`

흔한 해결법:
- 빨간 에러가 없는데 화면만 분홍색 → 무시해도 됩니다(기본 2D 표시).
  화면 **전체**가 분홍이면 잘못 URP로 바꾼 경우이니 되돌리세요.
- 점프가 안 됨 → **Edit ▸ Project Settings ▸ Player ▸ Other Settings ▸
  Active Input Handling** 가 **Input Manager (Old)** 인지 확인.
- 알 수 없는 에러가 계속되면 **Assets ▸ Reimport All** 후 다시 Play.

---

## 7. Android 빌드 테스트 방법 (Windows/Mac 공통)
> 실제 휴대폰이나 에뮬레이터에서 돌려보는 단계입니다. (선택 사항)

1. **준비:** Unity 설치 시 **Android Build Support**(+ *SDK & NDK Tools*, *OpenJDK*)
   를 체크했는지 확인. 안 했다면 Hub의 *Installs ▸ ⚙ ▸ Add Modules*에서 추가.
2. Unity 메뉴 **File ▸ Build Settings(빌드 설정)** 열기.
3. 플랫폼 목록에서 **Android** 선택 → 아래 **Switch Platform** 클릭(전환에 시간 소요).
4. **Scenes In Build**에 4개 씬(Boot/MainMenu/Gameplay/Ending)이 체크되어 있는지 확인.
   (이 프로젝트는 이미 등록되어 있습니다.)
5. **실제 폰으로 바로 실행하려면:**
   - 폰에서 **개발자 옵션 + USB 디버깅**을 켜고 USB로 PC에 연결.
   - Build Settings의 **Run Device**에서 내 폰을 선택 → **Build And Run**.
   - 저장할 `.apk` 위치/이름을 정하면 빌드 후 폰에 자동 설치·실행됩니다.
6. **에뮬레이터로 실행하려면:** Android Studio의 가상 기기(AVD)를 먼저 실행해두면
   **Run Device** 목록에 나타납니다. 동일하게 **Build And Run**.
7. **세로 화면**으로 뜨고 화면 터치로 점프되면 성공.

> 참고: 스토어 업로드용이 아니라 **테스트용**이라 서명/타깃 SDK는 기본값으로 충분합니다.
> (정식 출시 시에는 타깃 SDK 상향, 키스토어 서명이 따로 필요합니다.)

---

## 8. iPhone 빌드 테스트 방법 (⚠️ macOS + Xcode 필요)
> iOS 빌드는 **Mac에서만** 가능합니다. Windows에서는 할 수 없습니다.

1. **준비물:** macOS, **Xcode**(App Store에서 무료 설치), Apple ID,
   Unity 설치 시 **iOS Build Support** 모듈.
2. Unity **File ▸ Build Settings ▸ iOS 선택 ▸ Switch Platform**.
3. **Build** 클릭 → 저장 폴더를 정하면 **Xcode 프로젝트 폴더**가 생성됩니다
   (`.xcodeproj` 또는 `.xcworkspace`).
4. 생성된 프로젝트를 **Xcode로 엽니다.**
5. Xcode 상단 프로젝트 설정에서 **Signing & Capabilities** 탭:
   - **Team**에 본인 Apple ID(개인 무료 계정도 가능) 선택.
   - 필요 시 **Bundle Identifier**를 고유한 값으로 변경
     (예: `com.본인이름.puppyformom`).
6. 아이폰을 USB로 Mac에 연결 → Xcode 상단 기기 선택에서 **내 아이폰** 선택.
7. ▶ **Run(실행)** 버튼 클릭 → 아이폰에 설치됩니다.
   - 처음 실행 시 아이폰 **설정 ▸ 일반 ▸ VPN 및 기기 관리**에서 개발자 앱을
     **신뢰(Trust)** 해줘야 실행됩니다.
8. 세로 화면 + 터치 점프가 되면 성공.

> 무료 Apple 계정으로 만든 빌드는 보통 **7일 후 만료**되어 재설치가 필요합니다.
> 시뮬레이터로도 테스트 가능하지만 실제 기기에서 확인하는 것을 권장합니다.

---

> 더 자세한 항목별 동작 점검은 [`TEST_CHECKLIST.md`](TEST_CHECKLIST.md) 를 참고하세요.

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

## 🚀 First run — step by step (read this first)

1. **Install Unity 6** (`6000.0.x`) via Unity Hub. The project pins
   `6000.0.32f1` in `ProjectSettings/ProjectVersion.txt`; any `6000.0.x` will
   open it (Hub may show an "upgrade" prompt — that's normal, accept it).
2. In **Unity Hub → Add → Add project from disk**, select **this folder**
   (the one containing `Assets/`, `Packages/`, `ProjectSettings/`).
3. Open the project. The **first import takes a few minutes** (Unity builds the
   `Library/` folder). Wait until the spinner in the bottom-right stops.
4. **Check the Console** (`Window → General → Console`). It should be free of
   red **compile errors**. (A few yellow warnings are fine.)
5. Open **`Assets/Scenes/Boot.unity`** (double-click it in the Project window).
6. Press **▶ Play**.

That's it — you should see the boot screen, then the Main Menu, then tap/click
**PLAY** to start running.

> ✅ **You can press Play from any scene.** A `RuntimeInitializeOnLoadMethod`
> creates the shared services before the first scene loads, so even if you press
> Play while `Gameplay.unity` (or any scene) is open, it still works — you are not
> forced to start from `Boot`.

The scene flow is `Boot → MainMenu → Gameplay → Ending`; all four scenes are
already registered in **Build Settings** (no manual setup needed).

> **Zero art assets required.** All sprites (puppy, obstacles, treats, clouds,
> backgrounds) and sound effects are generated **procedurally at runtime**, so the
> game renders and plays immediately. Swap them for real art/audio later.

### ✅ Pre-flight verification (optional, command line)
You can confirm the project compiles without opening the Editor GUI:

```bash
# point UNITY at your installed editor binary, then:
UNITY="/Applications/Unity/Hub/Editor/6000.0.32f1/Unity.app/Contents/MacOS/Unity" \
  ./Tools/verify_compile.sh
```

This launches Unity in **batchmode**, imports + compiles all scripts, and fails
if any `error CS####` appears in the log. See `Tools/verify_compile.sh` for the
exact command and platform paths.

### 🛟 Troubleshooting
- **Pink / magenta sprites:** ignore — generated sprites use the default 2D
  material and render correctly in the built-in pipeline. If the whole screen is
  pink, you likely switched to URP without assigning a pipeline asset.
- **No input / jump doesn't work:** confirm `Edit → Project Settings → Player →
  Other Settings → Active Input Handling` is **Input Manager (Old)**. This repo
  ships it set that way (`activeInputHandler: 0`) and uses the legacy `Input` API.
- **Korean text shows as boxes in a build:** the built-in font falls back to the
  device's system font for Korean; add a TextMeshPro Korean font for guarantees.
- **NullReference about GameManager:** make sure you opened the *project root*
  (so `RuntimeInitializeOnLoadMethod` runs); re-import if needed
  (`Assets → Reimport All`).

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
