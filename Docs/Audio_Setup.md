# Last Light — Audio Setup (GameAudio)

Script: `Assets/Scripts/GameAudio.cs`. Everything below is one-time Editor setup.

## 1. AudioMixer

1. `Assets` → right-click → **Create → Audio Mixer**, name it `GameAudioMixer` (suggested location: `Assets/Audio/GameAudioMixer.mixer`).
2. In the Audio Mixer window, under **Groups**, select **Master** and click **+** twice to add two child groups: **SFX** and **Music**.

## 2. GameObject and AudioSources

1. In the boot scene (the first scene that loads), create an empty GameObject named **GameAudio** at the scene root (it must not be a child — `DontDestroyOnLoad` requires a root object).
2. Add the **GameAudio** script to it.
3. Add **five AudioSource** components to the same GameObject. For **every** source set:
   - **Play On Awake: OFF**
   - **Spatial Blend: 0 (2D)**
   (The script re-enforces both at runtime, but set them anyway so nothing plays before Awake.)
4. Assign **Output**:
   - Source 1 → `GameAudioMixer/SFX`
   - Sources 2–5 → `GameAudioMixer/Music`
5. Enable **Loop** on sources 2–5 (the stems). Leave Loop off on source 1.

## 3. Inspector wiring on the GameAudio component

**Audio Sources** (drag the components in this order):

| Field | Source |
|---|---|
| Sfx Source | AudioSource 1 (SFX group) |
| Light Stem Source | AudioSource 2 (Music group) |
| Body Stem Source | AudioSource 3 (Music group) |
| Senses Stem Source | AudioSource 4 (Music group) |
| Danger Stem Source | AudioSource 5 (Music group) |

**SFX Clips** (from `Assets/Audio/`):

| Field | Clip |
|---|---|
| Last Light Ping Clip | 01_last_light_ping |
| Move Swipe Clip | 02_move_swipe |
| Hazard Spike Clip | 03_hazard_spike |
| Hazard Wall Clip | 04_hazard_wall |
| Shadow Pass Clip | 05_shadow_pass |
| Damage Crack Clip | 06_damage_crack |
| Altar Approach Clip | 07_altar_approach |
| Pay Cost Clip | 08_pay_cost |
| Accept Risk Clip | 09_accept_risk |
| Light Loss Clip | 10_light_loss |
| Death Clip | 11_death |
| Restart Clip | 12_restart |
| Low Light Warning Clip | 13_low_light_warning |
| Final Good Clip | 14_final_good |
| Final Bad Clip | 15_final_bad |

**Music Stems**:

| Field | Clip |
|---|---|
| Stem Light Clip | 16_stem_light_loop_8s |
| Stem Body Clip | 17_stem_body_loop_8s |
| Stem Senses Clip | 18_stem_senses_loop_8s |
| Stem Danger Clip | 19_stem_danger_loop_8s |

**Tuning** defaults already match the spec (danger base 0.45, swipe cooldown 0.12 s, sacrifice/risk fades 0.25 s, death fade 0.20 s, ending fade 0.5 s, low-light threshold 0.25).

## 4. Import settings

Select clips in the Project window and set in the importer (Default tab is fine; it applies to iOS/Android too):

**Short SFX (01–15):**
- Force To Mono: **ON** (they're 2D UI-style sounds; halves memory)
- Load Type: **Decompress On Load**
- Compression Format: **ADPCM**
- Preload Audio Data: ON

**Stems (16–19):**
- Force To Mono: OFF
- Load Type: **Decompress On Load**
- Compression Format: **PCM** (or ADPCM if memory is tight)
- Preload Audio Data: ON

Do **not** use Vorbis/MP3 or Streaming for the stems: lossy encoders pad the clip ends, which breaks the seamless 8-second loop point and drifts the sync; streaming adds scheduling latency that defeats `PlayScheduled` alignment.

Project Settings → Audio: keep **DSP Buffer Size = Good latency** (default) for mobile.

## 5. Gameplay integration (wired)

All calls go through `GameAudio.Instance?.Method()` (null-safe for test scenes without the audio object) — no gameplay script touches an AudioSource directly.

| Method | Wired in | When |
|---|---|---|
| `StartRun()` | `RunController.Restart()` | Every run start/reset (first start, death, retry) — music starts synced |
| `PlayLastLightPing()` | `StartScreen.HandleTapStart()` | Identity ping when the player starts the game |
| `PlayMoveSwipe()` | `PlayerMotor.SetInput()` | Per swipe *gesture*: new drag after a 0.25 s pause or a direction change (plus the 0.12 s hard cooldown inside GameAudio) |
| `PlayHazardTelegraph(hazardType)` | `ObstacleBase.Update()` | Once when the hazard scrolls within `telegraphLeadDistance` (default 2) world units above the camera's visible top edge (camera-relative, so it holds across aspect ratios; `telegraphY` is only a no-camera fallback); re-armed on `OnEnable` when stages reset. Type comes from the serialized `Hazard Type` field (default Spike), overridable in code via the `protected virtual TelegraphHazardType` property |
| `PlayDamage()` | `PlayerHealth.ApplyDamage()` | On each successful hit (invulnerability window already filtered) |
| `PlayAltarApproach()` | `ChoiceGate.HandleEntered()` | Once per altar interaction (gated by `hasChosen`) |
| `ChooseSacrifice(...)` | `CostApplier.Apply()` | `CostType.Light` → Light, `CostType.Body` → Body, `CostType.Sight` → Senses (idempotent per run) |
| `ChooseRisk()` | `CostApplier.Apply()` | `CostType.Blood` — keeps all abilities but risks the body, so it raises the Danger stem |
| `SetLightLevel(x)` | `RelicLight.ChangeLight()` | `currentLight / 100`; GameAudio handles the loss sting + one-shot 25 % warning |
| `PlayDeath()` | `RunController.HandleDeath()` | Input is disabled and the restart waits `deathRestartDelay` (0.45 s, serialized) so the 0.2 s fade + sting play out; duplicate deaths are ignored while pending, and an external `Restart()` supersedes the pending one |
| `RestartRun()` | `OutcomeScreen.HandleRetry()` | Retry button: 12_restart sting + full mix restore |
| `PlayEnding(...)` | `EndingResolver.Resolve()` | `EndingType.Light` → Good, `EndingType.Dark` → Bad |
| `StopAllAudio()` | (unused) | Available for a future quit-to-menu flow |

`StartRun()` fires exactly once per run start: first start and death-restart reach it once by construction, and the retry path (`RestartRun()` + `RunController.Restart()` in the same frame) is collapsed to a single stem schedule by a same-frame guard inside `GameAudio.StartRun()`.
