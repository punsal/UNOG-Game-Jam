# Device Test — Pacing/Prefab Build (2026-07-19)

Build `Builds/Android/LastLight.apk` (development, IL2CPP, 66.5 MB, 0 errors,
173 s build time) on Samsung Galaxy S23 Ultra (SM-S918B), driven by scripted
adb input: two blind runs and one dodge-aware run. Full logcat analysis via
the core-loop logging added this session.

## Verified working on device

| System | Evidence |
| --- | --- |
| Boot, title, tap-to-start | Title screenshot; `Start requested` in logcat |
| Gate hold | `Stage N held at the choice gate` on every gated stage; screenshot shows the run parked at the gate with both altars framed on screen |
| Offer readability | Both decision cards fully visible side by side at the held gate (closes the G3 capture gap; altars are inside the fitted 4.5 half-width view) |
| Choice commit | Exactly one `Choice made` per gate, rejected offer logged, no `Ignored entry` anomalies |
| Distinct pairs | Sight/Blood, Body/Light, Body/Sight, Light/Blood, Sight/Light observed |
| Cost application | Run-state snapshots correct after every choice (incl. Light -20 → 80) |
| Pacing | Stage completions 30 / 30 / 35 / 43.7* / 40 s vs targets 30/30/35/35/40 (*includes ~8 s decision wait at the held gate) |
| Death → restart | Deaths restart cleanly; gates re-arm (`Choice gate re-armed`) |
| Hitch clamp | Fired twice under adb-input stress (`Frame delta 0.117s clamped`), zero skipped content; clean run had no clamps |

## Not reproduced this pass

- **Ending screen on device.** All three automated runs died before stage 6
  completed. Endings remain verified in-editor (both Light and Dark) and the
  outcome screen was device-verified in the G3 pass.

## Design finding: difficulty vs. the new run length

With 3 HP, no healing between stages, and ~4-minute runs, blind runs died
deterministically (stage 4 twice, stage 6 once at 1 HP). Damage pattern:
spike stages are dodgeable by lane choice, but the moving blade reaches the
full corridor width, so each blade stage (2, 3, 5, 6) costs a hit unless the
player times it visually. A run therefore needs near-perfect blade timing
across four stages; dying on stage 6 restarts the whole ~4-minute run.

Options if this is too punishing (designer's call):
1. Heal 1 HP at each committed choice ("the altar takes, but steadies you").
2. Restart from the current stage instead of stage 1 after death.
3. Narrow the blade's sweep so a wall-hugging lane exists.

## Automation notes

- Driver scripts: scripted taps/swipes with per-stage lane plans; the gate
  hold makes timing forgiving (late swipes just wait at the gate).
- adb screenshot/input bursts still induce frame hitches, but the 0.1 s
  scroll clamp contains them (previously they teleported the run forward).
