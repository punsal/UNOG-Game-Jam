# Last Light - TASKS v2

## Working Agreement

1. Read `DECISIONS.md` before implementation.
2. Use the approved **Architecture and 48h Plan** as the implementation baseline.
3. Use the approved **AI Art Prompt & Asset Production Plan** as the art execution baseline.
4. G1-G3 must use primitives or placeholders. Gameplay may not wait for art generation, cleanup, animation, or import.
5. P0 art integration begins only after G3 passes and must not endanger G4 mobile usability.
6. P0 art receives an initial four-hour integration timebox. When a package exceeds its estimate, use its documented fallback and continue.
7. P1 and P2 art are optional. The shadow hazard, music, expanded environment, and additional animation are cut immediately when the schedule slips.
8. Do not add features outside the locked scope. New ideas go to the Parking Lot unless they replace existing work through a PM decision.
9. Feature freeze begins at Hour 38. After that point, only blocker, build, device, and readability fixes are allowed.

## Approved Specialist Plans

- Art: https://docs.google.com/spreadsheets/d/13zvYWP2P-KUD3PrtktCfC7JhIwMcNdx7FKuwHPENju8/edit
- Development: https://docs.google.com/spreadsheets/d/14eTCn-Jw3IXBu6H8PLdYbzMCBc6id-sApQzwv1cmIoI/edit
- PM Board: https://docs.google.com/spreadsheets/d/1fESbpXml7lgn3yb6luROjUXm_fwznk8u55EFGcqUB3k/edit

## Critical Path

1. Player Controller
2. Health and Damage
3. Obstacles
4. Level Flow
5. Choice Gate
6. Cost Application
7. Relic Light
8. Ending Resolver
9. UI
10. Minimum Art and Audio
11. QA and Submission

## Task List

| ID | Task | Priority | Estimate | Dependency | Gate |
|---|---|---:|---:|---|---|
| LL-001 | Create portrait project baseline and Pixel Perfect camera | Must | 1h | None | G1 |
| LL-002 | Implement touch and mouse horizontal drag movement | Must | 2h | LL-001 | G1 |
| LL-003 | Implement health, damage and invulnerability | Must | 2h | LL-002 | G1 |
| LL-004 | Implement death and fast restart | Must | 1.5h | LL-003 | G1 |
| LL-005 | Build reusable obstacle base and collision setup | Must | 1.5h | LL-003 | G1 |
| LL-006 | Create spike wall prefab and variations | Must | 1h | LL-005 | G1 |
| LL-007 | Create moving blade prefab | Must | 1.5h | LL-005 | G1 |
| LL-008 | Implement stage start, completion and sequence flow | Must | 2h | LL-004, LL-006 | G2 |
| LL-009 | Create CostData ScriptableObject | Must | 1h | LL-008 | G2 |
| LL-010 | Implement physical two-offer ChoiceGate | Must | 2h | LL-008, LL-009 | G2 |
| LL-011 | Implement CostApplier and modifier caps | Must | 2.5h | LL-009, LL-010 | G2 |
| LL-012 | Implement relic light resource | Must | 1h | LL-011 | G2 |
| LL-013 | Implement ending resolver and outcome screen | Must | 2h | LL-012 | G3 |
| LL-014 | Assemble six fixed stage chunks | Must | 4h | LL-007, LL-008, LL-011 | G3 |
| LL-015 | Create start screen and micro-tutorial | Must | 1h | LL-002 | G3 |
| LL-016 | Create HUD for health, stage and light | Must | 1.5h | LL-003, LL-012 | G3 |
| LL-017 | Create concise decision cards | Must | 1.5h | LL-009, LL-010 | G3 |
| LL-018 | Run first real-device input test | Must | 1h | LL-002, LL-004 | G4 |
| LL-019 | Pass min-speed and max-body fairness matrix | Must | 1.5h | LL-011, LL-014 | G5 |
| LL-020 | Integrate P0 mobile-minimum art package | Should | 4h | G3 passed; approved Art Plan P0 | G5 |
| LL-021 | Add essential SFX and impact feedback | Should | 1.5h | G3 passed | G5 |
| LL-022 | Run external playtest and decision review | Must | 1.5h | G3 passed | G5 |
| LL-023 | Feature freeze and blocker-only pass | Must | 4h | G5 passed | G6 |
| LL-024 | Create final build and submission assets | Must | 3h | LL-023 | G6 |
