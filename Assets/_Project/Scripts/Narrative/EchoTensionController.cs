using UnityEngine;

/// <summary>
/// Ramps narrative tension while Echo entities are active and have line-of-sight.
/// </summary>
public class EchoTensionController : MonoBehaviour
{
    [SerializeField] private float maxSenseDistance = 10f;
    [SerializeField] private float baseTension = 0.55f;
    [SerializeField] private float maxTension = 0.95f;

    private EventBus eventBus;

    private void Awake()
    {
        eventBus = Resources.Load<EventBus>("EventBus");
    }

    private void Update()
    {
        if (eventBus == null) return;

        var echoes = FindObjectsByType<EchoEntity>(FindObjectsSortMode.None);
        if (echoes.Length == 0) return;

        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var closest = maxSenseDistance;
        var anyLos = false;
        foreach (var echo in echoes)
        {
            if (echo == null) continue;
            var dist = Vector2.Distance(echo.transform.position, player.transform.position);
            if (dist < closest)
                closest = dist;
            if (echo.HasLineOfSight)
                anyLos = true;
        }

        if (!anyLos) return;

        var ramp = 1f - Mathf.Clamp01(closest / maxSenseDistance);
        eventBus.SetTension(Mathf.Lerp(baseTension, maxTension, ramp));
    }
}