using UnityEngine;

/// <summary>
/// Scene bootstrap — wires core managers and logs chapter start.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Managers")]
    public KeyManager keyManager;
    public UIManager uiManager;
    public EventBus eventBus;

    private void Awake()
    {
        if (eventBus == null)
            eventBus = Resources.Load<EventBus>("EventBus");

        if (keyManager != null && uiManager != null)
            uiManager.keyManager = keyManager;

        if (eventBus != null)
            eventBus.OnChapterCompleted += HandleChapterCompleted;

        EnsureStoryboardSystems();
    }

    private void EnsureStoryboardSystems()
    {
        if (GetComponent<ChapterBeatDirector>() == null)
            gameObject.AddComponent<ChapterBeatDirector>();
        if (GetComponent<GhostPhaseVFX>() == null)
            gameObject.AddComponent<GhostPhaseVFX>();
        if (GetComponent<ParticleVFXController>() == null)
            gameObject.AddComponent<ParticleVFXController>();

        if (FindFirstObjectByType<HideSpot>() == null)
            CreateHideArch();

        var passage = GameObject.Find("PassageZone");
        if (passage != null && passage.GetComponent<PassageEscapeZone>() == null)
            passage.AddComponent<PassageEscapeZone>();
    }

    private static void CreateHideArch()
    {
        var arch = new GameObject("HideArch");
        arch.transform.position = new Vector3(5.2f, 0f, 0f);
        var col = arch.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.4f, 2.6f);
        arch.AddComponent<HideSpot>();
    }

    private void Start()
    {
        Debug.Log("Locke & Key: Chapter 1 — Welcome to Keyhouse");
    }

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnChapterCompleted -= HandleChapterCompleted;
    }

    private void HandleChapterCompleted()
    {
        Debug.Log("Chapter 1 complete.");
    }
}