# 🧪 Puppy For Mom — Play Test Checklist

Use this to verify the MVP is stable and playable after opening in Unity 6.
Check items off as you go. Nothing here requires real ad/IAP SDKs (all mocked).

> Tip: keep the **Console** open (`Window → General → Console`) during testing.
> Expect `[AdManager] (MOCK) ...` and `[IAP MOCK] ...` logs — those are normal.

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

## 10. Build smoke test (optional)
- [ ] `File → Build Settings → Android → Switch Platform` succeeds.
- [ ] `Build And Run` to an Android emulator/device launches in **portrait** and
      is playable with touch.
- [ ] (iOS) `Switch Platform → iOS → Build` produces an Xcode project that opens.

---

### Known MVP limitations (expected, not bugs)
- Art & SFX are procedural placeholders.
- Ads & IAP are **mocked** (logs + delays), not real SDKs.
- Render pipeline is built-in 2D (URP intentionally not enabled yet).
- Korean glyphs rely on system-font fallback unless a TMP Korean font is added.
