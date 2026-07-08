using UnityEngine;

/// <summary>
/// Spawns the first Echo when Ghost Key risk triggers or the sealed door opens.
/// </summary>
public class EchoEncounterManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Sprite echoSprite;
    [SerializeField] private bool spawnOnGhostPhaseEnd = true;
    [SerializeField] private bool spawnOnEchoEvent = true;

    private EventBus eventBus;
    private PlayerController player;
    private bool hasSpawned;
    private bool encounterActive;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
        eventBus = Resources.Load<EventBus>("EventBus");

        if (eventBus != null)
        {
            if (spawnOnEchoEvent)
                eventBus.OnEchoTriggered += HandleEchoTriggered;

            if (spawnOnGhostPhaseEnd)
                eventBus.OnGhostPhaseEnded += HandleGhostPhaseEnded;
        }
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;

        eventBus.OnEchoTriggered -= HandleEchoTriggered;
        eventBus.OnGhostPhaseEnded -= HandleGhostPhaseEnded;
    }

    public void RestoreFromSave(ChapterSaveData save)
    {
        if (save == null) return;

        if (save.chapterComplete || save.echoEncounterCleared)
        {
            hasSpawned = true;
            encounterActive = false;
            return;
        }

        encounterActive = save.echoEncounterActive;
        hasSpawned = !encounterActive;

        if (encounterActive)
            SpawnEcho(force: true);
    }

    public void RespawnEchoIfNeeded()
    {
        if (!encounterActive || player == null) return;
        SpawnEcho(force: true);
    }

    private void HandleEchoTriggered()
    {
        SpawnEcho();
    }

    private void HandleGhostPhaseEnded()
    {
        var sealedDoor = FindFirstObjectByType<SealedDoorPuzzle>();
        if (sealedDoor == null || !sealedDoor.isSolved)
            return;

        SpawnEcho();
    }

    private void SpawnEcho(bool force = false)
    {
        if (player == null) return;
        if (!force && hasSpawned) return;

        foreach (var existing in FindObjectsByType<EchoEntity>(FindObjectsSortMode.None))
            Destroy(existing.gameObject);

        hasSpawned = true;
        encounterActive = true;
        ChapterSaveManager.Instance?.RecordEchoEncounterStarted();

        var echoGo = new GameObject("Echo");
        echoGo.transform.position = spawnPoint != null
            ? spawnPoint.position
            : player.transform.position + Vector3.left * 4f;

        var renderer = echoGo.AddComponent<SpriteRenderer>();
        renderer.sprite = echoSprite != null
            ? echoSprite
            : LoadEchoFrame() ?? CreatePlaceholderSprite();
        renderer.color = new Color(0.55f, 0.1f, 0.18f, 0.35f);
        renderer.sortingOrder = 6;

        echoGo.AddComponent<EchoSpriteAnimator>();
        var echo = echoGo.AddComponent<EchoEntity>();
        echo.Initialize(player.transform);

        eventBus?.SetTension(0.85f);
        Debug.Log("An Echo has been drawn to the Ghost Key's power...");
    }

    private static Sprite LoadEchoFrame() => Resources.Load<Sprite>("Art/Enemies/echo_00");

    private static Sprite CreatePlaceholderSprite()
    {
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}