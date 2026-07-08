using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Persists Chapter 1 progress: keys, puzzles, beat, position, and completion flag.
/// </summary>
public class ChapterSaveManager : MonoBehaviour
{
    public const string SaveFileName = "chapter1_save.json";

    [SerializeField] private bool autoSave = true;
    [SerializeField] private float positionSaveInterval = 2f;

    private static readonly Dictionary<int, Vector2> BeatCheckpoints = new()
    {
        { 0, new Vector2(-2f, -1f) },
        { 1, new Vector2(-1f, -1f) },
        { 2, new Vector2(2f, -1f) },
        { 3, new Vector2(4.5f, -1f) },
        { 4, new Vector2(5.2f, -1f) },
        { 5, new Vector2(8.5f, -1f) },
    };

    private ChapterSaveData data = new();
    private float positionSaveTimer;
    private bool isApplying;

    public static ChapterSaveManager Instance { get; private set; }

    public ChapterSaveData Data => data;
    public bool HasSaveFile => File.Exists(GetSavePath());

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadOrCreate();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveNow();
    }

    private void OnApplicationQuit()
    {
        SaveNow();
    }

    private void Start()
    {
        ApplyLoadedState();
    }

    private void Update()
    {
        if (!autoSave || isApplying) return;

        positionSaveTimer += Time.deltaTime;
        if (positionSaveTimer < positionSaveInterval) return;

        positionSaveTimer = 0f;
        CapturePlayerPosition();
        WriteSave();
    }

    public void LoadOrCreate()
    {
        var path = GetSavePath();
        if (!File.Exists(path))
        {
            data = new ChapterSaveData();
            SeedCheckpointForBeat(0);
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            data = JsonUtility.FromJson<ChapterSaveData>(json) ?? new ChapterSaveData();
            if (data.solvedPuzzleIds == null)
                data.solvedPuzzleIds = new List<string>();
            if (data.collectedPickupIds == null)
                data.collectedPickupIds = new List<string>();

            if (data.version < 2)
                MigrateV1ToV2();
        }
        catch
        {
            data = new ChapterSaveData();
        }

        if (!data.hasSavedPosition)
            SeedCheckpointForBeat(data.currentBeat);
    }

    public void SaveNow()
    {
        CaptureRuntimeState();
        WriteSave();
    }

    public void ResetSave()
    {
        data = new ChapterSaveData();
        SeedCheckpointForBeat(0);
        WriteSave();
    }

    public Vector3 GetCheckpointPosition()
    {
        if (data.checkpointX != 0f || data.checkpointY != 0f)
            return new Vector3(data.checkpointX, data.checkpointY, 0f);

        return BeatCheckpoints.TryGetValue(data.currentBeat, out var pos)
            ? new Vector3(pos.x, pos.y, 0f)
            : Vector3.zero;
    }

    public void RecordCheckpoint(Vector3 position)
    {
        data.checkpointX = position.x;
        data.checkpointY = position.y;
        WriteSave();
    }

    public void RecordCheckpointForBeat(int beatIndex)
    {
        data.currentBeat = Mathf.Max(data.currentBeat, beatIndex);
        var pos = BeatCheckpoints.TryGetValue(beatIndex, out var cp)
            ? cp
            : new Vector2(data.playerX, data.playerY);
        data.checkpointX = pos.x;
        data.checkpointY = pos.y;
        WriteSave();
    }

    public bool IsPuzzleSolved(string puzzleId) =>
        !string.IsNullOrEmpty(puzzleId) && data.solvedPuzzleIds.Contains(puzzleId);

    public bool IsPickupCollected(string pickupId) =>
        !string.IsNullOrEmpty(pickupId) && data.collectedPickupIds.Contains(pickupId);

    public void RecordPuzzleSolved(string puzzleId)
    {
        if (string.IsNullOrEmpty(puzzleId) || data.solvedPuzzleIds.Contains(puzzleId))
            return;

        data.solvedPuzzleIds.Add(puzzleId);
        if (puzzleId == "chapter1_bookshelf")
            data.ghostKeyRevealed = true;
        CapturePlayerPosition();
        WriteSave();
    }

    public void RecordPickupCollected(string pickupId)
    {
        if (string.IsNullOrEmpty(pickupId) || data.collectedPickupIds.Contains(pickupId))
            return;

        data.collectedPickupIds.Add(pickupId);
        CapturePlayerPosition();
        WriteSave();
    }

    public void RecordBeat(int beatIndex)
    {
        if (beatIndex <= data.currentBeat) return;
        data.currentBeat = beatIndex;
        RecordCheckpointForBeat(beatIndex);
    }

    public void RecordChapterComplete()
    {
        data.chapterComplete = true;
        data.echoEncounterActive = false;
        WriteSave();
    }

    public void RecordEchoCleared()
    {
        data.echoEncounterCleared = true;
        data.echoEncounterActive = false;
        WriteSave();
    }

    public void RecordEchoEncounterStarted()
    {
        data.echoEncounterActive = true;
        data.echoEncounterCleared = false;
        WriteSave();
    }

    public void RecordRoom(int roomIndex)
    {
        data.currentRoom = roomIndex;
        WriteSave();
    }

    private void ApplyLoadedState()
    {
        isApplying = true;

        var player = FindFirstObjectByType<PlayerController>();
        if (player != null && data.hasSavedPosition)
            player.transform.position = new Vector3(data.playerX, data.playerY, 0f);
        else if (player != null && data.currentBeat > 0)
            player.transform.position = GetCheckpointPosition();

        var inventory = FindFirstObjectByType<PlayerInventory>();
        inventory?.RestoreHouseKey(data.hasHouseKey);

        var keyManager = FindFirstObjectByType<KeyManager>();
        keyManager?.RestoreFromSave(data);

        foreach (var puzzle in FindObjectsByType<PuzzleBase>(FindObjectsSortMode.None))
        {
            if (puzzle != null && IsPuzzleSolved(puzzle.puzzleID))
                puzzle.RestoreSolvedState();
        }

        foreach (var pickup in FindObjectsByType<SaveablePickup>(FindObjectsSortMode.None))
            pickup.RestoreFromSave(this);

        foreach (var ghostPickup in FindObjectsByType<GhostKeyPickup>(FindObjectsSortMode.None))
            ghostPickup.RestoreFromSave(this);

        foreach (var headPickup in FindObjectsByType<HeadKeyPickup>(FindObjectsSortMode.None))
            headPickup.RestoreFromSave(this);

        var beatDirector = FindFirstObjectByType<ChapterBeatDirector>();
        beatDirector?.RestoreFromSave(data.currentBeat);

        var echoManager = FindFirstObjectByType<EchoEncounterManager>();
        echoManager?.RestoreFromSave(data);

        if (data.chapterComplete)
            Resources.Load<EventBus>("EventBus")?.ChapterCompleted();

        isApplying = false;
    }

    private void CaptureRuntimeState()
    {
        CapturePlayerPosition();

        var inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
            data.hasHouseKey = inventory.HasHouseKey;

        var keyManager = FindFirstObjectByType<KeyManager>();
        if (keyManager != null)
            keyManager.CaptureToSave(data);

        var beatDirector = FindFirstObjectByType<ChapterBeatDirector>();
        if (beatDirector != null)
            data.currentBeat = (int)beatDirector.CurrentBeat;

        var roomDirector = FindFirstObjectByType<ChapterRoomDirector>();
        if (roomDirector != null)
            data.currentRoom = (int)roomDirector.CurrentRoom;
    }

    private void CapturePlayerPosition()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var pos = player.transform.position;
        data.playerX = pos.x;
        data.playerY = pos.y;
        data.hasSavedPosition = true;
    }

    private void MigrateV1ToV2()
    {
        data.version = 2;
        if (data.playerX != 0f || data.playerY != 0f)
            data.hasSavedPosition = true;
        if (IsPuzzleSolved("chapter1_bookshelf"))
            data.ghostKeyRevealed = true;
        if (data.currentBeat >= (int)ChapterBeatDirector.Beat.EchoEncounter && !data.echoEncounterCleared)
            data.echoEncounterActive = true;
        SeedCheckpointForBeat(data.currentBeat);
    }

    private void SeedCheckpointForBeat(int beatIndex)
    {
        if (BeatCheckpoints.TryGetValue(beatIndex, out var cp))
        {
            data.checkpointX = cp.x;
            data.checkpointY = cp.y;
        }
    }

    private void WriteSave()
    {
        try
        {
            var dir = Path.GetDirectoryName(GetSavePath());
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(GetSavePath(), JsonUtility.ToJson(data, prettyPrint: true));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Chapter save failed: {ex.Message}");
        }
    }

    private static string GetSavePath() =>
        Path.Combine(Application.persistentDataPath, SaveFileName);
}