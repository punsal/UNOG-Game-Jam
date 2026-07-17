# Last Light - TASKS

Authoritative implementation queue for Claude Code and the developer.

## Workflow

Use statuses: `Backlog -> Ready -> In Progress -> Review/Test -> Done`.
Do not begin a task until its dependencies are complete. Mark blockers explicitly.

## [ ] LL-001 - Create portrait project baseline and Pixel Perfect camera

- **Epic:** E1 Core Controller
- **Priority:** Must
- **Initial status:** Ready
- **Estimate:** 1 h
- **Dependencies:** None
- **Milestone:** G1
- **Implementation note:** Critical path

**Acceptance criteria**

- Scene runs at 9:16 reference resolution with correct camera framing and no Console errors.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-002 - Implement touch and mouse horizontal drag movement

- **Epic:** E1 Core Controller
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 2 h
- **Dependencies:** LL-001
- **Milestone:** G1
- **Implementation note:** No inertia or joystick

**Acceptance criteria**

- A first-time player can move left and right within 10 seconds using mouse or one finger.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-003 - Implement health, damage and invulnerability

- **Epic:** E2 Survival Loop
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 2 h
- **Dependencies:** LL-002
- **Milestone:** G1
- **Implementation note:** Start health 3

**Acceptance criteria**

- Obstacle contact removes one health and repeated contact is blocked for 0.8-1.0 seconds.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-004 - Implement death and fast restart

- **Epic:** E2 Survival Loop
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** LL-003
- **Milestone:** G1
- **Implementation note:** Protected core

**Acceptance criteria**

- At zero health the run ends and a new run begins within five seconds with all state reset.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-005 - Build reusable obstacle base and collision setup

- **Epic:** E3 Obstacle Framework
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** LL-003
- **Milestone:** G1
- **Implementation note:** Critical path

**Acceptance criteria**

- Obstacle prefabs damage the player once per valid contact and reset correctly.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-006 - Create spike wall prefab and variations

- **Epic:** E3 Obstacle Framework
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1 h
- **Dependencies:** LL-005
- **Milestone:** G1
- **Implementation note:** MVP obstacle 1

**Acceptance criteria**

- Spike wall has a clearly readable safe corridor and configurable gap position and width.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-007 - Create moving blade prefab

- **Epic:** E3 Obstacle Framework
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** LL-005
- **Milestone:** G1
- **Implementation note:** MVP obstacle 2

**Acceptance criteria**

- Blade moves horizontally with configurable speed and phase and remains visually readable.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-008 - Implement stage start, completion and sequence flow

- **Epic:** E4 Level Flow
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 2 h
- **Dependencies:** LL-004, LL-006
- **Milestone:** G2
- **Implementation note:** Critical path

**Acceptance criteria**

- A stage starts, progresses automatically, completes once, and transitions without soft lock.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-009 - Create CostData ScriptableObject

- **Epic:** E5 Choice & Cost
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1 h
- **Dependencies:** LL-008
- **Milestone:** G2
- **Implementation note:** Stable public API

**Acceptance criteria**

- An offer stores icon, short copy, benefit, cost or risk type, and value without scene-specific logic.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-010 - Implement physical two-offer ChoiceGate

- **Epic:** E5 Choice & Cost
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 2 h
- **Dependencies:** LL-008, LL-009
- **Milestone:** G2
- **Implementation note:** No confirmation button

**Acceptance criteria**

- Dragging into an altar selects exactly one offer, applies it once, and advances the flow.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-011 - Implement CostApplier and modifier caps

- **Epic:** E5 Choice & Cost
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 2.5 h
- **Dependencies:** LL-009, LL-010
- **Milestone:** G2
- **Implementation note:** Blood, Light, Sight, Body

**Acceptance criteria**

- At least one permanent cost visibly affects the next stage and all modifiers respect documented limits.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-012 - Implement relic light resource

- **Epic:** E6 Relic & Endings
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1 h
- **Dependencies:** LL-011
- **Milestone:** G2
- **Implementation note:** Separate from health

**Acceptance criteria**

- Light starts at 100%, updates through events, may reach 0%, and resets on restart.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-013 - Implement ending resolver and outcome screen

- **Epic:** E6 Relic & Endings
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 2 h
- **Dependencies:** LL-012
- **Milestone:** G3
- **Implementation note:** Target 3, minimum 2

**Acceptance criteria**

- At least two endings trigger correctly from remaining-light thresholds and show Retry.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-014 - Assemble six fixed stage chunks

- **Epic:** E4 Level Flow
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 4 h
- **Dependencies:** LL-007, LL-008, LL-011
- **Milestone:** G3
- **Implementation note:** Use placeholders first

**Acceptance criteria**

- The game completes from opening through six stages to an ending in under five minutes.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-015 - Create start screen and micro-tutorial

- **Epic:** E7 UX/UI
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1 h
- **Dependencies:** LL-002
- **Milestone:** G3
- **Implementation note:** Short English copy

**Acceptance criteria**

- Tap starts the run and the drag hint disappears after first movement.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-016 - Create HUD for health, stage and light

- **Epic:** E7 UX/UI
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** LL-003, LL-012
- **Milestone:** G3
- **Implementation note:** Safe area required

**Acceptance criteria**

- HUD reflects health, current stage and light without containing game logic.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-017 - Create concise decision cards

- **Epic:** E7 UX/UI
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** LL-009, LL-010
- **Milestone:** G3
- **Implementation note:** Readability gate

**Acceptance criteria**

- Each card shows an icon, one benefit line, and Cost or Risk copy within roughly 8-12 words.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-018 - Run first real-device input test

- **Epic:** E9 QA & Submission
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1 h
- **Dependencies:** LL-002, LL-004
- **Milestone:** G4
- **Implementation note:** Mandatory early test

**Acceptance criteria**

- Input, safe area and frame rate are acceptable on at least one Android device.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-019 - Pass min-speed and max-body fairness matrix

- **Epic:** E9 QA & Submission
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** LL-011, LL-014
- **Milestone:** G5
- **Implementation note:** Block release if failed

**Acceptance criteria**

- Every stage is passable at minimum allowed movement speed and maximum character size.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-020 - Replace critical placeholders with minimum pixel-art set

- **Epic:** E8 Art/Audio Polish
- **Priority:** Should
- **Initial status:** Backlog
- **Estimate:** 4 h
- **Dependencies:** G3 passed
- **Milestone:** G5
- **Implementation note:** Must not block critical path

**Acceptance criteria**

- Traveller, light, two obstacles, altar and minimum ending visuals are readable and integrated.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-021 - Add essential SFX and impact feedback

- **Epic:** E8 Art/Audio Polish
- **Priority:** Should
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** G3 passed
- **Milestone:** G5
- **Implementation note:** Music optional

**Acceptance criteria**

- Damage, cost selection and stage transition have clear feedback; shadow remains readable muted.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-022 - Run external playtest and decision review

- **Epic:** E9 QA & Submission
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 1.5 h
- **Dependencies:** G3 passed
- **Milestone:** G5
- **Implementation note:** 5-10 rapid tests

**Acceptance criteria**

- Record control comprehension, cost clarity, fairness, run time and retry intent; convert findings into prioritized fixes.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-023 - Feature freeze and blocker-only pass

- **Epic:** E9 QA & Submission
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 4 h
- **Dependencies:** G5 passed
- **Milestone:** G6
- **Implementation note:** Begins with 10 hours remaining

**Acceptance criteria**

- Only blocker, build and readability fixes are accepted; zero blocker bugs remain.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.

## [ ] LL-024 - Create final build and submission assets

- **Epic:** E9 QA & Submission
- **Priority:** Must
- **Initial status:** Backlog
- **Estimate:** 3 h
- **Dependencies:** LL-023
- **Milestone:** G6
- **Implementation note:** Primary Android unless rules require WebGL

**Acceptance criteria**

- Final build, controls, description, screenshots and submission page are complete and smoke-tested.

**Test checklist**

- [ ] Implemented without adding unrelated scope.
- [ ] Tested in Unity Play Mode.
- [ ] No new Console errors or warnings caused by this task.
- [ ] Acceptance criterion verified manually.
- [ ] Relevant state resets correctly after restart.
