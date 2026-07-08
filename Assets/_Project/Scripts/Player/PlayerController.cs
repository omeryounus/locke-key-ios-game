using System.Collections;
using UnityEngine;

/// <summary>
/// 2.5D player movement and key-ability hooks for touch-first iOS gameplay.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpForce = 8f;

    [Header("Ghost Key")]
    public float ghostPhaseDuration = 5f;
    public LayerMask solidLayers;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isGhostPhasing;
    private Coroutine ghostPhaseRoutine;

    public bool IsGhostPhasing => isGhostPhasing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(float horizontalInput)
    {
        if (isGhostPhasing) return;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        if (isGhostPhasing || !isGrounded) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public void ActivateGhostPhase(float duration)
    {
        if (ghostPhaseRoutine != null)
            StopCoroutine(ghostPhaseRoutine);

        ghostPhaseRoutine = StartCoroutine(GhostPhaseRoutine(duration > 0 ? duration : ghostPhaseDuration));
    }

    public void TryMirrorTravel()
    {
        Debug.Log("Mirror Key: searching for nearby reflective surface...");
    }

    public void ManipulateShadows()
    {
        Debug.Log("Shadow Key: shadow manipulation mode active.");
    }

    private IEnumerator GhostPhaseRoutine(float duration)
    {
        isGhostPhasing = true;
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.isTrigger = true;

        yield return new WaitForSeconds(duration);

        if (collider != null)
            collider.isTrigger = false;

        isGhostPhasing = false;
        ghostPhaseRoutine = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}