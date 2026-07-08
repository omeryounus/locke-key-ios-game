using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Touch hold button for move-left/move-right controls.
/// </summary>
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEngine.Events.UnityEvent onDown = new();
    public UnityEngine.Events.UnityEvent onUp = new();

    public void OnPointerDown(PointerEventData eventData) => onDown.Invoke();
    public void OnPointerUp(PointerEventData eventData) => onUp.Invoke();
    public void OnPointerExit(PointerEventData eventData) => onUp.Invoke();
}