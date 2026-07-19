# Fairness Matrix — Populated Stages (LL-019)

Analytical verification of the populated stage layouts at worst-case
permanent modifiers, computed from actual prefab colliders (2026-07-19).

## Model

- Player: speed floor **x0.7** (4.2 u/s lateral), body scale **x1.25**
  (collider half-width 0.625), clamp ±3.5.
- Spike row: box collider 2.6 wide → blocks spike center ±1.925 in
  player-center space.
- Blade: circle r 0.7 sweeping ±amplitude → conservatively treated as
  blocking its entire sweep; **amplitude is capped at 2.0** so a player
  pinned at either wall clamp is safe from a fully extended blade by
  0.175 u (the guaranteed refuge rule).
- Twin spike rows sit at **±2.75**, leaving a 1.65 u center gap at max
  body (2.3 u at normal size).
- Transition check: worst-case lateral travel between consecutive rows'
  safe lanes must fit in the scroll time between rows minus a 0.4 s
  reaction allowance.

## Results (all PASS)

| Stage | Rows | Narrowest safe lane | Tightest transition margin |
| --- | --- | --- | --- |
| 1 | 5 | 1.58 u | +8.3 u |
| 2 | 5 | 0.18 u (wall refuge) | +8.5 u |
| 3 | 7 | 0.18 u (wall refuge) | +6.3 u |
| 4 | 7 | 0.18 u (wall refuge) | +7.4 u |
| 5 | 7 | 0.18 u (wall refuge) | +9.2 u |
| 6 | 9 | 0.18 u (wall refuge) | +7.1 u |

Notes:

- "Wall refuge" rows are blade rows: the static model blocks the whole
  sweep, so the remaining safe width is the wall margin. Holding the
  drag against a wall is trivial input; the refuge is guaranteed, not
  probabilistic.
- Twin-spike rows at ±2 (pre-fix) left a 0.15 u gap at max body —
  effectively an unavoidable hit. Widened to ±2.75 in stages 4, 5, 6.
- Blade amplitudes above 2.0 (pre-fix 2.5–3.0 in stages 2–6) broke the
  wall-refuge guarantee. All capped to 2.0; difficulty progression is
  carried by blade speed (2.0→3.0), phase offsets, and row density.
- Benefits only relax these numbers (blade slow, spike narrow, scroll
  slow, hazard removal); the matrix assumes none are taken.

Human calibration (tense-but-winnable feel) remains covered by the
LL-022 playtest, not this matrix.
