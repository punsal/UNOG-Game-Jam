# Last Light

A portrait 2D auto-runner for Android, built in a 48-hour jam. You carry the
world's last light down a scrolling corridor: dodge spikes and blades with a
one-finger horizontal drag, and at the end of each stage choose one of two
altars — every choice takes something from you permanently (health, speed,
vision, or the light itself) and buys relief in the next stage. Six stages,
under three minutes, two endings decided by how much light survives to reach
the Last Hearth.

## Requirements

- Unity **6000.5.4f1** (Android build support module for device builds).
- An Android device (min SDK 26) or the editor's Play Mode.

## Run it

1. Open the project, open `Assets/Scenes/Bootstrap.unity`, press Play
   (Bootstrap immediately loads `MainScene`; opening `MainScene` directly
   also works).
2. Drag horizontally with the mouse to steer. Enter an altar to see its
   offer; Accept commits, Decline recentres you with both altars still open.

## Build it

- Target Android, scenes `Bootstrap` + `MainScene` (already configured).
- Package id `com.punsal.lastlight`, app name "Last Light".
- Development builds enable the dev conveniences below; **release builds
  compile them out** (`DEVELOPMENT_BUILD` guard).

## Project layout

```
Assets/
  Scenes/            Bootstrap (entry), MainScene (the game), ArtTech_Test (dev)
  Scripts/
    Run/             StageDirector (scroll/hold/pacing), RunController (reset/death), Boot, endings
    Player/          drag input, motor, health (incl. god mode)
    Choice/          ChoiceGate + altars, CostApplier (costs), BenefitApplier (reliefs)
    Hazards/         ObstacleBase (damage+telegraph), MovingBlade
    UI/              start/death/outcome screens, choice popup, HUD toast
    Audio/, VFX/     GameAudio stems; pooled pixel VFX systems
  Prefabs/
    Stages/          PF_Stage_Base + PF_Stage_1..6 variants — ALL level content
    Altars/, Hazards/, Environment/, UI/
  Art/               Production sprites (+ Aseprite sources in Art/Source)
  Data/Costs/        The four CostData assets (offer copy, type, value)
Docs/                Living docs + authoritative planning sources (see Docs/README.md)
```

## How the game fits together

- **Stages are prefab variants**: `PF_Stage_Base` holds the shared skeleton
  (floor, walls, two altars wired to a ChoiceGate, EndMarker); each
  `PF_Stage_N` variant adds its hazard layout and overrides the two altar
  offers. Edit level content there, never in the scene.
- **Pacing is derived**: `StageDirector.stageDurations` (seconds per stage)
  divided by actual travel distance gives scroll speed. Change stage length
  or durations independently; speed follows.
- **The gate holds**: a stage cannot complete until a choice is accepted.
  Entering an altar opens the confirmation popup; costs apply exactly once;
  each paid sacrifice also heals 1 HP.
- **Benefits are one-shot**: `BenefitApplier` applies each accepted offer's
  relief to the next stage only (Light removes the first hazard, Body slows
  blades, Blood narrows spikes, Sight slows the scroll) and undoes it after.
- **Endings**: below 50 relic light at the end = Dark ("Villain"), otherwise
  Light ("Hero"). Light only drains through the three Dim-the-Relic offers —
  keep Light in exactly three gates or the Dark ending becomes unreachable.

## Before changing stage layouts

Read `Docs/Fairness_Matrix.md` first. It documents hard invariants (blade
amplitude ≤ 2.0, twin spikes at ±2.75, one safe lane per row) and contains
the analytical model to re-verify passability at worst-case modifiers.

## Dev conveniences (development builds and editor only)

- **God mode**: tick the checkbox on the Player's `PlayerHealth`, or tap the
  top-left HP label five times quickly in a build. Hazards stop hurting;
  altars still work. For recording footage.
- **Logging**: the core loop logs every event (stage start/complete with
  timings, offers opened/declined/accepted with the rejected option, costs
  with full run-state snapshots, benefits applied, deaths by stage). A whole
  run can be reconstructed from logcat: `adb logcat -s Unity`.

## Known quirks

- This editor setup may not tick frames in Play Mode when driven remotely;
  time-based logic is verified by calling `Tick()` methods via reflection.
- The TextMesh Pro fallback font asset accumulates glyph-cache churn in the
  working tree; it is safe to discard or commit.
