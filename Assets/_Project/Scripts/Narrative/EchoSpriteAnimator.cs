using UnityEngine;

/// <summary>
/// Animates Echo from authored fog sprite sheet frames.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EchoSpriteAnimator : MonoBehaviour
{
    [SerializeField] private float frameRate = 6f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames = System.Array.Empty<Sprite>();
    private int frameIndex;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadFrames();
    }

    private void LoadFrames()
    {
        var list = new System.Collections.Generic.List<Sprite>();
        for (var i = 0; i < 6; i++)
        {
            var sprite = Resources.Load<Sprite>($"Art/Enemies/echo_{i:00}");
            if (sprite != null)
                list.Add(sprite);
        }

        frames = list.ToArray();
        if (frames.Length > 0)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames.Length == 0) return;

        timer += Time.deltaTime * frameRate;
        if (timer < 1f) return;

        timer = 0f;
        frameIndex = (frameIndex + 1) % frames.Length;
        spriteRenderer.sprite = frames[frameIndex];
    }
}