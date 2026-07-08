using UnityEngine;

/// <summary>
/// Procedural SFX for Chapter 1 beats until authored clips are imported.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GameAudioController : MonoBehaviour
{
    [SerializeField] private float masterVolume = 0.55f;
    [SerializeField] private float tensionLoopVolume = 0.22f;

    private AudioSource oneShotSource;
    private AudioSource tensionSource;
    private EventBus eventBus;
    private AudioClip keyPickupClip;
    private AudioClip doorRattleClip;
    private AudioClip doorUnlockClip;
    private AudioClip ghostPhaseClip;
    private AudioClip memoryTransitionClip;
    private AudioClip echoContactClip;
    private AudioClip tensionLoopClip;
    private bool tensionPlaying;

    private void Awake()
    {
        oneShotSource = GetComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 0f;

        var loopGo = new GameObject("TensionLoop");
        loopGo.transform.SetParent(transform);
        tensionSource = loopGo.AddComponent<AudioSource>();
        tensionSource.playOnAwake = false;
        tensionSource.loop = true;
        tensionSource.spatialBlend = 0f;
        tensionSource.volume = tensionLoopVolume;

        BuildClips();

        eventBus = Resources.Load<EventBus>("EventBus");
        if (eventBus == null) return;

        eventBus.OnKeyDiscovered += HandleKeyDiscovered;
        eventBus.OnGhostPhaseStarted += HandleGhostPhaseStarted;
        eventBus.OnGhostPhaseEnded += StopTensionLoop;
        eventBus.OnEchoTriggered += HandleEchoSpawned;
        eventBus.OnEchoCaught += HandleEchoContact;
        eventBus.OnTensionChanged += HandleTensionChanged;
        eventBus.OnPuzzleSolved += HandlePuzzleSolved;
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;

        eventBus.OnKeyDiscovered -= HandleKeyDiscovered;
        eventBus.OnGhostPhaseStarted -= HandleGhostPhaseStarted;
        eventBus.OnGhostPhaseEnded -= StopTensionLoop;
        eventBus.OnEchoTriggered -= HandleEchoSpawned;
        eventBus.OnEchoCaught -= HandleEchoContact;
        eventBus.OnTensionChanged -= HandleTensionChanged;
        eventBus.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandleKeyDiscovered(KeyType _) => PlayOneShot(keyPickupClip);

    public void PlayKeyPickup() => PlayOneShot(keyPickupClip);
    public void PlayDoorRattle() => PlayOneShot(doorRattleClip);
    public void PlayDoorUnlock() => PlayOneShot(doorUnlockClip);
    public void PlayMemoryTransition() => PlayOneShot(memoryTransitionClip);
    public void PlayEchoContact() => PlayOneShot(echoContactClip);

    private void HandleGhostPhaseStarted()
    {
        PlayOneShot(ghostPhaseClip);
        GameHaptics.PhaseStart();
        StartTensionLoop();
    }

    private void HandleEchoSpawned()
    {
        StartTensionLoop();
    }

    private void HandleEchoContact()
    {
        PlayEchoContact();
        GameHaptics.EchoContact();
    }

    private void HandleTensionChanged(float tension)
    {
        if (tensionSource == null) return;
        tensionSource.volume = Mathf.Lerp(0.05f, tensionLoopVolume, tension);
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

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || oneShotSource == null) return;
        oneShotSource.PlayOneShot(clip, masterVolume);
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
        keyPickupClip = CreateChime(880f, 0.18f, 0.7f);
        doorRattleClip = CreateNoiseBurst(0.22f, 0.35f);
        doorUnlockClip = CreateChime(392f, 0.32f, 0.55f, secondTone: 523f);
        ghostPhaseClip = CreateSwell(220f, 520f, 0.9f);
        memoryTransitionClip = CreateSwell(160f, 280f, 0.75f);
        echoContactClip = CreateNoiseBurst(0.14f, 0.5f);
        tensionLoopClip = CreateLoopPad(110f, 2.4f);
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