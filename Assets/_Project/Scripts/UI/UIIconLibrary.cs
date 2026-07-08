using UnityEngine;

/// <summary>
/// Loads touch-control and key HUD sprites from Resources/Art/UI.
/// </summary>
[CreateAssetMenu(fileName = "UIIconLibrary", menuName = "LockeKey/UI/Icon Library")]
public class UIIconLibrary : ScriptableObject
{
    [Header("Touch Controls")]
    public Sprite moveLeft;
    public Sprite moveRight;
    public Sprite jump;
    public Sprite interact;
    public Sprite useKey;

    [Header("Key Status")]
    public Sprite ghostKeyIcon;
    public Sprite headKeyIcon;
    public Sprite houseKeyIcon;

    private static UIIconLibrary cached;

    public static UIIconLibrary LoadDefault()
    {
        if (cached != null)
            return cached;

        cached = Resources.Load<UIIconLibrary>("Art/UI/UIIconLibrary");
        return cached;
    }
}