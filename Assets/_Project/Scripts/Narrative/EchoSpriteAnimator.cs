using UnityEngine;

/// <summary>
/// Echo animation using production frames: idle cycle + attack/hurt/death when available.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EchoSpriteAnimator : MonoBehaviour
{
    public enum Clip { Idle, Attack, Hurt, Death }

    [SerializeField] private float frameRate = 7f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] idleFrames = System.Array.Empty<Sprite>();
    private Sprite attackFrame;
    private Sprite hurtFrame;
    private Sprite deathFrame;
    private int frameIndex;
    private float timer;
    private Clip clip = Clip.Idle;
    private float specialTimer;

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

        idleFrames = list.ToArray();
        // Production named frames may also live as echo_attack etc. under Production — also try Resources aliases
        attackFrame = Resources.Load<Sprite>("Art/Enemies/echo_01") ?? (idleFrames.Length > 1 ? idleFrames[1] : null);
        hurtFrame = Resources.Load<Sprite>("Art/Enemies/echo_02") ?? (idleFrames.Length > 2 ? idleFrames[2] : null);
        deathFrame = Resources.Load<Sprite>("Art/Enemies/echo_03") ?? (idleFrames.Length > 3 ? idleFrames[3] : null);

        if (idleFrames.Length > 0)
            spriteRenderer.sprite = idleFrames[0];
    }

    public void Play(Clip c, float duration = 0.45f)
    {
        clip = c;
        specialTimer = duration;
        if (c == Clip.Attack && attackFrame != null) spriteRenderer.sprite = attackFrame;
        else if (c == Clip.Hurt && hurtFrame != null) spriteRenderer.sprite = hurtFrame;
        else if (c == Clip.Death && deathFrame != null) spriteRenderer.sprite = deathFrame;
    }

    private void Update()
    {
        if (specialTimer > 0f)
        {
            specialTimer -= Time.deltaTime;
            if (specialTimer <= 0f && clip != Clip.Death)
                clip = Clip.Idle;
            else
                return;
        }

        if (clip == Clip.Death) return;
        if (idleFrames.Length == 0) return;

        timer += Time.deltaTime * frameRate;
        if (timer < 1f) return;
        timer = 0f;
        frameIndex = (frameIndex + 1) % idleFrames.Length;
        spriteRenderer.sprite = idleFrames[frameIndex];
    }
}
