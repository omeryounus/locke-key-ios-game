using UnityEngine;

/// <summary>
/// Immersive procedural audio bed for Chapter 1: footsteps, wood creaks,
/// distant thunder, whisper ambience, unlock chimes, and key sparkles.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GameAudioController : MonoBehaviour
{
    [SerializeField] private float masterVolume = 0.58f;
    [SerializeField] private float tensionLoopVolume = 0.2f;

    private AudioSource oneShotSource;
    private AudioSource tensionSource;
    private AudioSource ambientSource;
    private AudioSource whisperSource;
    private AudioSource thunderSource;
    private EventBus eventBus;

    private AudioClip keyPickupClip;
    private AudioClip keySparkleClip;
    private AudioClip doorRattleClip;
    private AudioClip doorUnlockClip;
    private AudioClip ghostPhaseClip;
    private AudioClip memoryTransitionClip;
    private AudioClip echoContactClip;
    private AudioClip tensionLoopClip;
    private AudioClip echoWhisperClip;
    private AudioClip footstepClip;
    private AudioClip woodCreakClip;
    private AudioClip ambientClip;
    private AudioClip whisperLoopClip;
    private AudioClip thunderClip;

    private bool tensionPlaying;
    private bool muffled;
    private float baseMasterVolume;
    private float footstepCooldown;
    private float creakTimer;
    private float thunderTimer;

    private void Awake()
    {
        oneShotSource = GetComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 0f;
        baseMasterVolume = masterVolume;

        tensionSource = MakeLoopSource("TensionLoop", tensionLoopVolume);
        ambientSource = MakeLoopSource("AmbientMansion", 0.14f);
        whisperSource = MakeLoopSource("WhisperBed", 0.09f);
        thunderSource = MakeLoopSource("ThunderBed", 0.0f);
        thunderSource.loop = false;

        BuildClips();

        if (ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.Play();
        }

        if (whisperLoopClip != null)
        {
            whisperSource.clip = whisperLoopClip;
            whisperSource.Play();
        }

        creakTimer = Random.Range(4f, 9f);
        thunderTimer = Random.Range(12f, 22f);

        eventBus = Resources.Load<EventBus>("EventBus");
        if (eventBus == null) return;

        eventBus.OnKeyDiscovered += HandleKeyDiscovered;
        eventBus.OnGhostPhaseStarted += HandleGhostPhaseStarted;
        eventBus.OnGhostPhaseEnded += HandleGhostPhaseEndedAudio;
        eventBus.OnEchoTriggered += HandleEchoSpawned;
        eventBus.OnEchoCaught += HandleEchoContact;
        eventBus.OnTensionChanged += HandleTensionChanged;
        eventBus.OnPuzzleSolved += HandlePuzzleSolved;
    }

    private AudioSource MakeLoopSource(string name, float volume)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = volume;
        return src;
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnKeyDiscovered -= HandleKeyDiscovered;
        eventBus.OnGhostPhaseStarted -= HandleGhostPhaseStarted;
        eventBus.OnGhostPhaseEnded -= HandleGhostPhaseEndedAudio;
        eventBus.OnEchoTriggered -= HandleEchoSpawned;
        eventBus.OnEchoCaught -= HandleEchoContact;
        eventBus.OnTensionChanged -= HandleTensionChanged;
        eventBus.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void Update()
    {
        if (footstepCooldown > 0f)
            footstepCooldown -= Time.deltaTime;

        // Occasional mansion creaks
        creakTimer -= Time.deltaTime;
        if (creakTimer <= 0f)
        {
            creakTimer = Random.Range(6f, 14f);
            PlayWoodCreak();
        }

        // Distant thunder rolls
        thunderTimer -= Time.deltaTime;
        if (thunderTimer <= 0f)
        {
            thunderTimer = Random.Range(16f, 32f);
            PlayDistantThunder();
        }

        // Whisper bed gently modulates
        if (whisperSource != null && whisperSource.isPlaying)
            whisperSource.volume = 0.06f + Mathf.PerlinNoise(Time.time * 0.15f, 0.4f) * 0.07f;
    }

    private void HandleKeyDiscovered(KeyType _) => PlayKeyPickup();

    public void PlayKeyPickup()
    {
        PlayOneShot(keyPickupClip, 0.9f);
        PlayOneShot(keySparkleClip, 0.65f);
    }

    public void PlayDoorRattle() => PlayOneShot(doorRattleClip, 0.75f);
    public void PlayDoorUnlock() => PlayOneShot(doorUnlockClip, 1f);
    public void PlayMemoryTransition() => PlayOneShot(memoryTransitionClip);
    public void PlayEchoContact() => PlayOneShot(echoContactClip);
    public void PlayEchoWhisper() => PlayOneShot(echoWhisperClip, 0.42f);
    public void PlayWoodCreak() => PlayOneShot(woodCreakClip, 0.28f);

    public void PlayDistantThunder()
    {
        if (thunderClip == null || thunderSource == null) return;
        thunderSource.Stop();
        thunderSource.clip = thunderClip;
        thunderSource.volume = masterVolume * 0.22f;
        thunderSource.pitch = Random.Range(0.85f, 1.05f);
        thunderSource.Play();
    }

    public void PlayFootstep()
    {
        if (footstepCooldown > 0f) return;
        footstepCooldown = 0.32f;
        // Slight pitch variance for natural steps
        if (oneShotSource != null)
            oneShotSource.pitch = muffled ? 0.72f : Random.Range(0.92f, 1.08f);
        PlayOneShot(footstepClip, 0.4f);
        if (oneShotSource != null && !muffled)
            oneShotSource.pitch = 1f;

        // Soft chance of board creak while walking
        if (Random.value < 0.12f)
            PlayWoodCreak();
    }

    public void SetMuffled(bool enabled)
    {
        muffled = enabled;
        masterVolume = enabled ? baseMasterVolume * 0.38f : baseMasterVolume;
        if (oneShotSource != null)
            oneShotSource.pitch = enabled ? 0.72f : 1f;
        if (tensionSource != null)
            tensionSource.pitch = enabled ? 0.68f : 1f;
        if (ambientSource != null)
            ambientSource.volume = enabled ? 0.05f : 0.14f;
        if (whisperSource != null)
            whisperSource.volume = enabled ? 0.04f : 0.09f;
    }

    private void HandleGhostPhaseStarted()
    {
        PlayOneShot(ghostPhaseClip);
        StartTensionLoop();
    }

    private void HandleGhostPhaseEndedAudio() => SetMuffled(false);

    private void HandleEchoSpawned()
    {
        StartTensionLoop();
        PlayEchoWhisper();
    }

    private void HandleEchoContact()
    {
        PlayEchoContact();
        GameHaptics.EchoContact();
    }

    private void HandleTensionChanged(float tension)
    {
        if (tensionSource == null) return;
        tensionSource.volume = Mathf.Lerp(0.04f, tensionLoopVolume, tension);
        if (tension > 0.45f && !tensionPlaying)
            StartTensionLoop();
        else if (tension < 0.2f)
            StopTensionLoop();
    }

    private void HandlePuzzleSolved(PuzzleBase puzzle)
    {
        if (puzzle == null) return;
        switch (puzzle.puzzleID)
        {
            case "chapter1_stuck_door":
                PlayDoorUnlock();
                GameHaptics.Unlock();
                break;
            case "chapter1_memory_fragment":
                PlayMemoryTransition();
                break;
        }
    }

    private void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || oneShotSource == null) return;
        oneShotSource.PlayOneShot(clip, masterVolume * volumeScale);
    }

    private void StartTensionLoop()
    {
        if (tensionLoopClip == null || tensionSource == null || tensionPlaying) return;
        tensionSource.clip = tensionLoopClip;
        tensionSource.Play();
        tensionPlaying = true;
    }

    private void StopTensionLoop()
    {
        if (tensionSource == null || !tensionPlaying) return;
        tensionSource.Stop();
        tensionPlaying = false;
    }

    private void BuildClips()
    {
        keyPickupClip = CreateChime(880f, 0.2f, 0.75f, secondTone: 1320f);
        keySparkleClip = CreateSparkle(0.35f);
        doorRattleClip = CreateNoiseBurst(0.22f, 0.35f);
        doorUnlockClip = CreateUnlockChime();
        ghostPhaseClip = CreateSwell(220f, 520f, 0.9f);
        memoryTransitionClip = CreateSwell(160f, 280f, 0.75f);
        echoContactClip = CreateNoiseBurst(0.14f, 0.5f);
        tensionLoopClip = CreateLoopPad(110f, 2.4f);
        echoWhisperClip = CreateWhisper(0.85f);
        footstepClip = CreateFootstep();
        woodCreakClip = CreateWoodCreak();
        ambientClip = CreateMansionBed(4.5f);
        whisperLoopClip = CreateWhisperLoop(3.2f);
        thunderClip = CreateThunder(2.8f);
    }

    private static AudioClip CreateFootstep()
    {
        const int sampleRate = 44100;
        var samples = (int)(sampleRate * 0.09f);
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)samples;
            var env = Mathf.Exp(-10f * t) * (1f - t);
            var thud = Mathf.Sin(2f * Mathf.PI * 90f * (i / (float)sampleRate)) * 0.45f;
            var noise = (Random.value * 2f - 1f) * 0.35f * env;
            data[i] = (thud * env + noise) * 0.7f;
        }
        var clip = AudioClip.Create("footstep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateWoodCreak()
    {
        const int sampleRate = 44100;
        var duration = 0.55f;
        var samples = (int)(sampleRate * duration);
        var data = new float[samples];
        float phase = 0f;
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            var env = Mathf.Sin(Mathf.PI * t / duration) * Mathf.Exp(-1.8f * t);
            var freq = Mathf.Lerp(180f, 320f, t / duration) + Mathf.Sin(t * 40f) * 25f;
            phase += 2f * Mathf.PI * freq / sampleRate;
            data[i] = Mathf.Sin(phase) * env * 0.35f
                      + (Random.value * 2f - 1f) * 0.04f * env;
        }
        var clip = AudioClip.Create("creak", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateThunder(float duration)
    {
        const int sampleRate = 44100;
        var samples = (int)(sampleRate * duration);
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            var env = Mathf.Exp(-1.1f * t) * (0.4f + 0.6f * Mathf.Exp(-8f * Mathf.Abs(t - 0.15f)));
            // Low rumble + noise
            var rumble = Mathf.Sin(2f * Mathf.PI * 38f * t) * 0.5f
                         + Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.25f;
            var noise = (Random.value * 2f - 1f) * 0.35f;
            // Simple lowpass-ish by blending
            data[i] = (rumble + noise * 0.55f) * env * 0.55f;
        }
        var clip = AudioClip.Create("thunder", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateSparkle(float duration)
    {
        const int sampleRate = 44100;
        var samples = (int)(sampleRate * duration);
        var data = new float[samples];
        float[] freqs = { 1200f, 1600f, 2100f, 2800f };
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            float sum = 0f;
            for (var f = 0; f < freqs.Length; f++)
            {
                var delay = f * 0.04f;
                if (t < delay) continue;
                var lt = t - delay;
                var env = Mathf.Exp(-12f * lt);
                sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * lt) * env;
            }
            data[i] = sum * 0.18f;
        }
        var clip = AudioClip.Create("sparkle", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateUnlockChime()
    {
        const int sampleRate = 44100;
        var duration = 0.55f;
        var samples = (int)(sampleRate * duration);
        var data = new float[samples];
        float[] tones = { 392f, 523f, 659f };
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            float sum = 0f;
            for (var n = 0; n < tones.Length; n++)
            {
                var delay = n * 0.08f;
                if (t < delay) continue;
                var lt = t - delay;
                var env = Mathf.Exp(-5f * lt / duration);
                sum += Mathf.Sin(2f * Mathf.PI * tones[n] * lt) * env;
            }
            data[i] = sum * 0.28f;
        }
        var clip = AudioClip.Create("unlock", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateMansionBed(float duration)
    {
        const int sampleRate = 44100;
        var samples = (int)(sampleRate * duration);
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            // Soft drone + air
            data[i] = (Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.22f
                       + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.12f
                       + (Random.value * 2f - 1f) * 0.03f) * 0.45f;
        }
        var clip = AudioClip.Create("mansion", samples, 1, sampleRate, true);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateWhisperLoop(float duration)
    {
        const int sampleRate = 44100;
        var samples = (int)(sampleRate * duration);
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            var noise = (Random.value * 2f - 1f) * 0.2f;
            var tone = Mathf.Sin(2f * Mathf.PI * 170f * t) * 0.05f
                       + Mathf.Sin(2f * Mathf.PI * 210f * t + 1.3f) * 0.04f;
            var env = 0.6f + 0.4f * Mathf.Sin(t * 1.7f);
            data[i] = (noise + tone) * env * 0.35f;
        }
        var clip = AudioClip.Create("whisperLoop", samples, 1, sampleRate, true);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateWhisper(float duration)
    {
        const int sampleRate = 44100;
        var samples = Mathf.Max(1, (int)(sampleRate * duration));
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            var env = Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t * 4f)) * (1f - t / duration);
            var noise = (Random.value * 2f - 1f) * 0.22f;
            var tone = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.08f;
            data[i] = (noise + tone) * env;
        }
        var clip = AudioClip.Create("whisper", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateChime(float freq, float duration, float volume, float secondTone = 0f)
    {
        const int sampleRate = 44100;
        var samples = Mathf.Max(1, (int)(sampleRate * duration));
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            var env = Mathf.Exp(-6f * t / duration);
            var wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            if (secondTone > 0f)
                wave = (wave + Mathf.Sin(2f * Mathf.PI * secondTone * t) * 0.55f) * 0.5f;
            data[i] = wave * env * volume;
        }
        var clip = AudioClip.Create("chime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateSwell(float startFreq, float endFreq, float duration)
    {
        const int sampleRate = 44100;
        var samples = Mathf.Max(1, (int)(sampleRate * duration));
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)samples;
            var freq = Mathf.Lerp(startFreq, endFreq, t);
            var env = Mathf.SmoothStep(0f, 1f, t) * (1f - t * 0.35f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * env * 0.45f;
        }
        var clip = AudioClip.Create("swell", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateNoiseBurst(float duration, float volume)
    {
        const int sampleRate = 44100;
        var samples = Mathf.Max(1, (int)(sampleRate * duration));
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var env = 1f - i / (float)samples;
            data[i] = (Random.value * 2f - 1f) * env * volume;
        }
        var clip = AudioClip.Create("noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateLoopPad(float freq, float duration)
    {
        const int sampleRate = 44100;
        var samples = Mathf.Max(1, (int)(sampleRate * duration));
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (float)sampleRate;
            data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.35f
                       + Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t) * 0.2f) * 0.35f;
        }
        var clip = AudioClip.Create("tension", samples, 1, sampleRate, true);
        clip.SetData(data, 0);
        return clip;
    }
}
