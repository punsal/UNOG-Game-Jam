# LAST LIGHT — PROCEDURAL SOUND PACK

Format:

* WAV, mono, 44.1 kHz, 16-bit PCM
* Ready to import directly into Unity
* No third-party samples were used; all sounds were procedurally synthesized during this session.

Files:
01_last_light_ping.wav        Last light / precious cargo motif
02_move_swipe.wav             Left-right movement
03_hazard_spike.wav           Thin and fast hazard
04_hazard_wall.wav            Large/heavy obstacle
05_shadow_pass.wav            Shadow hazard
06_damage_crack.wav           Damage
07_altar_approach.wav         Approaching an altar
08_pay_cost.wav               Paying the cost
09_accept_risk.wav            Accepting the risk
10_light_loss.wav             Light decreasing
11_death.wav                  Death
12_restart.wav                Restart
13_low_light_warning.wav      Low-light warning
14_final_good.wav             Good ending
15_final_bad.wav              Weak/bad ending
16_stem_light_loop_8s.wav     Music layer: light
17_stem_body_loop_8s.wav      Music layer: body
18_stem_senses_loop_8s.wav    Music layer: senses
19_stem_danger_loop_8s.wav    Music layer: danger
00_quick_preview_all_sfx.wav  Preview for listening to all effects in sequence

Unity Recommendations:

* Short sound effects: Set Load Type to **Decompress On Load**
* 8-second stems: Enable **Loop** and set Load Type to **Compressed In Memory**
* Start all stems simultaneously, then fade their AudioSource volume levels down over 0.15–0.30 seconds according to the player's choices.
* Keep damage and decision sounds approximately 3–6 dB louder than the music.
