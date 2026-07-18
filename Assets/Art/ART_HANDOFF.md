# Art Handoff — P0 Pack

Jam-speed batch pass. See `Docs/Last_Light_Technical_Art_Baseline.pdf` and `TECHNICAL_ART_DECISIONS.md` for full rules. This file is the quick-reference index.

## Production assets

| Asset | Path | Size | Pivot |
|---|---|---|---|
| Traveller | `Art/Production/Characters/chr_traveller_base.png` | 24x24 | Bottom Center |
| Relic core | `Art/Production/VFX/fx_relic_core.png` | 8x8 | Center |
| Relic glow (inner) | `Art/Production/VFX/fx_relic_glow_inner.png` | 16x16 | Center |
| Spike module | `Art/Production/Hazards/haz_spike_module.png` | 16x16 | Bottom Center |
| Moving blade | `Art/Production/Hazards/haz_blade_base.png` | 32x32 | Center |
| Floor (clean/cracked) | `Art/Production/Environment/env_floor_clean.png`, `env_floor_cracked.png` | 16x16 | Center |
| Wall edge L/R, cap | `Art/Production/Environment/env_wall_edge_left.png`, `_right.png`, `env_wall_cap.png` | 16x16 | Center |
| Void strip | `Art/Production/Environment/env_void_strip.png` | 16x32 | Center |
| Altars (safe/cost) | `Art/Production/Altars/altar_safe_idle.png`, `altar_cost_idle.png` | 32x48 | Bottom Center |
| Icons (5) | `Art/Production/UI/ui_icon_{health,light,speed,vision,body_size}.png` | 24x24 | Center |

All: PRESET_PixelArt_World applied (Point filter, PPU 16, Compression None, Mipmaps Off, Full Rect).

## Prefabs

| Prefab | Path | Sorting |
|---|---|---|
| Traveller (+relic) | `Prefabs/Characters/PF_Traveller.prefab` | World/60; GlowInner order -2, TravellerBase 0, RelicCore 5 |
| Spikes | `Prefabs/Hazards/PF_Hazard_Spikes.prefab` | World/50 |
| Blade | `Prefabs/Hazards/PF_Hazard_Blade.prefab` | World/50 |
| Altar Safe/Cost | `Prefabs/Altars/PF_Altar_Safe.prefab`, `PF_Altar_Cost.prefab` | World/40 |
| Floor | `Prefabs/Environment/PF_Environment_Floor.prefab` | World/0 |
| Wall L/R | `Prefabs/Environment/PF_Environment_WallLeft.prefab`, `WallRight.prefab` | World/20 |
| Void strip | `Prefabs/Environment/PF_Environment_VoidStrip.prefab` | Background/0 |

All prefab roots: **Transform only**. No gameplay scripts, Collider2D, Rigidbody2D, or Animator anywhere in any prefab — confirmed by hierarchy dump (component types are only `Transform`, `SortingGroup`, `SpriteRenderer`).

## Empty anchors (reserved for gameplay wiring)

- `PF_Hazard_Spikes`: `ColliderAnchor`, `TelegraphAnchor`
- `PF_Hazard_Blade`: `ColliderAnchor`, `PivotAnchor` (rotation center), `TelegraphAnchor`
- `PF_Altar_Safe` / `PF_Altar_Cost`: `TriggerAnchor`, `ChoiceLabelAnchor` (y=3.3, above altar), `VFXAnchor` (y=1.94, on the inset gem)

## Scene

`Assets/Scenes/ArtTech_Test.unity` — camera at (0,0,-10) with `PixelPerfectCamera` matching the approved 288x512/16PPU reference, one of each prefab placed plus a 7-tile floor row and the 5 icons, for a visual sanity pass. `MainScene.unity` and `Bootstrap.unity` were not touched.

## Confirmed

- Zero gameplay scripts/physics in any new prefab or scene object.
- `git diff` against `MainScene.unity`, `Bootstrap.unity`, `Assets/Scripts/`, `ProjectSettings/`, `Packages/manifest.json` — all empty.
- Unity Console: no new errors (only benign editor/MCP housekeeping messages).

## Limitations / fallbacks

- **BladeBody / Hub**: only one combined blade+hub texture was generated. `BladeBody` carries the full image; `Hub` is an empty anchor (no separate hub texture) reserved for future independent tint/flash art.
- **Altar Base / Inset**: same pattern — `Base` carries the full altar texture (body+gem already composited); `Inset` is an empty anchor positioned exactly on the gem, reserved for future independent pulsing/tint.
- **Icon colors**: Health = Danger Red, Vision = Shadow Purple (both fit the established palette meanings); Light/Speed/BodySize = Bone (neutral — Gold/Cyan are reserved for relic and safety respectively, and none of those three icons clearly earn that reservation).
- **ArtTech_Test screenshot check**: this automation environment reports a degenerate Game View size (`Screen.height=72`), so `PixelPerfectCamera` couldn't compute its real crop for the verification screenshot — individual assets were confirmed readable, but the full 18x32-world-unit framing could only be spot-checked, not screenshotted end-to-end. Camera/prefab setup itself matches the approved reference and will frame correctly in a real Editor window or device build.
- **Altar pair**: cost altar's stone body is derived from the safe altar's quantized silhouette (guaranteeing identical outer mass per spec) rather than independently normalized from its own PixelLab generation — the raw cost reference is kept in `Art/Generated/References/ART-05/` for comparison.
