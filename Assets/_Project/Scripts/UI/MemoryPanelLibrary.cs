using UnityEngine;

[CreateAssetMenu(fileName = "MemoryPanelLibrary", menuName = "LockeKey/UI/Memory Panel Library")]
public class MemoryPanelLibrary : ScriptableObject
{
    public Sprite panel1;
    public Sprite panel2;
    public Sprite panel3;

    private static MemoryPanelLibrary cached;

    public static MemoryPanelLibrary LoadDefault()
    {
        if (cached != null)
            return cached;
        cached = Resources.Load<MemoryPanelLibrary>("Art/Memory/MemoryPanelLibrary");
        return cached;
    }

    public Sprite GetPanel(int index)
    {
        return index switch
        {
            1 => panel1,
            2 => panel2,
            3 => panel3,
            _ => panel1
        };
    }
}