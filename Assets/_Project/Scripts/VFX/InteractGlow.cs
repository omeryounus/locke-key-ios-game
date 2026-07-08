using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Soft pulsing point light for interactable pickups.
/// </summary>
public class InteractGlow : MonoBehaviour
{
    [SerializeField] private Color glowColor = new(1f, 0.88f, 0.45f, 1f);
    [SerializeField] private float baseIntensity = 0.55f;
    [SerializeField] private float pulseAmount = 0.35f;
    [SerializeField] private float pulseSpeed = 2.4f;

    private Light2D light2D;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        if (light2D == null)
            light2D = gameObject.AddComponent<Light2D>();

        light2D.lightType = Light2D.LightType.Point;
        light2D.color = glowColor;
        light2D.pointLightInnerRadius = 0.1f;
        light2D.pointLightOuterRadius = 1.2f;
        light2D.intensity = baseIntensity;
    }

    private void Update()
    {
        if (light2D == null) return;
        light2D.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
    }
}