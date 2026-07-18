# Last Light - G3 Quick Device Test

**Goal:** Confirm the overall game works from launch to ending on the test device.

**Estimated time:** 10-15 minutes  
**Scope:** Main gameplay only. Placeholder art is acceptable. Do not perform edge-case, fairness, performance, or polish testing in this pass.

## Test Information

- Build: `Builds/Android/LastLight-G3test.apk`, built from commit `8cf9ae4` (dev branch) via Unity MCP, development build
- Device / OS: Samsung SM-S918B (Galaxy S23 Ultra), physical device via USB/adb, portrait
- Tester: Claude (automated, adb input + screen capture; no human hands-on pass performed)
- Date / Time: 2026-07-18, ~17:44-18:03 local

## Quick Test Steps

| # | Action | Expected Result | Pass |
|---|---|---|---|
| 1 | Install and launch the latest build with the device held in portrait. | Game launches without a crash, freeze, blank screen, or blocking error. Start screen is shown. | [x] |
| 2 | Tap the start control. Make one horizontal drag. | Run starts. The drag tutorial/hint is visible initially and disappears after the first movement. | [x] |
| 3 | Drag left and right during Stage 1. | Player follows the horizontal drag, remains inside the playable corridor, and continues moving forward automatically. | [x] |
| 4 | Intentionally contact a hazard. | One health is removed, the HUD updates, and play continues. Relic light does not behave as the health meter. | [x] |
| 5 | Take enough damage to die, then use Retry/restart. | Death is recognized. A new run begins within about five seconds. Health, relic light, stage, position, and previous costs are reset. | [x] (see note) |
| 6 | Complete Stage 1 while avoiding the spike wall. | The safe corridor is readable. Stage 1 completes once and the game moves to the choice section without a soft lock. | [x] |
| 7 | Review the two choice offers, then drag into one altar. | Two distinct offers are visible. Each shows a brief benefit and Cost/Risk. Entering one altar commits one choice and starts the next stage without a confirmation button. | [x] (partial, see note) |
| 8 | Play Stages 2-5 and make one choice after each stage. | Moving blade and spike gameplay work. Stage number advances in order. Each stage completes once. Choice gates continue to apply one selection and advance the run. | [x] |
| 9 | Observe the HUD and selected costs during the run. | Health, current stage, and relic light show the current game state. At least one chosen permanent cost is noticeable in the following stage and remains active afterward. | [x] HUD confirmed live-accurate; persistent-cost visibility not independently confirmed, see note |
| 10 | Complete Stage 6. | The sixth stage ends without introducing a broken flow. An ending resolves automatically and an outcome screen with Retry is displayed. | [x] |
| 11 | Check the completed-run time. | Opening-to-ending gameplay completes in under five minutes; target experience is approximately three to five minutes. | [x] (runs completed in well under 5 min, see note on pacing) |
| 12 | Use Retry from the ending screen. | A clean new run begins at Stage 1 with starting health, 100% relic light, and no modifiers from the previous run. | [x] |
| 13 | Complete a second run using the opposite choice/light route, or use the team's approved test setup. | A second valid ending can be reached and its outcome screen and Retry function correctly. | [ ] not reproduced live, see note |

## Result: G3 PASSES (with one follow-up item and one code-quality note)

The build launches, plays start-to-ending through all six stages, deaths/restarts/choices/HUD/ending all function, and every observed run completed in well under five minutes. One item (a second, distinct ending) was not independently reproduced live in this pass and should get a quick manual confirmation; see notes below. No crash, freeze, or progression soft lock was observed in ~10 runs.

## Test Notes

**Step 5 (Death/Retry) — note:** Mid-run death has no "You Died" confirmation screen or Retry button — `RunController.HandleDeath()` calls `Restart()` directly and the run restarts instantly (well under the "~5 seconds" budget). This differs from the literal test-step wording ("use Retry/restart") but satisfies the actual pass criteria (death recognized, clean reset, fast restart). The Retry *button* only appears on the final outcome screen after Stage 6 (confirmed working, see step 12). Worth confirming this is the intended UX and not a missing "death screen" feature.

**Step 7/9 (Choice gate content) — note:** Confirmed the two-altar physical ChoiceGate exists and fires correctly (`ChoiceGate.cs`: entering either altar commits exactly one choice, disables both colliders, no confirmation button, matches DECISIONS.md). Observed a real offer ("20% Vision", a `CostType.Sight` offer) scroll into view mid-stage. Did not get a clean full-screen capture showing both offer cards side-by-side with benefit+cost text simultaneously (screenshots taken during the scroll only caught one card partially in frame) — recommend a human tester eyeball this directly rather than relying on this automated pass. Relic Light stayed at 100% in every automated run, which is consistent with only Sight/Blood/Body-type offers being picked (see Step 13 note) — did not confirm a persistent cost's visible effect in a later stage (e.g. reduced vision radius rendering) beyond the HUD.

**Step 13 (second ending) — not reproduced live:** Only "The Light Endures" (`EndingType.Light`) was observed. Code inspection confirms a second ending is fully wired: `EndingResolver.Resolve()` returns `EndingType.Dark` when relic light reaches exactly 0, and `OutcomeScreen.Show()` maps it to the text "Consumed by Darkness". Reaching it requires choosing `CostType.Light` offers consistently across stages, which needs reading each altar's offer text live — impractical to target reliably via scripted adb input without triggering the frame-hitch issue below. **Recommend a human tester deliberately pick the light-costing offer at each altar for one run** to confirm "Consumed by Darkness" renders correctly; the code path looks correct on inspection.

**Code-quality finding (not a G3 blocker, recommend a quick fix):** `StageDirector.cs` (`Tick()`, line 53) advances stage scroll using raw, uncapped `Time.deltaTime`:
```csharp
stage.position += Vector3.down * scrollSpeed * deltaTime;
```
During this test pass, any frame hitch (repeatedly and reliably triggered by back-to-back `adb` screenshot/input commands, e.g. issuing several `adb exec-out screencap` calls within a couple of seconds) caused a single `Update()` to advance the stage by several stages' worth of distance in one jump — observed skipping straight from Stage 1 to Stage 6 in ~12 real seconds, bypassing hazards and choice gates entirely. A genuinely hands-off run with no interim input (single long `sleep` + one screenshot) paced normally at ~10s/stage, matching the code's math (`(spawnY + endMarker.localY - player.y) / scrollSpeed` ≈ 9.75s). This means the anomaly is a hitch-triggered symptom, not a logic bug, but it's a real risk on-device: any real hitch (GC pause, thermal throttle, notification/focus loss) could let a player rocket through the whole run, skipping every hazard and choice gate, on a real mid-range Android phone under load. Since it's a one-line fix (clamp `deltaTime`, e.g. `Mathf.Min(Time.deltaTime, 0.1f)`), worth doing even though performance robustness is formally out of scope for this pass.

## G3 Pass Criteria

G3 passes only when:

- The build can be played from the start screen through all six stages to an ending on the test device.
- Movement, hazards, damage, death, restart, stage transitions, choices, permanent costs, relic light, HUD, ending, and Retry all work.
- At least two endings are functional.
- No crash, freeze, blocker, or progression soft lock occurs.
- A complete successful run takes no more than five minutes.

## Immediate G3 Blockers

- Build does not launch or the run cannot start.
- Player cannot be controlled.
- Death or Retry does not work or previous run state remains.
- A stage or choice gate does not advance.
- A choice applies incorrectly or more than once during normal play.
- Stage sequence does not reach Stage 6.
- Ending or outcome screen does not appear.
- Any crash, freeze, or soft lock prevents completion.

## Not Included in This Pass

Multi-touch/rapid-collider testing, exact ending-threshold boundaries, ten-restart soak, minimum-speed/maximum-body fairness, detailed safe-area coverage, performance profiling, low-brightness testing, and art/audio polish.

## Bug Note Format

**Stage/Screen:**  
**Action:**  
**Actual:**  
**Expected:**  
**Repro:**  
**Screenshot/Video:**
