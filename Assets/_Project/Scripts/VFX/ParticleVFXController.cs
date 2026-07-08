using UnityEngine;

/// <summary>
/// Runtime particle bursts for ghost phase, memory, and Echo fog VFX sheets.
/// </summary>
public class ParticleVFXController : MonoBehaviour
{
    [SerializeField] private Sprite[] ghostPhaseSprites;
    [SerializeField] private Sprite[] memorySprites;
    [SerializeField] private Sprite[] echoFogSprites;

    private EventBus eventBus;

    private void Awake()
    {
        eventBus = Resources.Load<EventBus>("EventBus");
        LoadSpritesIfNeeded();

        if (eventBus != null)
        {
            eventBus.OnGhostPhaseStarted += HandleGhostPhaseStarted;
            eventBus.OnEchoTriggered += HandleEchoTriggered;
        }
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= HandleGhostPhaseStarted;
        eventBus.OnEchoTriggered -= HandleEchoTriggered;
    }

    public void PlayMemoryBurst(Vector3 position)
    {
        SpawnBurst(position, memorySprites, new Color(1f, 0.82f, 0.45f, 0.8f), 1.4f, 12);
    }

    public void PlayGhostRevealBurst(Vector3 position)
    {
        SpawnBurst(position, ghostPhaseSprites, new Color(0.55f, 0.95f, 0.85f, 0.75f), 1.6f, 14);
    }

    private void HandleGhostPhaseStarted()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            SpawnBurst(player.transform.position, ghostPhaseSprites, new Color(0.55f, 0.95f, 0.85f, 0.75f), 1.8f, 18);
    }

    private void HandleEchoTriggered()
    {
        var spawn = GameObject.Find("EchoSpawnPoint");
        var pos = spawn != null ? spawn.transform.position : transform.position;
        SpawnBurst(pos, echoFogSprites, new Color(0.55f, 0.2f, 0.35f, 0.6f), 2.2f, 14);
    }

    private void LoadSpritesIfNeeded()
    {
        if (ghostPhaseSprites == null || ghostPhaseSprites.Length == 0)
            ghostPhaseSprites = LoadSheet("Art/VFX/ghostphase");
        if (memorySprites == null || memorySprites.Length == 0)
            memorySprites = LoadSheet("Art/VFX/memory");
        if (echoFogSprites == null || echoFogSprites.Length == 0)
            echoFogSprites = LoadSheet("Art/VFX/echofog");
    }

    private static Sprite[] LoadSheet(string prefix)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        for (var i = 0; i < 6; i++)
        {
            var sprite = Resources.Load<Sprite>($"Art/VFX/{prefix}_{i:00}");
            if (sprite != null)
                list.Add(sprite);
        }
        return list.ToArray();
    }

    private static void SpawnBurst(Vector3 position, Sprite[] sprites, Color tint, float lifetime, int count)
    {
        if (sprites == null || sprites.Length == 0) return;

        var go = new GameObject("VFXBurst");
        go.transform.position = position;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = lifetime;
        main.startSpeed = 0.6f;
        main.startSize = 0.35f;
        main.startColor = tint;
        main.maxParticles = count;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.duration = lifetime;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var texture = sprites[0].texture;
        renderer.material.mainTexture = texture;

        Destroy(go, lifetime + 0.5f);
    }
}