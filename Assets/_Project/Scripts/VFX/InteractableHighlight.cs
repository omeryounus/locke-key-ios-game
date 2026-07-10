using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Soft glow pulse on the nearest interactable (color + optional Light2D).
/// Does not fight host scale animations (keys already bob).
/// </summary>
public class InteractableHighlight : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 4.2f;
    [SerializeField] private Color highlightTint = new(1f, 0.94f, 0.72f, 1f);

    private SpriteRenderer spriteRenderer;
    private Color baseColor = Color.white;
    private bool active;
    private float phase;
    private Light2D boostLight;
    private bool ownsBoostLight;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    public void SetHighlighted(bool on)
    {
        if (active == on) return;
        active = on;
        if (!on)
            Restore();
        else
            EnsureBoostLight();
    }

    private void EnsureBoostLight()
    {
        if (boostLight != null) return;
        boostLight = GetComponent<Light2D>();
        if (boostLight == null)
        {
            boostLight = gameObject.AddComponent<Light2D>();
            ownsBoostLight = true;
            boostLight.lightType = Light2D.LightType.Point;
            boostLight.color = highlightTint;
            boostLight.pointLightInnerRadius = 0.05f;
            boostLight.pointLightOuterRadius = 1.4f;
            boostLight.intensity = 0f;
        }
    }

    private void Update()
    {
        if (!active) return;

        phase += Time.deltaTime * pulseSpeed;
        var mix = 0.45f + Mathf.Sin(phase) * 0.35f;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.Lerp(baseColor, highlightTint, mix);

        if (boostLight != null)
            boostLight.intensity = 0.35f + Mathf.Sin(phase * 1.2f) * 0.35f;
    }

    private void OnDisable() => Restore();

    private void Restore()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
        if (boostLight != null)
        {
            if (ownsBoostLight)
                boostLight.intensity = 0f;
        }

        phase = 0f;
    }

    public static InteractableHighlight Ensure(Component target)
    {
        if (target == null) return null;
        var h = target.GetComponent<InteractableHighlight>();
        if (h == null)
            h = target.gameObject.AddComponent<InteractableHighlight>();
        return h;
    }
}
