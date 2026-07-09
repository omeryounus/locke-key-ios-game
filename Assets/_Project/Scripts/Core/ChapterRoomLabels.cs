/// <summary>
/// Display labels for S3 HUD room title (from RoomId or map destination).
/// </summary>
public static class ChapterRoomLabels
{
    public static string ForRoomId(ChapterRoomZone.RoomId room) => room switch
    {
        ChapterRoomZone.RoomId.ExteriorEntrance => "Keyhouse Grounds",
        ChapterRoomZone.RoomId.Foyer => "Keyhouse Foyer",
        ChapterRoomZone.RoomId.Library => "Library",
        ChapterRoomZone.RoomId.SealedPassage => "Sealed Passage",
        ChapterRoomZone.RoomId.MemoryPortrait => "Memory Portrait",
        _ => "Keyhouse"
    };

    public static string ForMapDestination(string destinationId) => destinationId switch
    {
        ChapterMapDestination.Foyer => "Keyhouse Foyer",
        ChapterMapDestination.Wellhouse => "Wellhouse",
        ChapterMapDestination.BlackDoor => "The Black Door",
        _ => "Keyhouse"
    };
}