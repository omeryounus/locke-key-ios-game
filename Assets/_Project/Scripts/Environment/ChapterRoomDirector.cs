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
        ChapterSaveManager.Instance?.RecordRoom((int)room);
        ApplyRoomAmbience(room);

        var index = (int)room;
        if (index < 0 || index >= visited.Length || visited[index]) return;

        visited[index] = true;
        ShowFirstVisitToast(room);
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