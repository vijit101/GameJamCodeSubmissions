using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 6.5f;
    public float crouchSpeed = 1.8f;
    public float gravity = -15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookUpAngle = 80f;
    public float maxLookDownAngle = -80f;

    [Header("Head Bob")]
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.05f;
    public float sprintBobMultiplier = 1.4f;

    [Header("Posture")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float standingCameraY = 0.6f;
    public float crouchCameraY = 0.25f;

    private CharacterController controller;
    private Transform cameraTransform;
    private float verticalVelocity;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;        // looking-back yaw offset
    private float bobTimer = 0f;
    private float originalCameraY;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isGlancing = false;
    private float glanceLerp = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        originalCameraY = standingCameraY;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCrouch();
        HandleGlance();
        HandleMouseLook();
        HandleMovement();
        HandleHeadBob();
    }

    void HandleCrouch()
    {
        bool wantCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        if (wantCrouch != isCrouching)
        {
            isCrouching = wantCrouch;
            if (controller != null)
            {
                controller.height = isCrouching ? crouchHeight : standingHeight;
                controller.center = new Vector3(0, controller.height * 0.5f, 0);
            }
            originalCameraY = isCrouching ? crouchCameraY : standingCameraY;
        }
    }

    void HandleGlance()
    {
        // Hold right mouse button to glance behind.
        bool want = Input.GetMouseButton(1);
        isGlancing = want;
        glanceLerp = Mathf.MoveTowards(glanceLerp, want ? 1f : 0f, Time.deltaTime * 6f);
        cameraYaw = Mathf.Lerp(0f, 180f, glanceLerp);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Don't yaw the body while glancing (otherwise the world spins).
        if (!isGlancing) transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, maxLookDownAngle, maxLookUpAngle);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, cameraYaw, 0f);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized;

        bool wantSprint = Input.GetKey(KeyCode.LeftShift) && moveZ > 0 && !isCrouching;
        isSprinting = wantSprint;
        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        // Glancing slows you (you're literally looking the wrong way).
        if (isGlancing) speed *= 0.5f;

        if (controller.isGrounded) verticalVelocity = -2f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        if (!controller.isGrounded) return;

        bool moving = IsMoving();

        if (moving)
        {
            float multiplier = isCrouching ? 0.6f : (isSprinting ? sprintBobMultiplier : 1f);
            bobTimer += Time.deltaTime * bobFrequency * multiplier;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude * multiplier;

            Vector3 localPos = cameraTransform.localPosition;
            localPos.y = originalCameraY + bobOffset;
            cameraTransform.localPosition = localPos;
        }
        else
        {
            bobTimer = 0f;
            Vector3 localPos = cameraTransform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, originalCameraY, Time.deltaTime * 5f);
            cameraTransform.localPosition = localPos;
        }
    }

    public bool IsMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
    }

    public bool IsSprinting() => isSprinting;
    public bool IsCrouching() => isCrouching;
    public bool IsGlancing() => isGlancing;

    // Noise radius the player is broadcasting right now.
    public float NoiseRadius()
    {
        if (!IsMoving() || isCrouching) return 0f;
        return isSprinting ? 14f : 5f;
    }
}
