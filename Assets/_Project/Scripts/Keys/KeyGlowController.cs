using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Colored 2D point light that follows the player's active key.
/// </summary>
public class KeyGlowController : MonoBehaviour
{
    [SerializeField] private KeyManager keyManager;
    [SerializeField] private float ghostIntensity = 0.85f;
    [SerializeField] private float headIntensity = 0.75f;
    [SerializeField] private float idleIntensity = 0.2f;

    private Light2D glowLight;
    private float pulseTimer;

    private void Awake()
    {
        if (keyManager == null)
            keyManager = FindFirstObjectByType<KeyManager>();

        glowLight = GetComponentInChildren<Light2D>();
        if (glowLight == null)
            glowLight = CreateGlowLight();

        ApplyIdleGlow();
    }

    private void Update()
    {
        pulseTimer += Time.deltaTime * 2.5f;
        UpdateGlowForActiveKey();
    }

    private void UpdateGlowForActiveKey()
    {
        if (glowLight == null || keyManager?.currentActiveKey == null)
        {
            ApplyIdleGlow();
            return;
        }

        switch (keyManager.currentActiveKey.abilityType)
        {
            case KeyManager.KeyAbilityType.GhostPhase:
                glowLight.color = new Color(0.45f, 0.85f, 1f, 1f);
                glowLight.intensity = ghostIntensity + Mathf.Sin(pulseTimer) * 0.15f;
                glowLight.pointLightOuterRadius = 3.2f;
                break;

            case KeyManager.KeyAbilityType.HeadMemory:
                glowLight.color = new Color(0.85f, 0.45f, 0.95f, 1f);
                glowLight.intensity = headIntensity + Mathf.Sin(pulseTimer) * 0.12f;
                glowLight.pointLightOuterRadius = 2.8f;
                break;

            default:
                glowLight.color = new Color(0.7f, 0.65f, 0.9f, 1f);
                glowLight.intensity = 0.45f;
                glowLight.pointLightOuterRadius = 2.2f;
                break;
        }
    }

    private void ApplyIdleGlow()
    {
        if (glowLight == null) return;
        glowLight.color = new Color(0.55f, 0.6f, 0.8f, 1f);
        glowLight.intensity = idleIntensity;
        glowLight.pointLightOuterRadius = 1.8f;
    }

    private Light2D CreateGlowLight()
    {
        var glowGo = new GameObject("KeyGlow");
        glowGo.transform.SetParent(transform, false);
        glowGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);

        var light = glowGo.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.intensity = idleIntensity;
        light.pointLightInnerRadius = 0.2f;
        light.pointLightOuterRadius = 1.8f;
        light.falloffIntensity = 0.5f;
        light.color = new Color(0.55f, 0.6f, 0.8f, 1f);
        return light;
    }
}