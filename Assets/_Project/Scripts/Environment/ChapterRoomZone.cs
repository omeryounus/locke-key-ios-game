using UnityEngine;

/// <summary>
/// Authored room trigger for Chapter 1 spatial flow.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ChapterRoomZone : MonoBehaviour
{
    public enum RoomId
    {
        ExteriorEntrance,
        Foyer,
        Library,
        SealedPassage,
        MemoryPortrait
    }

    [SerializeField] private RoomId room = RoomId.ExteriorEntrance;
    [SerializeField] private ChapterRoomDirector director;

    public RoomId Room => room;

    public void Configure(RoomId roomId) => room = roomId;

    private void Awake()
    {
        if (director == null)
            director = FindFirstObjectByType<ChapterRoomDirector>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        director?.EnterRoom(room);
    }
}