using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class BirdController : NetworkBehaviour
{
    public float jumpForce = 3f;
    public float moveSpeed = 2f;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private Rigidbody2D rb;
    private bool isDead = false;
    private bool canPlay = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            canPlay = true;
            Debug.Log($"🎮 Player {OwnerClientId} is ready to play.");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsOwner || isDead || !canPlay) return;

        HandleInput();
    }

    [ClientRpc]
    public void PreGameFreezeClientRpc()
    {
        if (!IsOwner) return;

        canPlay = false;
        isDead = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        if (!IsOwner) return;

        spriteRenderer.enabled = true;
        col.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        canPlay = true;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Pipe") || collision.gameObject.CompareTag("Ground"))
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;

        if (collision.CompareTag("Score Zone"))
        {
            Debug.Log("Scored!");
        }
    }
}
