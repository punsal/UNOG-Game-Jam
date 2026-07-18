# Technical Art Decisions

Source of truth: `Docs/Last_Light_Technical_Art_Baseline.pdf`. This file records approved deviations and standing production rules. If this file and the PDF disagree, this file wins for the items listed below.

## Approved deviation: dual reference resolution

| Reference | Value | Used for |
|---|---|---|
| World camera (Pixel Perfect Camera) | 288 x 512 | Game-world framing, gameplay tuning (e.g. `PlayerMotor.halfWidth`) |
| UI Canvas (CanvasScaler) | 360 x 640 | HUD / UI layout only, once a Canvas is added |
| PPU (both) | 16 | Sprite import and world-unit scale |

**Reason:** 288x512 at 16 PPU resolves to an exact 18x32 Unity-unit viewport and preserves the project's existing gameplay tuning (camera framing and `PlayerMotor` movement bounds), which was already built and verified against 288x512 (see commit `5aaa0d8`, LL-001). The PDF's 360x640 remains the mobile UI/layout reference and will be used as the CanvasScaler reference resolution when the UI Canvas is introduced. Both values share the same 9:16 aspect ratio, so this is a scale choice, not an aspect-ratio deviation.

Source asset sizes (traveller 24x24, altars 32x48, spike/floor/wall modules 16x16, etc.) are unchanged from the PDF — this deviation only affects the world camera's reference resolution, not sprite dimensions or PPU.

## Production gating (unchanged from PDF)

- G1-G3 continue using primitives/placeholders. No visual feature blocks movement, collision, hazards, choices, or the ending.
- P0 art integration begins only after the end-to-end build (G3) is stable on-device.
- Art is integrated one asset package at a time, not all at once.

## Asset provenance rule

No PixelLab-generated asset is production-ready as-is. Every generated asset requires technical audit (size, PPU, pivot, alpha rules per the PDF) and manual normalization before it leaves `Art/Generated/` for `Art/Production/`.
