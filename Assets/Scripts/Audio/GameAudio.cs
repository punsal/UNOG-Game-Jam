using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum HazardType { Spike, Wall, Shadow }
public enum SacrificeType { Light, Body, Senses }
public enum EndingQuality { Good, Bad }

/// <summary>Coordinates persistent music stems and gameplay sound effects.</summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Audio Sources (2D, Play On Awake OFF)")]
    [SerializeField] private AudioSource sfxSource;         // -> SFX mixer group
    [SerializeField] private AudioSource lightStemSource;   // -> Music mixer group
    [SerializeField] private AudioSource bodyStemSource;    // -> Music mixer group
    [SerializeField] private AudioSource sensesStemSource;  // -> Music mixer group
    [SerializeField] private AudioSource dangerStemSource;  // -> Music mixer group

    [Header("SFX Clips")]
    [SerializeField] private AudioClip lastLightPingClip;   // 01_last_light_ping
    [SerializeField] private AudioClip moveSwipeClip;       // 02_move_swipe
    [SerializeField] private AudioClip hazardSpikeClip;     // 03_hazard_spike
    [SerializeField] private AudioClip hazardWallClip;      // 04_hazard_wall
    [SerializeField] private AudioClip shadowPassClip;      // 05_shadow_pass
    [SerializeField] private AudioClip damageCrackClip;     // 06_damage_crack
    [SerializeField] private AudioClip altarApproachClip;   // 07_altar_approach
    [SerializeField] private AudioClip payCostClip;         // 08_pay_cost
    [SerializeField] private AudioClip acceptRiskClip;      // 09_accept_risk
    [SerializeField] private AudioClip lightLossClip;       // 10_light_loss
    [SerializeField] private AudioClip deathClip;           // 11_death
    [SerializeField] private AudioClip restartClip;         // 12_restart
    [SerializeField] private AudioClip lowLightWarningClip; // 13_low_light_warning
    [SerializeField] private AudioClip finalGoodClip;       // 14_final_good
    [SerializeField] private AudioClip finalBadClip;        // 15_final_bad

    [Header("Music Stems (8s synchronized loops)")]
    [SerializeField] private AudioClip stemLightClip;       // 16_stem_light_loop_8s
    [SerializeField] private AudioClip stemBodyClip;        // 17_stem_body_loop_8s
    [SerializeField] private AudioClip stemSensesClip;      // 18_stem_senses_loop_8s
    [SerializeField] private AudioClip stemDangerClip;      // 19_stem_danger_loop_8s

    [Header("Tuning")]
    [SerializeField] private float dangerBaseVolume = 0.45f;
    [SerializeField] private float swipeCooldown = 0.12f;
    [SerializeField] private float sacrificeFadeTime = 0.25f;
    [SerializeField] private float riskFadeTime = 0.25f;
    [SerializeField] private float deathFadeTime = 0.20f;
    [SerializeField] private float endingFadeTime = 0.5f;
    [SerializeField] private float lowLightThreshold = 0.25f;
    // Cumulative light drop (normalized) that re-triggers the loss sting; prevents
    // per-frame spam when light drains continuously.
    [SerializeField] private float lightLossStep = 0.08f;

    private const int StemCount = 4;   // Light, Body, Senses (= SacrificeType order), then Danger.
    private const int DangerIndex = 3;
    private const double StemStartDelay = 0.1; // seconds of DSP lead time for PlayScheduled.

    private readonly AudioSource[] stemSources = new AudioSource[StemCount];
    private readonly AudioClip[] stemClips = new AudioClip[StemCount];
    private readonly Coroutine[] stemFades = new Coroutine[StemCount];
    private readonly bool[] sacrificed = new bool[3];

    private float lastSwipeTime = float.NegativeInfinity;
    private int lastStartFrame = -1;
    private float previousLight = 1f;
    private float lightAtLastLoss = 1f;
    private bool lowLightWarned;
    private bool deathTriggered;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Duplicate from a reloaded scene.
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        stemSources[0] = lightStemSource;
        stemSources[1] = bodyStemSource;
        stemSources[2] = sensesStemSource;
        stemSources[3] = dangerStemSource;
        stemClips[0] = stemLightClip;
        stemClips[1] = stemBodyClip;
        stemClips[2] = stemSensesClip;
        stemClips[3] = stemDangerClip;

        if (sfxSource == null)
            Debug.LogError("GameAudio: SFX AudioSource is not assigned.", this);
        else
            ConfigureSource(sfxSource, loop: false);

        for (int i = 0; i < StemCount; i++)
        {
            if (stemSources[i] == null)
                Debug.LogError($"GameAudio: stem AudioSource {i} (Light/Body/Senses/Danger) is not assigned.", this);
            else
                ConfigureSource(stemSources[i], loop: true);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Enforce 2D, no autoplay, regardless of Inspector state.
    private static void ConfigureSource(AudioSource source, bool loop)
    {
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    // ---------------- Run lifecycle ----------------

    // Resets the whole mix and starts the four stems on one shared DSP timestamp.
    public void StartRun()
    {
        // Collapse overlapping start requests (e.g. RestartRun + the run-reset
        // path in the same frame) into a single schedule.
        if (Time.frameCount == lastStartFrame) return;
        lastStartFrame = Time.frameCount;
        Debug.Log("Audio mix started for a new run.", this);

        deathTriggered = false;
        lowLightWarned = false;
        previousLight = 1f;
        lightAtLastLoss = 1f;
        for (int i = 0; i < sacrificed.Length; i++) sacrificed[i] = false;

        CancelAllFades();

        double startTime = AudioSettings.dspTime + StemStartDelay;
        for (int i = 0; i < StemCount; i++)
        {
            AudioSource source = stemSources[i];
            if (source == null) continue;

            source.Stop();
            if (stemClips[i] == null)
            {
                Debug.LogWarning($"GameAudio: stem clip {i} (Light/Body/Senses/Danger) missing - stem skipped.", this);
                continue;
            }
            source.clip = stemClips[i];
            source.loop = true;
            source.volume = i == DangerIndex ? dangerBaseVolume : 1f;
            source.PlayScheduled(startTime);
        }
    }

    public void RestartRun()
    {
        PlaySfx(restartClip);
        StartRun(); // Restores every stem, including sacrificed ones.
    }

    public void PlayDeath()
    {
        if (deathTriggered) return;
        deathTriggered = true;
        Debug.Log("Playing death audio.", this);

        FadeAllStems(0f, deathFadeTime, stopWhenSilent: true);
        PlaySfx(deathClip);
    }

    public void PlayEnding(EndingQuality endingQuality)
    {
        Debug.Log($"Playing {endingQuality} ending audio.", this);
        FadeAllStems(0f, endingFadeTime, stopWhenSilent: true);
        PlaySfx(endingQuality == EndingQuality.Good ? finalGoodClip : finalBadClip);
    }

    public void StopAllAudio()
    {
        CancelAllFades();
        for (int i = 0; i < StemCount; i++)
            if (stemSources[i] != null) stemSources[i].Stop();
        if (sfxSource != null) sfxSource.Stop();
    }

    // ---------------- Choices ----------------

    public void ChooseSacrifice(SacrificeType sacrificeType)
    {
        int index = (int)sacrificeType;
        if (sacrificed[index]) return; // Permanent for this run; never replay/refade.
        sacrificed[index] = true;
        Debug.Log($"Muted {sacrificeType} music stem.", this);

        PlaySfx(payCostClip);
        if (sacrificeType == SacrificeType.Light) PlaySfx(lightLossClip);

        // Keep the source looping at 0 volume so the mix stays sample-aligned.
        FadeStem(index, 0f, sacrificeFadeTime);
    }

    public void ChooseRisk()
    {
        Debug.Log("Raised danger music stem.", this);
        PlaySfx(acceptRiskClip);
        FadeStem(DangerIndex, 1f, riskFadeTime);
    }

    // ---------------- Light level ----------------

    // Expects a 0-1 value; safe to call every frame.
    public void SetLightLevel(float normalizedLight)
    {
        float value = Mathf.Clamp01(normalizedLight);

        // Loss sting on meaningful cumulative decrease, not per-frame drain.
        if (lightAtLastLoss - value >= lightLossStep)
        {
            PlaySfx(lightLossClip);
            lightAtLastLoss = value;
        }
        else if (value > lightAtLastLoss)
        {
            lightAtLastLoss = value; // Light regained; track the new high point.
        }

        // One warning per run when crossing the threshold downward.
        if (!lowLightWarned && previousLight > lowLightThreshold && value <= lowLightThreshold)
        {
            lowLightWarned = true;
            PlaySfx(lowLightWarningClip);
        }

        previousLight = value;
    }

    // ---------------- One-shot SFX ----------------

    public void PlayLastLightPing() => PlaySfx(lastLightPingClip);

    // Call from movement code on each swipe; internally rate-limited.
    public void PlayMoveSwipe()
    {
        if (Time.unscaledTime - lastSwipeTime < swipeCooldown) return;
        lastSwipeTime = Time.unscaledTime;
        PlaySfx(moveSwipeClip);
    }

    // Telegraph: call when the threat appears / enters warning range, not on hit.
    public void PlayHazardTelegraph(HazardType hazardType)
    {
        switch (hazardType)
        {
            case HazardType.Spike: PlaySfx(hazardSpikeClip); break;
            case HazardType.Wall: PlaySfx(hazardWallClip); break;
            case HazardType.Shadow: PlaySfx(shadowPassClip); break;
        }
    }

    public void PlayDamage() => PlaySfx(damageCrackClip);

    // Call once per altar interaction (e.g. trigger enter), never per frame.
    public void PlayAltarApproach() => PlaySfx(altarApproachClip);

    // ---------------- Internals ----------------

    private void PlaySfx(AudioClip clip, [CallerMemberName] string caller = null)
    {
        if (sfxSource == null) return; // Already reported in Awake.
        if (clip == null)
        {
            Debug.LogWarning($"GameAudio: missing AudioClip for {caller} - sound skipped.", this);
            return;
        }
        sfxSource.PlayOneShot(clip);
    }

    private void FadeAllStems(float target, float duration, bool stopWhenSilent)
    {
        for (int i = 0; i < StemCount; i++)
            FadeStem(i, target, duration, stopWhenSilent);
    }

    // Replaces any fade already running on the same stem.
    private void FadeStem(int index, float target, float duration, bool stopWhenSilent = false)
    {
        AudioSource source = stemSources[index];
        if (source == null) return;

        if (stemFades[index] != null) StopCoroutine(stemFades[index]);
        stemFades[index] = StartCoroutine(FadeRoutine(index, source, target, duration, stopWhenSilent));
    }

    private IEnumerator FadeRoutine(int index, AudioSource source, float target, float duration, bool stopWhenSilent)
    {
        float start = source.volume;
        if (duration > 0f)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
        }
        source.volume = target;
        if (stopWhenSilent && target <= 0f) source.Stop();
        stemFades[index] = null;
    }

    private void CancelAllFades()
    {
        for (int i = 0; i < StemCount; i++)
        {
            if (stemFades[i] != null)
            {
                StopCoroutine(stemFades[i]);
                stemFades[i] = null;
            }
        }
    }
}
