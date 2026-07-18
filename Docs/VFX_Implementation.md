# Last Light — VFX Implementation

Status: complete pass, validated in-editor, **uncommitted by request** (a concurrent
audio session shares this working tree; commit coordination is manual).
Motto enforced throughout: **"Every gain flashes briefly. Every loss remains."**

## Systems

| System | Script (Assets/Scripts/VFX/) | Lives on | Trigger |
|---|---|---|---|
| Carried light core | `LightCoreView.cs` (pre-existing, verified) | PF_Traveller/VisualRoot | `RelicLight.LightChanged` |
| Sparks + discrete trail | `LightSparksView.cs` | VisualRoot/LightVFX | `RelicLight.LightChanged` + LateUpdate |
| Damage feedback | `DamageVFXView.cs` | VisualRoot/DamageVFX | `PlayerHealth.Damaged(Vector2)` (new) |
| Sacrifice transfer | `SacrificeTransferVFX.cs` | VisualRoot/SacrificeVFX | `ChoiceGate.AltarChosen` (new) |
| Risk acceptance | `RiskAcceptanceVFX.cs` | VisualRoot/SacrificeVFX | null-offer altar via transfer view |
| Persistent losses | `PersistentSacrificeView.cs` | VisualRoot/PersistentLossVisuals | `SacrificeTransferVFX.TransferCompleted` |
| Pixel vignette | `PixelVignetteView.cs` | HUDCanvas/VignetteRoot | `PersistentSacrificeView` (sight) |
| HUD loss | `HUDLossVFX.cs` | HUDCanvas | **hook only** — `LoseElement(HUDElement)`; no gameplay cost removes HUD info yet |
| Altar effects | `AltarVFXView.cs` | both altar prefabs (VFXAnchor/AltarVFX) | gate events + per-frame proximity |
| Hazard telegraphs | `HazardTelegraphVFX.cs` | both hazard prefabs (TelegraphAnchor) | position-driven; warning starts at camera visible top + `warningLeadDistance` (2), mirroring ObstacleBase's audio trigger (fixed `warningY`=12 only as no-camera fallback) |
| Section transitions | `SectionTransitionVFX.cs` | HUDCanvas | `StageDirector.StageChanged` |
| World degradation | `WorldDegradationVFX.cs` | HUDCanvas | StageChanged + AltarChosen |
| Death + reassembly | `DeathReassemblyVFX.cs` | VisualRoot/DeathVFX | `PlayerHealth.Died` + `ResetRun` |
| Final transfer (3 tiers) | `FinalLightTransferVFX.cs` | VisualRoot/FinalVFX | `EndingResolver.RunEnded` |
| Screen overlays | `ScreenFXOverlay.cs` | HUDCanvas/ScreenFXRoot | called by the above |
| Shared | `LastLightVFXSettings.cs`, `FixedSpritePool.cs`, `SpriteFlash.cs`, `CameraImpulse2D.cs`, `PixelPositionUtility.cs` | — | — |

Settings asset: `Assets/Data/VFX/LastLightVFXSettings.asset` (palette, thresholds
50/20, PPU 16, `motionScale`).

## Gameplay files touched for VFX (all additive; audio session's edits preserved)

- `PlayerHealth.cs` — `Damaged(Vector2 hitDirection)` presentation event + `ApplyDamage(int, Vector2 sourcePos)` overload.
- `ObstacleBase.cs` — passes `transform.position` into the new overload.
- `ChoiceGate.cs` — `AltarChosen(AltarTrigger)` presentation event + `AltarA`/`AltarB` getters.

**Known gameplay gap (not fixed, by policy):** nothing subscribes
`ChoiceGate.CostChosen` → `CostApplier.Apply` is never called, so costs don't
actually apply. VFX intentionally integrates through `AltarChosen`
so all effects fire regardless; wire `CostChosen → CostApplier.Apply` when ready
(PersistentSacrificeView tracks its own totals and will stay in sync visually).

## Final tiers (no ending logic invented)

`EndingResolver` is binary (Dark/Light at 0 light). Visual tiers:
Dark → **bad**; Light with ≥50 remaining → **good**; otherwise **medium**.
Threshold serialized (`goodLightThreshold`). Recipient transform is serialized and
optional (defaults to 4u above the player). Note: `OutcomeScreen` shows its panel
on the same event — check overlap during device review.

## Pools & budgets (all prefab-authored, start disabled, zero runtime Instantiate/Destroy)

sparks 8 · trail 3 · debris 8 · sacrifice 12 · risk lines 6+wave · altar motes 4/altar
· link dashes 3/altar · dust 3/spike · blade edges 2 · death fragments 8 · final motes 8.
Camera impulse ≤80 ms; damage edge ≤0.12 s; hit-stop 80 ms (Play-Mode-only, serialized, 0 disables).

## Placeholders (ALL current VFX art)

All 38 sprites under `Assets/Art/VFX/**` are deterministic generated placeholders
(single-sprite PNGs, palette-exact, PPU 16/Point/uncompressed via
`Assets/Editor/VFXTexturePostprocessor.cs` — scoped to `Assets/Art/VFX/` only).
Replace by overwriting the PNG at the same path — no prefab rewiring needed.
Aseprite pipeline: `Tools/Aseprite/` (`vfx_manifest.lua`, `create_vfx_templates.lua`,
`validate_vfx_palette.lua`, `export_vfx.lua`, `export_vfx.sh|.ps1`). Aseprite CLI was
not installed on this machine; run `Tools/Aseprite/export_vfx.sh` once it is —
templates are created into `Art/VFX/` without overwriting existing sources.

## Reduced motion

`LastLightVFXSettings.motionScale` (0.25–1, default 1; runtime hook
`SetMotionScale`). Scales camera impulse, risk-line speed, fragment travel.
Telegraph colors/shapes unaffected.

## Audio-loss visual hook

`PersistentSacrificeView.PlaySensoryCollapse()` — purple two-tick body flash.
No audio-layer cost exists yet; call it when one is added.

## Validation notes

Everything above was validated **in-editor** by driving public `Tick(dt)` /
`ApplyX()` methods on loaded prefab contents (this editor does not tick Play Mode
reliably, and it is shared with the audio session — no Play Mode, no scene loads).
Not visually verified: real-time composition at 360x640, overlap of final VFX
with the outcome panel, and on-device performance. Reflection was used only to
drive private handlers in validation, never in runtime code.

## Human review checklist

- [ ] Full / low / critical / zero light (core pulse, spark density, trail 3/2/1/0)
- [ ] Damage: white flash, contact-side burst, debris, edge frame, core lag, impulse
- [ ] Pay sacrifice per category (Blood/Light/Sight/Body): pull-to-altar + permanent mark
- [ ] Accept risk: cyan collapse, red top lines, wave stops above input area
- [ ] Vision loss levels 1-3 + severe jitter + reset
- [ ] HUD loss via `HUDLossVFX.LoseElement` (manual trigger for now)
- [ ] Body crack (1st blood), missing pixels (2nd), spark-energy drop (speed)
- [ ] Spike crack 3-tick warning + dust; blade leading edge + reversal squash
- [ ] Six sections: scanline accents cyan→purple, degradation tint steps
- [ ] Death burst → converge → yellow spark, repeated deaths, no stale VFX
- [ ] Good / medium / bad finals (force via light value before the last stage)
- [ ] 360x640 readability, lower input third clear, no subpixel shimmer
