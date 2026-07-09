using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool HasContinuableSave =>
        HasSaveFile && HasProgress(data);

    public string SaveSummary =>
        $"Beat {(ChapterBeatDirector.Beat)data.currentBeat} | House:{data.hasHouseKey} Ghost:{data.hasGhostKey} Head:{data.hasHeadKey}";

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
            if (data.discoveredKeyIds == null)
                data.discoveredKeyIds = new List<string>();
            if (data.unlockedRoomIds == null)
                data.unlockedRoomIds = new List<string> { "foyer" };
            if (!data.unlockedRoomIds.Contains("foyer"))
                data.unlockedRoomIds.Add("foyer");
            if (data.solvedHotspotIds == null)
                data.solvedHotspotIds = new List<string>();
            if (data.codexUnlockedKeyIds == null)
                data.codexUnlockedKeyIds = new List<string>();

            if (data.version < 2)
                MigrateV1ToV2();

            // Keep legacy solvedPuzzleIds and new solvedHotspotIds in sync.
            MirrorHotspotIds();
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

    public void StartNewGame()
    {
        ResetSave();
        ReloadChapterScene();
    }

    public void ContinueGame()
    {
        LoadOrCreate();
        ReloadChapterScene();
    }

    public void ResetChapterSaveAndReload()
    {
        ResetSave();
        ReloadChapterScene();
    }

    public static void ReloadChapterScene()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public static bool HasContinuableSaveOnDisk()
    {
        var save = TryLoadFromDisk();
        return save != null && HasProgress(save);
    }

    public static bool HasCompletedOnboardingOnDisk()
    {
        var save = TryLoadFromDisk();
        return save != null && save.hasCompletedOnboarding;
    }

    public static void RecordOnboardingCompleteOnDisk()
    {
        var save = TryLoadFromDisk() ?? new ChapterSaveData();
        save.hasCompletedOnboarding = true;
        if (save.unlockedRoomIds == null || save.unlockedRoomIds.Count == 0)
            save.unlockedRoomIds = new List<string> { "foyer" };
        WriteSaveDataToDisk(save);
    }

    public static void ResetOnboardingOnDisk()
    {
        var save = TryLoadFromDisk();
        if (save == null) return;
        save.hasCompletedOnboarding = false;
        WriteSaveDataToDisk(save);
    }

    /// <summary>Replay Story: reset onboarding flag only, keep keys and progress.</summary>
    public static void ReplayStoryFromDisk()
    {
        if (Instance != null)
        {
            Instance.SaveNow();
            Instance.ResetOnboardingOnly();
            return;
        }

        ResetOnboardingOnDisk();
    }

    public static string ReadSaveSummaryFromDisk()
    {
        var save = TryLoadFromDisk();
        if (save == null || !HasProgress(save))
            return string.Empty;

        return $"Beat {(ChapterBeatDirector.Beat)save.currentBeat} | House:{save.hasHouseKey} Ghost:{save.hasGhostKey} Head:{save.hasHeadKey}";
    }

    public static void StartNewGameFromTitle()
    {
        ResetSaveOnDisk();
        SceneManager.LoadScene(SceneNames.Chapter1);
    }

    public static void ContinueFromTitle()
    {
        SceneManager.LoadScene(SceneNames.Chapter1);
    }

    public static void ReturnToTitle()
    {
        if (Instance != null)
            Instance.SaveNow();

        SceneManager.LoadScene(SceneNames.Title);
    }

    public static void ReplayChapterFromEnd()
    {
        if (Instance != null)
            Instance.ResetSave();
        else
            ResetSaveOnDisk();

        SceneManager.LoadScene(SceneNames.Chapter1);
    }

    private static ChapterSaveData TryLoadFromDisk()
    {
        var path = GetSavePath();
        if (!File.Exists(path))
            return null;

        try
        {
            var json = File.ReadAllText(path);
            var save = JsonUtility.FromJson<ChapterSaveData>(json);
            if (save == null)
                return null;

            save.solvedPuzzleIds ??= new List<string>();
            save.collectedPickupIds ??= new List<string>();
            return save;
        }
        catch
        {
            return null;
        }
    }

    public static void ResetSaveOnDisk()
    {
        var save = new ChapterSaveData();
        if (BeatCheckpoints.TryGetValue(0, out var checkpoint))
        {
            save.checkpointX = checkpoint.x;
            save.checkpointY = checkpoint.y;
        }

        WriteSaveDataToDisk(save);
    }

    private static void WriteSaveDataToDisk(ChapterSaveData save)
    {
        try
        {
            var path = GetSavePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonUtility.ToJson(save, prettyPrint: true));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Chapter save failed: {ex.Message}");
        }
    }

    public static bool HasProgress(ChapterSaveData save)
    {
        if (save == null) return false;
        return save.currentBeat > 0
               || save.hasHouseKey
               || save.hasGhostKey
               || save.hasHeadKey
               || save.chapterComplete
               || (save.solvedPuzzleIds != null && save.solvedPuzzleIds.Count > 0)
               || (save.collectedPickupIds != null && save.collectedPickupIds.Count > 0);
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

    public bool IsGhostKeyRevealed => data.ghostKeyRevealed;

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

    // ── S0-S6 flow helpers ───────────────────────────────────────────────

    /// <summary>Record that the player has completed the S1 onboarding reel.</summary>
    public void RecordOnboardingComplete()
    {
        data.hasCompletedOnboarding = true;
        WriteSave();
    }

    /// <summary>Reset onboarding flag for Story Replay (keeps key progress).</summary>
    public void ResetOnboardingOnly()
    {
        data.hasCompletedOnboarding = false;
        WriteSave();
    }

    /// <summary>Record key discovered via S4 sheet. Also unlocks Codex entry.</summary>
    public void RecordKeyDiscovered(string keyId)
    {
        if (!data.discoveredKeyIds.Contains(keyId))
        {
            data.discoveredKeyIds.Add(keyId);
            if (!data.codexUnlockedKeyIds.Contains(keyId))
                data.codexUnlockedKeyIds.Add(keyId);
        }
        WriteSave();
    }

    /// <summary>Set the currently equipped key by id (e.g. "anywhere").</summary>
    public void RecordEquippedKey(string keyId)
    {
        data.equippedKeyId = keyId;
        WriteSave();
    }

    /// <summary>Record a hotspot solved (e.g. "foyer_stair_door") and mirror to legacy list.</summary>
    public void RecordHotspotSolved(string hotspotId)
    {
        if (!data.solvedHotspotIds.Contains(hotspotId))
            data.solvedHotspotIds.Add(hotspotId);
        MirrorHotspotIds();
        WriteSave();
    }

    /// <summary>Unlock a room node on the S2 Chapter Map (e.g. "wellhouse").</summary>
    public void RecordRoomUnlocked(string roomId)
    {
        if (!data.unlockedRoomIds.Contains(roomId))
            data.unlockedRoomIds.Add(roomId);
        WriteSave();
    }

    public bool IsHotspotSolved(string hotspotId) =>
        data.solvedHotspotIds.Contains(hotspotId);

    public bool IsRoomUnlocked(string roomId) =>
        data.unlockedRoomIds.Contains(roomId);

    public bool HasKeyDiscovered(string keyId) =>
        data.discoveredKeyIds.Contains(keyId);

    public string ActiveMapDestination =>
        string.IsNullOrEmpty(data.activeMapDestination)
            ? ChapterMapDestination.Foyer
            : data.activeMapDestination;

    public void RecordMapDestination(string destinationId)
    {
        if (string.IsNullOrEmpty(destinationId)) return;
        data.activeMapDestination = destinationId;
        WriteSave();
    }

    /// <summary>
    /// Keep legacy solvedPuzzleIds and new solvedHotspotIds in sync.
    /// Both lists are maintained so old scene scripts (PuzzleBase) keep working.
    /// </summary>
    private void MirrorHotspotIds()
    {
        // Legacy → new
        if (data.solvedPuzzleIds.Contains("chapter1_door") &&
            !data.solvedHotspotIds.Contains("foyer_stair_door"))
            data.solvedHotspotIds.Add("foyer_stair_door");

        // New → legacy
        if (data.solvedHotspotIds.Contains("foyer_stair_door") &&
            !data.solvedPuzzleIds.Contains("chapter1_door"))
            data.solvedPuzzleIds.Add("chapter1_door");

        // Mirror room unlock from hotspot
        if (data.solvedHotspotIds.Contains("foyer_stair_door") &&
            !data.unlockedRoomIds.Contains("wellhouse"))
            data.unlockedRoomIds.Add("wellhouse");
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