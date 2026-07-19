# Fairness Matrix (LL-019)

Analytical verification that every stage layout is passable at worst-case
permanent modifiers, computed from actual prefab colliders. Re-run this
whenever stage layouts, hazard colliders, stage geometry (spawnY /
EndMarker), or stage durations change. Last run: 2026-07-19, post
playtest-easing and rebalance (travel 41 u, durations 22/22/26/26/28/30 s).

## Model

- Player: speed floor **x0.7** (4.2 u/s lateral), body scale **x1.25**
  (collider half-width 0.625), clamp ±3.5, 0.4 s reaction allowance.
- Spike row: box collider 2.6 wide → blocks spike center ±1.925 in
  player-center space. Twin rows sit at **±2.75**, leaving a 1.65 u
  center gap at max body.
- Blade: circle r 0.7 sweeping ±amplitude, conservatively treated as
  blocking its entire sweep. **Amplitude is capped at 2.0 and speed at
  2.4 everywhere**: a player pinned at either wall clamp is safe from a
  fully extended blade by 0.175 u (the guaranteed wall-refuge rule).
- Transition check: worst-case lateral travel between consecutive rows'
  safe lanes must fit in the scroll time between rows minus reaction.

## Invariants (do not break)

1. Blade amplitude ≤ 2.0 — preserves the wall refuge.
2. Twin spikes at ±2.75, never ±2 — a 2.6-wide spike at ±2 closes the
   center gap to 0.15 u at max body (an unavoidable hit).
3. Every row must leave at least one safe lane; every transition margin
   must stay positive at worst-case modifiers.
4. Light offers appear in exactly three gates, or the Dark ending
   (threshold 50) becomes unreachable.

## Results (all PASS)

| Stage | Obstacles | Scroll speed | Narrowest safe lane | Tightest margin |
| --- | --- | --- | --- | --- |
| 1 | 5 | 1.86 | 1.58 u | +6.2 u |
| 2 | 5 | 1.86 | 0.18 u (wall refuge) | +6.3 u |
| 3 | 7 | 1.58 | 0.18 u (wall refuge) | +4.9 u |
| 4 | 8 | 1.58 | 0.18 u (wall refuge) | +5.5 u |
| 5 | 8 | 1.46 | 0.18 u (wall refuge) | +6.4 u |
| 6 | 7 | 1.37 | 0.18 u (wall refuge) | +4.3 u |

Notes:

- "Wall refuge" rows are blade rows: the static model blocks the whole
  sweep, so the remaining safe width is the wall margin. Holding the
  drag against a wall is trivial input; the refuge is guaranteed.
- Benefits only relax these numbers (hazard removal, blade slow x0.6,
  spike narrow x0.7, scroll slow x0.9); the matrix assumes none taken.
- Survivability beyond geometry: each accepted sacrifice heals 1 HP,
  and invulnerability after a hit is 1.0 s.
- Human calibration (tense-but-winnable) is covered by playtest, not
  this matrix.
