# Last Light - DECISIONS v2

## Locked Product Decisions

- Portrait Unity 2D auto-runner.
- One-finger horizontal drag control.
- Six fixed stages.
- Target run length: under 3 minutes total (jam rule, confirmed 2026-07-19; supersedes the earlier 3-5 minute target).
- Health and relic light are separate resources.
- Two physical offers appear at the end of a stage; the stage holds at the gate until a choice is accepted.
- Altars display only their offer icon in the world.
- Entering an altar opens a confirmation popup showing the offer's icon, benefit, and cost, with Accept and Decline.
- Accepting commits exactly one choice, applies its effects, and resumes the run.
- Declining closes the popup, returns the traveller to the corridor centre, and leaves both altars armed.
- (Superseded 2026-07-19: "Entering an altar commits exactly one choice; there is no confirmation button." Reversed because gates became mandatory with a hold — accidental drags near two armed triggers made instant commitment a UX hazard, and readable benefit/cost comparison needs more space than the in-world cards.)
- Permanent costs use bounded modifiers.
- MVP hazards are spike wall and moving blade.
- Two endings are required; a third is conditional.
- Android APK is the submission platform (jam rules confirmed 2026-07-19; no WebGL requirement).
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
