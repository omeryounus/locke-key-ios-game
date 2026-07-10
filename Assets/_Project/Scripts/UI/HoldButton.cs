using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Touch hold button for move-left/move-right with press scale feedback.
/// </summary>
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEngine.Events.UnityEvent onDown = new();
    public UnityEngine.Events.UnityEvent onUp = new();

    private bool held;
    private UIButtonFeedback feedback;

    private void Awake()
    {
        feedback = UIButtonFeedback.Ensure(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (held) return;
        held = true;
        onDown.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData) => Release();

    public void OnPointerExit(PointerEventData eventData) => Release();

    private void Release()
    {
        if (!held) return;
        held = false;
        onUp.Invoke();
    }

    private void OnDisable()
    {
        if (!held) return;
        held = false;
        onUp.Invoke();
    }
}
