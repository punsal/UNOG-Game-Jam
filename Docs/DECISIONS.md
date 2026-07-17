# Last Light - DECISIONS

These decisions protect the MVP. Claude Code should treat locked decisions as constraints, not suggestions.

## Product decisions

### Scope rule

- **Status:** Accepted
- **Decision:** No feature is added unless another feature is removed.

### Primary build

- **Status:** Pending
- **Decision:** Android by default; switch to WebGL only if jam rules require it.

### Language

- **Status:** Accepted
- **Decision:** Use icon-heavy short English copy.

### Ending count

- **Status:** Pending
- **Decision:** Three target, two minimum; decide after G3.

### Core loop

- **Status:** Locked
- **Decision:** Auto-run, horizontal one-finger movement, damage avoidance, end-stage choice, permanent sacrifice.

### Resources

- **Status:** Locked
- **Decision:** Health and relic light are separate resources.

### Run structure

- **Status:** Locked
- **Decision:** Six short fixed stages, total run length 3-5 minutes.

### Feature freeze

- **Status:** Locked
- **Decision:** Begins at Hour 38. Only blocker, build and readability fixes after that point.

## Active risks

### R-01 - Scope growth

- **Likelihood:** High
- **Impact:** High
- **Response:** Move new ideas to Parking Lot unless they replace existing scope.

### R-02 - Mobile input feel

- **Likelihood:** Medium
- **Impact:** High
- **Response:** Run early real-device test and tune smoothing.

### R-03 - Impossible cost combinations

- **Likelihood:** Medium
- **Impact:** High
- **Response:** Cap modifiers and test minimum speed with maximum body size.

## Explicitly out of scope

- Combat
- Procedural generation
- Inventory systems
- Dialogue trees
- Multiple character classes
- Online features
- Complex meta-progression
- Any feature that delays the critical path
