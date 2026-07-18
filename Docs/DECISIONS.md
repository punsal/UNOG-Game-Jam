# Last Light - DECISIONS v2

## Locked Product Decisions

- Portrait Unity 2D auto-runner.
- One-finger horizontal drag control.
- Six fixed stages.
- Target run length: 3-5 minutes.
- Health and relic light are separate resources.
- Two physical offers appear at the end of a stage.
- Entering an altar commits exactly one choice; there is no confirmation button.
- Permanent costs use bounded modifiers.
- MVP hazards are spike wall and moving blade.
- Two endings are required; a third is conditional.
- Android is the default build unless jam rules require WebGL.
- Feature freeze begins at Hour 38.

## Approved Technical Baseline

The development architecture spreadsheet is approved. Its one-scene, fixed-prefab, immutable-config, mutable-RunState, centralized-reset approach is the implementation baseline. Technical substitutions are permitted only when they preserve the locked product behavior and do not expand scope.

## Approved Art Baseline

The art production spreadsheet is approved. G1-G3 remain placeholder-driven. P0 art is integrated after G3 with an initial four-hour timebox. Every P0 item must retain a usable fallback. P1/P2 remain optional.

## Explicit Non-Goals

- Combat
- Procedural generation
- Inventory
- Dialogue trees
- Character classes
- Online features
- Complex meta-progression
- Any feature that delays the critical path
