using UnityEngine;

/// <summary>
/// Safe passage exit during the Echo encounter.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PassageEscapeZone : MonoBehaviour
{
    [SerializeField] private ChapterBeatDirector beatDirector;

    private void Awake()
    {
        if (beatDirector == null)
            beatDirector = FindFirstObjectByType<ChapterBeatDirector>();

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null)
            return;

        var echoes = FindObjectsByType<EchoEntity>(FindObjectsSortMode.None);
        foreach (var echo in echoes)
            Destroy(echo.gameObject);

        beatDirector?.NotifyEchoEscaped();
    }
}