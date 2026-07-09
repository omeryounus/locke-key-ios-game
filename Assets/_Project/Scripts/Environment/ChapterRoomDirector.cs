using UnityEngine;

/// <summary>
/// Tracks Chapter 1 room progression and first-visit guidance.
/// </summary>
public class ChapterRoomDirector : MonoBehaviour
{
    [SerializeField] private GameplayHUD hud;
    [SerializeField] private ParallaxLayer foyerParallax;
    [SerializeField] private ParallaxLayer libraryParallax;

    private ChapterRoomZone.RoomId currentRoom = ChapterRoomZone.RoomId.ExteriorEntrance;
    private readonly bool[] visited = new bool[5];

    public ChapterRoomZone.RoomId CurrentRoom => currentRoom;

    private void Awake()
    {
        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUD>();
    }

    public void EnterRoom(ChapterRoomZone.RoomId room)
    {
        if (room == currentRoom) return;

        currentRoom = room;
        var save = ChapterSaveManager.Instance;
        save?.RecordMapDestination(ChapterMapDestination.Foyer);
        save?.RecordRoom((int)room);
        ApplyRoomAmbience(room);
        RefreshHudRoomTitle();

        var index = (int)room;
        if (index < 0 || index >= visited.Length || visited[index]) return;

        visited[index] = true;
        ShowFirstVisitToast(room);
    }

    /// <summary>S2 card → load foyer or wellhouse gameplay view.</summary>
    public void LoadMapDestination(string destinationId)
    {
        var save = ChapterSaveManager.Instance;
        save?.RecordMapDestination(destinationId);

        switch (destinationId)
        {
            case ChapterMapDestination.Foyer:
                TeleportPlayer(-1.5f);
                currentRoom = ChapterRoomZone.RoomId.Foyer;
                save?.RecordRoom((int)currentRoom);
                ApplyRoomAmbience(currentRoom);
                RefreshHudRoomTitle();
                break;

            case ChapterMapDestination.Wellhouse:
                if (save != null && !save.IsHotspotSolved("foyer_stair_door"))
                {
                    GrokUIFlowManager.Instance?.ShowToast("Wellhouse is locked. Solve the foyer stair door first.");
                    return;
                }

                TeleportPlayer(0f);
                RefreshHudRoomTitle();
                break;
        }
    }

    private void RefreshHudRoomTitle()
    {
        if (hud == null) return;

        var save = ChapterSaveManager.Instance;
        if (save != null && save.ActiveMapDestination == ChapterMapDestination.Wellhouse)
            hud.SetRoomTitle(ChapterRoomLabels.ForMapDestination(ChapterMapDestination.Wellhouse));
        else
            hud.SetRoomTitle(ChapterRoomLabels.ForRoomId(currentRoom));
    }

    private static void TeleportPlayer(float x)
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var pos = player.transform.position;
        player.transform.position = new Vector3(x, pos.y, pos.z);
        ChapterSaveManager.Instance?.RecordCheckpoint(player.transform.position);
    }

    private void ApplyRoomAmbience(ChapterRoomZone.RoomId room)
    {
        if (foyerParallax != null)
            foyerParallax.enabled = room is ChapterRoomZone.RoomId.Foyer or ChapterRoomZone.RoomId.ExteriorEntrance;
        if (libraryParallax != null)
            libraryParallax.enabled = room is ChapterRoomZone.RoomId.Library
                or ChapterRoomZone.RoomId.SealedPassage
                or ChapterRoomZone.RoomId.MemoryPortrait;
    }

    private void ShowFirstVisitToast(ChapterRoomZone.RoomId room)
    {
        if (hud == null) return;

        switch (room)
        {
            case ChapterRoomZone.RoomId.ExteriorEntrance:
                hud.ShowToast("Keyhouse exterior — the door won't budge without a key.", 3.5f);
                break;
            case ChapterRoomZone.RoomId.Foyer:
                hud.ShowToast("Foyer — warm light leaks from the stuck door ahead.", 3.5f);
                break;
            case ChapterRoomZone.RoomId.Library:
                hud.ShowToast("Library — dust, shelves, and something hidden.", 3.5f);
                break;
            case ChapterRoomZone.RoomId.SealedPassage:
                hud.ShowToast("Sealed passage — old magic blocks the way.", 3.5f);
                break;
            case ChapterRoomZone.RoomId.MemoryPortrait:
                hud.ShowToast("Memory portrait — the Head Key may answer.", 3.5f);
                break;
        }
    }
}