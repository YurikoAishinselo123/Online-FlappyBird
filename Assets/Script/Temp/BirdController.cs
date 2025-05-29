using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BirdController : NetworkBehaviour
{
    public float jumpForce = 3f;
    public float moveSpeed = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private ScoreManager scoreManager;

    private bool isDead = false;
    private bool canPlay = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        scoreManager = GetComponent<ScoreManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            Debug.Log($"🎮 Player {OwnerClientId} is ready to play.");
            ResetPlayer(); // Ensure frozen state until countdown ends
        }
    }

    private void Update()
    {
        if (!IsOwner || isDead || !canPlay) return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Jump();
        }

        float h = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = rb.linearVelocity;
        velocity.x = h * moveSpeed;
        rb.linearVelocity = velocity;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (!IsOwner || isDead) return;

    //     if (collision.gameObject.CompareTag("Pipe") || collision.gameObject.CompareTag("Ground"))
    //     {
    //         Die();
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner || isDead) return;

        if (collision.CompareTag("Score Zone"))
        {
            // Debug.Log("🏆 Scored!");
            scoreManager?.AddScore(1); // Local-only
        }
    }


    private void Die()
    {
        isDead = true;
        canPlay = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Debug.Log("💀 Player died.");
        // Optionally: Trigger death UI or notify server here
    }

    private void ResetPlayer()
    {
        isDead = false;
        canPlay = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (col != null) col.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }

    [ClientRpc]
    public void PreGameFreezeClientRpc()
    {
        if (!IsOwner) return;
        ResetPlayer();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        if (!IsOwner) return;

        rb.bodyType = RigidbodyType2D.Dynamic;

        if (col != null) col.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        canPlay = true;
    }
}
