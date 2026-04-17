using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 10f;
    public float maxFallSpeed = -18f;
    public float fallGravityMultiplier = 2.5f;
    public float lowJumpGravityMultiplier = 2f;

    [Header("Ground Check")]
    public float groundCheckOffset = 1.0f;

    [Header("Jump Tuning")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    private Rigidbody rb;
    private bool controlEnabled = true;
    private int groundLayerMask;

    // Ground & Jump state
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool hasJumped;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        groundLayerMask = LayerMask.GetMask("Default");
        if (groundLayerMask == 0) groundLayerMask = 1;

        // Hide capsule renderer if character model exists
        Renderer capsuleRenderer = GetComponent<Renderer>();
        if (capsuleRenderer != null && transform.Find("CharacterModel") != null)
            capsuleRenderer.enabled = false;
    }

    void Update()
    {
        if (!controlEnabled) return;

        CheckGround();

        // Coyote time — grace period after leaving ground
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            hasJumped = false;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // Jump buffer — remember jump press for a short window
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Movement
        float moveInput = Input.GetAxis("Horizontal");
        if (ControlReverser.Instance != null && ControlReverser.Instance.IsReversed)
            moveInput *= -1f;
        Vector3 vel = rb.velocity;
        vel.x = moveInput * moveSpeed;

        rb.velocity = vel;

        // Jump — requires coyote time remaining AND jump buffer AND not already jumped
        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !hasJumped)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            hasJumped = true;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }

        // Lock Z position
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.z) > 0.01f)
        {
            pos.z = 0f;
            transform.position = pos;
        }
    }

    private void CheckGround()
    {
        // 3-ray ground check — center, left, right
        Vector3 origin = transform.position;
        float dist = groundCheckOffset + 0.1f;

        isGrounded = Physics.Raycast(origin, Vector3.down, dist, groundLayerMask, QueryTriggerInteraction.Ignore)
                  || Physics.Raycast(origin + Vector3.left * 0.25f, Vector3.down, dist, groundLayerMask, QueryTriggerInteraction.Ignore)
                  || Physics.Raycast(origin + Vector3.right * 0.25f, Vector3.down, dist, groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Better jump feel: fall faster, short hop if Space released early
        if (rb.velocity.y < 0)
        {
            // Falling — apply extra gravity for snappy descent
            rb.velocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Rising but Jump released — cut the jump short
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // Enforce fall speed cap
        if (rb.velocity.y < maxFallSpeed)
            rb.velocity = new Vector3(rb.velocity.x, maxFallSpeed, rb.velocity.z);
    }

    public void DisableControl()
    {
        controlEnabled = false;
        if (rb != null) rb.velocity = Vector3.zero;
    }

    public void EnableControl()
    {
        controlEnabled = true;
        hasJumped = false;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
    }

    public void ApplyBounce(float force)
    {
        if (rb != null)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
            hasJumped = true;
            coyoteTimer = 0f;
        }
    }
}
