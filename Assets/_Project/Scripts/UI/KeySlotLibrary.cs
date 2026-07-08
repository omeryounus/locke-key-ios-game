using UnityEngine;

[CreateAssetMenu(fileName = "KeySlotLibrary", menuName = "LockeKey/UI/Key Slot Library")]
public class KeySlotLibrary : ScriptableObject
{
    public Sprite empty;
    public Sprite ghostActive;
    public Sprite headActive;
    public Sprite cooldown;
    public Sprite discovered;

    private static KeySlotLibrary cached;

    public static KeySlotLibrary LoadDefault()
    {
        if (cached != null)
            return cached;

        cached = Resources.Load<KeySlotLibrary>("Art/UI/KeySlots/KeySlotLibrary");
        return cached;
    }
}