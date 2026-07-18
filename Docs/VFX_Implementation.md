# Last Light — VFX Implementation

Status: complete pass, **live Play-Mode validated** (2026-07-19) on branch
`feature/complete-vfx-pass`. The sacrifice gameplay flow is now wired and
verified end-to-end. Motto enforced throughout: **"Every gain flashes briefly.
Every loss remains."**

## Systems

| System | Script (Assets/Scripts/VFX/) | Lives on | Trigger |
|---|---|---|---|
| Carried light core | `LightCoreView.cs` (pre-existing, verified) | PF_Traveller/VisualRoot | `RelicLight.LightChanged` |
| Sparks + discrete trail | `LightSparksView.cs` | VisualRoot/LightVFX | `RelicLight.LightChanged` + LateUpdate |
| Damage feedback | `DamageVFXView.cs` | VisualRoot/DamageVFX | `PlayerHealth.Damaged(Vector2)` |
| Sacrifice transfer | `SacrificeTransferVFX.cs` | VisualRoot/SacrificeVFX | `ChoiceGate.AltarChosen` |
| Risk acceptance | `RiskAcceptanceVFX.cs` | VisualRoot/SacrificeVFX | null-offer altar (**content-inactive**, see below) |
| Persistent losses | `PersistentSacrificeView.cs` | VisualRoot/PersistentLossVisuals | `SacrificeTransferVFX.TransferCompleted` |
| Pixel vignette | `PixelVignetteView.cs` | HUDCanvas/VignetteRoot | `PersistentSacrificeView` (sight) |
| HUD loss | `HUDLossVFX.cs` | HUDCanvas | **hook only** — no HUD cost type exists |
| Altar effects | `AltarVFXView.cs` | both altar prefabs (VFXAnchor/AltarVFX) | gate events + per-frame proximity |
| Hazard telegraphs | `HazardTelegraphVFX.cs` | both hazard prefabs (TelegraphAnchor) | position-driven; warning starts at camera visible top + `warningLeadDistance` (2), mirroring ObstacleBase's audio trigger |
| Section transitions | `SectionTransitionVFX.cs` | HUDCanvas | `StageDirector.StageChanged` |
| World degradation | `WorldDegradationVFX.cs` | HUDCanvas | StageChanged + AltarChosen |
| Death + reassembly | `DeathReassemblyVFX.cs` | VisualRoot/DeathVFX | `PlayerHealth.Died` + `ResetRun` |
| Final transfer (3 tiers) | `FinalLightTransferVFX.cs` | VisualRoot/FinalVFX | `EndingResolver.RunEnded` |
| Screen overlays | `ScreenFXOverlay.cs` | HUDCanvas/ScreenFXRoot | called by the above |
| Shared | `LastLightVFXSettings.cs`, `FixedSpritePool.cs`, `SpriteFlash.cs`, `CameraImpulse2D.cs`, `PixelPositionUtility.cs` | — | — |

Settings asset: `Assets/Data/VFX/LastLightVFXSettings.asset` (palette, thresholds
50/20, PPU 16, `motionScale` 0.25–1 reduced-motion hook via `SetMotionScale`).

## Gameplay integration (verified live)

- `PlayerHealth.cs` — `Damaged(Vector2)` presentation event + `ApplyDamage(int, Vector2)` overload.
- `ObstacleBase.cs` — passes `transform.position` into the overload.
- `ChoiceGate.cs` — `AltarChosen(AltarTrigger)` presentation event + `AltarA/AltarB` getters.
- `CostApplier.cs` — **sacrifice flow fix**: subscribes once (Start/OnDestroy) to every
  `ChoiceGate.CostChosen`; null offers (risk) pay nothing. Verified live: each cost
  applies exactly once, second altar ignored, resets don't re-apply, audio calls preserved.

**Restart is delayed, not synchronous:** `RunController` restarts 0.45 s after death
(audio sting window, input disabled meanwhile). `DeathReassemblyVFX.ResetRun`
keeps an active death timeline across that reset; idle resets clear everything.

## Content notes (facts, not bugs)

- **Every scene gate offers two costs** (no null-offer altar exists), so the risk
  path (`RiskAcceptanceVFX`, altar collapse reaction) is *content-inactive*. The
  code is live-ready: any altar left with `offer = None` becomes a true risk altar.
  Altar chosen-reactions follow the actual offer (absorb when a cost is paid),
  not the prefab's idle styling (Pay=yellow inward motes / Risk=cyan rising motes).
- Cost data: `CD_Blood -1`, `CD_Body -0.1` (speed), `CD_Light -20`, `CD_Sight -0.2`.
  No positive Body value exists → **size-penalty ring inactive** (functional, no data).
- No HUD or audio-layer cost type exists → `HUDLossVFX.LoseElement` and
  `PersistentSacrificeView.PlaySensoryCollapse()` are inactive hooks.
- Endings are binary (Dark/Light at 0). Visual tiers: Dark→bad; Light ≥50→good;
  else medium (`goodLightThreshold` serialized).

## Play Mode validation performed (MainScene, live frames)

Editor gotcha solved: set `Application.runInBackground = true` in Play Mode —
without it this editor's player loop freezes while unfocused.

- **Light states:** idle glow pulse 0.5↔0.5625 white core; low 45 → amber glow,
  base 0.375; critical 15 → ember glow 0.3125, core flicker observed (white and
  ember samples); zero → sparks 0, trail 0, min ember footprint, no yellow left.
  Trail hidden while stationary.
- **Damage:** left and right hits → hp changed, 5 debris, burst on contact side,
  white flash + restore, camera restored to origin, edge frame cleared,
  `timeScale` 1, rapid second hit correctly blocked by invulnerability.
- **Pay chain (real gates):** Sight → sightMult 1→0.8, 9 transfer particles,
  hit-stop engaged and restored, vignette level 2 from real state. Body(speed) →
  motor 0.9 and spark energy 0.9 in lockstep. Blood → maxHP 3→2, crack overlay on
  (missing-pixel overlay correctly waits for a 2nd Blood). Light → 100→80.
  Exactly-once verified (second altar invocation ignored). Degradation stepped to 3.
- **Hazards:** blade telegraph leading-edge follows real oscillation (right edge
  while moving right); spike crack observed live at full brightness in the
  imminent window, warning began above the visible top edge.
- **Sections:** stage 1 scanline with section accent; degradation quantized.
- **Death/restart:** 8 fragments + rising/extinguishing ember at death position;
  delayed 0.45 s restart verified; multi-minute unattended soak (repeated organic
  deaths/restarts/altar hits) produced zero console errors and stable timeScale.
- **Finals:** good = 8 motes + 0.10-alpha broad tint, cleared after rise;
  medium = 6 motes; bad = 1 pixel + navy dark closure 0.55, restart clears.
  Outcome panel shows over the VFX (UI is a separate canvas above ScreenFX).
- **Composition screenshot** (fresh run): silhouette readable, relic distinct,
  HUD crisp, no blur, input area clear.

Not observed live: risk acceptance in real content (no data — synthetic only),
trail while moving under real drag input (verified synthetically; no touch input
in editor), device aspect ratios other than the game view, on-device performance,
Profiler GC capture (static review only: pooled renderers, no per-frame
allocations/LINQ/`.material`, one-time `Find*` in `Start` only).

## Assets

All 38 sprites under `Assets/Art/VFX/**` are deterministic placeholders
(single-sprite PNGs, palette-exact). Import enforced by
`Assets/Editor/VFXTexturePostprocessor.cs` (scoped to `Assets/Art/VFX/` only):
Sprite/**Single**, PPU 16, Point, Uncompressed, no mips, Clamp, alpha transparency,
FullRect. Replace art by overwriting the PNG at the same path — verified: no
prefab rewiring needed (references were re-verified after the Single-mode fix).
Aseprite pipeline in `Tools/Aseprite/`; CLI not installed — when available run
`Tools/Aseprite/export_vfx.sh` (templates never overwrite existing sources).

## Pools & budgets

sparks 8 · trail 3 · debris 8 · sacrifice 12 · risk lines 6+wave · altar motes
4/altar · link dashes 3/altar · dust 3/spike · blade edges 2 · death fragments 8
· final motes 8. Camera impulse ≤80 ms; damage edge ≤0.12 s; hit-stop 80 ms
(Play-Mode-only). Sorting: order offsets inside the traveller SortingGroup
(trail 4 < core 5 < sparks 6 < transfer 7 < final 9); no new sorting layers.

## Human review checklist (visual pass on device)

- [ ] Light states in motion; trail 3/2/1/0 while dragging
- [ ] Damage from both sides during real dodging
- [ ] Each cost's transfer + permanent mark at real altars
- [ ] Spike crack timing vs actual dodge difficulty; blade edge readability
- [ ] Six-section scanline accents and degradation feel
- [ ] Death fragments + reassembly during the 0.45 s restart window
- [ ] All three finals vs the outcome panel (overlap acceptable?)
- [ ] 360x640 + one taller aspect; lower input third clear; no shimmer
- [ ] Placeholder art replacement priority: spark/trail, damage burst, telegraphs
