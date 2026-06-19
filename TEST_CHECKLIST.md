# 🧪 Puppy For Mom — Play Test Checklist

Use this to verify the MVP is stable and playable after opening in Unity 6.
Check items off as you go. Nothing here requires real ad/IAP SDKs (all mocked).

> Tip: keep the **Console** open (`Window → General → Console`) during testing.
> Expect `[AdManager] (MOCK) ...` and `[IAP MOCK] ...` logs — those are normal.

---

## 🇰🇷 처음이라면 (3분 요약)

> 프로젝트를 받고 여는 자세한 방법은 [`README.md`](README.md)의
> **"초보자용 로컬 실행 가이드"** 를 먼저 보세요. 아래는 빠른 점검용입니다.

1. Unity Hub에서 프로젝트 열기 → 첫 임포트 끝날 때까지 대기.
2. **Console 창**(`Window ▸ General ▸ Console`)에 **빨간 에러가 없는지** 확인.
3. `Assets ▸ Scenes ▸ Boot` 더블클릭으로 씬 열기.
4. 상단 **▶ Play** 누르기.
5. **부팅 → 메인 메뉴 → PLAY → 달리기**가 되면 성공. 클릭/스페이스로 점프.

### ✅ "정상" 한눈에 보기
| 단계 | 정상 화면 |
|------|-----------|
| Play 직후 | 따뜻한 파스텔 부팅 화면 (약 0.6초) |
| 그 다음 | 메인 메뉴: 제목 + 강아지 + **PLAY / 상점 / 도감 / 설정** |
| PLAY 후 | 상단에 거리/점수/뼈다귀, "탭하여 시작" 안내 |
| 클릭/스페이스 | 강아지가 점프하며 자동 달리기 시작 |
| 장애물 충돌 | 게임오버 패널 (다시하기 / 메인 메뉴 / 부활) |

### 🚨 에러가 보이면 어디를 캡처?
- **Console 창의 빨간 줄** → 클릭해서 펼친 **상세 내용 전체**를 캡처.
  특히 **`error CS####`** 줄이 핵심입니다.
- 또는 로그 파일: Windows `…\AppData\Local\Unity\Editor\Editor.log` /
  macOS `~/Library/Logs/Unity/Editor.log`.

---

## 0. Project sanity (before pressing Play)
- [ ] Project opens in Unity Hub without errors; first import finishes.
- [ ] **Console has zero red compile errors.** (Yellow warnings OK.)
- [ ] `Edit → Project Settings → Player → Other Settings → Active Input Handling`
      is **Input Manager (Old)**.
- [ ] `Edit → Project Settings → Player → Resolution and Presentation` shows
      **Portrait** as the default orientation.
- [ ] `File → Build Settings` lists 4 scenes in order: **Boot, MainMenu, Gameplay, Ending**.
- [ ] (Optional) `./Tools/verify_compile.sh` prints **"No compile errors"**.

## 1. Boot → Main Menu
- [ ] Open `Assets/Scenes/Boot.unity`, press **Play**.
- [ ] Boot shows briefly, then auto-loads **Main Menu** (~0.6s).
- [ ] Main Menu shows: title "Puppy For Mom", subtitle, best-record line,
      a big **PLAY** button, and **상점 / 도감 / 설정** buttons.
- [ ] A puppy + clouds + pastel background are visible.

## 2. Main Menu sub-panels
- [ ] **상점 (Shop):** opens; shows Remove Ads / Skin Pack / Puppy Bundle; tapping
      "광고 제거" logs `[IAP MOCK] Remove Ads purchased.` and closes; **닫기** closes.
- [ ] **도감 (Collection):** opens; shows 6 puppy cards (Loui unlocked, others
      locked/greyed). **닫기** closes.
- [ ] **설정 (Settings):** opens; **소리 끄기/켜기** toggles label & mutes SFX;
      **기록 초기화** resets saved bests; **닫기** closes.

## 3. Core gameplay (the important part)
- [ ] Press **PLAY** → Gameplay loads with HUD (distance / score / bones) + pause button.
- [ ] "탭하여 시작 / Tap to start" prompt is shown; puppy idles on the ground.
- [ ] **First tap/click/Space** starts the run and makes the puppy jump.
- [ ] Puppy auto-runs (world scrolls left); ground tufts & clouds move.
- [ ] **Tap = small jump, hold = higher jump** (variable height feels different).
- [ ] **Distance** counter increases; **Score** increases over time.
- [ ] Obstacles (car/puddle/bin/fence/cone) approach from the right.
- [ ] Hitting an obstacle ends the run → **Game Over** panel appears.
- [ ] Bones increment the **bone counter**; collecting plays a blip + pops.
- [ ] Scent / photo-piece collectibles can be picked up without error.

## 3b. Controls, duck & difficulty (new)
**Controls**
- [ ] **Left-screen touch** (or **Space / ↑ / Left-mouse**) makes the puppy **jump**.
- [ ] **Right-screen touch** (or **↓ / S / Right-mouse**) makes the puppy **duck**.
- [ ] PC inputs all work in the Editor (mouse + keyboard).
- [ ] While ducking, the puppy uses the crouched (lower) sprite and its **collider
      becomes shorter**; releasing returns it to the standing collider.
- [ ] Ducking in the **air does nothing** (duck only works while grounded).
- [ ] Pressing jump while ducking cancels the duck and jumps.

**New obstacle patterns**
- [ ] **GroundObstacle / LowObstacle** are cleared by **jumping**.
- [ ] **HighObstacle** (overhead bar) is cleared by **ducking**; running into it
      while standing ends the run.
- [ ] **CollectibleLine** spawns a row of bones to grab.
- [ ] **MixedPattern** = a jump obstacle then a duck bar, with enough space to land
      first (jump → duck), never an instant-death.
- [ ] **SafeGap** gives a calm breather (no hazard, a couple of treats).

**Difficulty**
- [ ] HUD shows `… m · Lv.N`; the level increases **every 100 m** with a `Lv.N` toast.
- [ ] Speed clearly increases as distance grows.
- [ ] Patterns get more frequent and more varied at higher levels (high/mixed appear).
- [ ] First **100 m** is easy (ground/low + frequent safe gaps).
- [ ] No impossible patterns: never two opposite-action hazards at the same spot,
      and never a duck-bar landing on you immediately after a forced jump.

## 4. Difficulty & speed
- [ ] Scroll speed slowly increases the further you travel (harder over time).
- [ ] Spawn cadence tightens with distance but stays clearable.

## 5. Milestones, cutscene & ending
- [ ] At ~**500 / 1500 / 3000 m**, a toast message appears (e.g. "엄마 어디 갔지?").
      *(Tip: temporarily lower thresholds in `GameConfig.cs` to test fast.)*
- [ ] At **5000 m**, a **cutscene card** appears and the run pauses; **계속하기**
      resumes the run.
- [ ] At **10000 m**, the **Ending** scene loads: reunion scene + scripted lines,
      then "무한 모드 / 메인 메뉴" buttons appear.
- [ ] After the ending, returning to Main Menu shows a **무한 모드 (Endless)** button.

## 6. Pause
- [ ] Pause button (or **Esc** / Android Back) opens the pause panel; game freezes.
- [ ] **계속** resumes, **다시하기** restarts the run, **메인 메뉴** returns to menu.

## 7. Game Over & revive (mock rewarded ad)
- [ ] Game Over shows distance / score / best.
- [ ] **광고 보고 부활** triggers a ~1s mock ad (Console log), then resumes play
      with brief invincibility; the revive button is hidden afterward (once per run).
- [ ] **다시하기** starts a fresh run; **메인 메뉴** returns to menu.
- [ ] Every **3rd** game over triggers a mock **interstitial** (Console log),
      unless Remove Ads was purchased.

## 8. Persistence (save system)
- [ ] After a run, best distance / high score persist on the Main Menu.
- [ ] Quitting and re-entering Play preserves bests (PlayerPrefs).
- [ ] Settings → 기록 초기화 clears them.

## 9. Stability / regression (no leaks, no spam)
- [ ] Play → die → **다시하기** several times: no growing pile of Console errors,
      HUD still updates correctly each run (verifies event-handler cleanup).
- [ ] Switch Main Menu ↔ Gameplay ↔ Ending repeatedly: only **one** EventSystem
      and **one** `~GameServices` object exist (check the Hierarchy with
      "DontDestroyOnLoad" expanded).
- [ ] No `MissingReferenceException` spam after scene changes.

## 10. Art pipeline (placeholder ↔ real PNG swap)
> See README → "Art pipeline" for folder paths, sizes and import settings.

**Without any PNGs (default state):**
- [ ] Game runs with procedural placeholders — **no errors** about missing art.
- [ ] The puppy is a **full-body side-view** dog (not just a face), fully visible
      on screen with paws on the ground.
- [ ] Running shows a simple **2-frame leg animation**; jumping shows the **jump**
      pose; hitting an obstacle shows the **hit** pose.
- [ ] Obstacles look **different per type** (car / puddle / bin / fence / cone).
- [ ] Collectibles look **different per type** (bone / scent / photo piece).
- [ ] Background shows distinct **parallax layers** (sky, clouds, city, trees, road)
      moving at different speeds.

**Size / collision balance (resolution-independent):**
- [ ] Bones are small — roughly **30–50% of the puppy's height**.
- [ ] Cones are roughly **60–90% of the puppy's height** (not wall-sized).
- [ ] Swapping in a much higher- or lower-resolution PNG does **not** change the
      in-game size (only aspect ratio matters).
- [ ] Selecting **`GameplayBootstrap`** in `Gameplay.unity` shows `Obstacle/Collectible
      World Height` + `Scale Multiplier` fields, and changing them resizes spawns.

**After dropping in real PNGs:**
- [ ] Put e.g. `Assets/Resources/Art/Characters/Loui/puppy_loui_idle.png` (Texture
      Type = **Sprite**) → re-enter Play → the **real image** is used automatically.
- [ ] Add `Assets/Resources/Art/Obstacles/obstacle_car.png` → cars now use it; other
      obstacles still fall back to placeholders (mixed state works).
- [ ] Add a `bg_*` image → that background layer shows the real art and still scrolls.
- [ ] Add `ui_bone_icon.png` / `ui_button.png` → HUD bone icon / buttons use them.
- [ ] Remove a PNG → it cleanly reverts to the placeholder (no errors).
- [ ] A character PNG of a **different resolution** still appears the correct on-screen
      size (auto-normalised height), with the collider fitting the body.

## 11. Build smoke test (optional)
- [ ] `File → Build Settings → Android → Switch Platform` succeeds.
- [ ] `Build And Run` to an Android emulator/device launches in **portrait** and
      is playable with touch.
- [ ] (iOS) `Switch Platform → iOS → Build` produces an Xcode project that opens.

---

### Known MVP limitations (expected, not bugs)
- Art & SFX are procedural placeholders **until** real PNGs are dropped into
  `Assets/Resources/Art/...` (see README → Art pipeline).
- Ads & IAP are **mocked** (logs + delays), not real SDKs.
- Render pipeline is built-in 2D (URP intentionally not enabled yet).
- Korean glyphs rely on system-font fallback unless a TMP Korean font is added.
