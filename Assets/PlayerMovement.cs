using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float airControlMultiplier = 0.7f;
    [SerializeField] private float jumpForce = 11.5f;
    [SerializeField] private float maxFallSpeed = 18f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.12f;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector2 spawnPoint;
    private float horizontalInput;
    private bool jumpQueued;
    private bool isGrounded;
    private Transform visual;
    private SpriteRenderer visualRenderer;
    private Sprite[] moveFrames;
    private Sprite idleFrame;
    private Sprite jumpFrame;
    private float animationTimer;

    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spawnPoint = transform.position;
        groundLayer = groundLayer == 0 ? LayerMask.GetMask("Default") : groundLayer;

        visual = transform.Find("Visual");
        if (visual != null)
        {
            visualRenderer = visual.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        ReadDesktopAndGamepadInput();
        UpdateGroundedState();
        AnimateVisual();
    }

    private void FixedUpdate()
    {
        float control = isGrounded ? 1f : airControlMultiplier;
        float targetVelocityX = horizontalInput * moveSpeed;
        float nextVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, acceleration * control * Time.fixedDeltaTime * moveSpeed);

        rb.linearVelocity = new Vector2(nextVelocityX, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));

        if (jumpQueued && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        jumpQueued = false;
    }

    public void SetMobileHorizontal(float direction)
    {
        horizontalInput = Mathf.Clamp(direction, -1f, 1f);
    }

    public void QueueMobileJump()
    {
        jumpQueued = true;
    }

    public void SetSpawnPoint(Vector2 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void SetAnimationFrames(Sprite standSprite, Sprite[] walkingSprites, Sprite airborneSprite = null)
    {
        if (visual == null)
        {
            visual = transform.Find("Visual");
        }

        if (visualRenderer == null && visual != null)
        {
            visualRenderer = visual.GetComponent<SpriteRenderer>();
        }

        idleFrame = standSprite;
        moveFrames = walkingSprites;
        jumpFrame = airborneSprite != null ? airborneSprite : standSprite;

        if (visualRenderer != null && idleFrame != null)
        {
            visualRenderer.sprite = idleFrame;
        }
    }

    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint;
    }

    private void ReadDesktopAndGamepadInput()
    {
        float keyboardInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                keyboardInput -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                keyboardInput += 1f;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }

        float stickInput = 0f;
        if (Gamepad.current != null)
        {
            stickInput = Gamepad.current.leftStick.ReadValue().x;

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }

        if (Mathf.Abs(keyboardInput) > 0.01f)
        {
            horizontalInput = keyboardInput;
        }
        else if (Mathf.Abs(stickInput) > 0.2f)
        {
            horizontalInput = stickInput;
        }
        else if (Touchscreen.current == null)
        {
            horizontalInput = 0f;
        }
    }

    private void UpdateGroundedState()
    {
        Bounds bounds = circleCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 size = new Vector2(bounds.size.x * 0.85f, 0.05f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != circleCollider && !hit.collider.isTrigger)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void AnimateVisual()
    {
        if (visual == null || visualRenderer == null)
        {
            return;
        }

        if (horizontalInput < -0.01f)
        {
            visual.localScale = new Vector3(-Mathf.Abs(visual.localScale.x), visual.localScale.y, 1f);
        }
        else if (horizontalInput > 0.01f)
        {
            visual.localScale = new Vector3(Mathf.Abs(visual.localScale.x), visual.localScale.y, 1f);
        }

        if (!isGrounded)
        {
            if (jumpFrame != null)
            {
                visualRenderer.sprite = jumpFrame;
            }
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.15f && moveFrames != null && moveFrames.Length > 0)
        {
            animationTimer += Time.deltaTime * 10f;
            int frameIndex = Mathf.FloorToInt(animationTimer) % moveFrames.Length;
            visualRenderer.sprite = moveFrames[frameIndex];
        }
        else if (idleFrame != null)
        {
            animationTimer = 0f;
            visualRenderer.sprite = idleFrame;
        }

        float stretchX = isGrounded ? 1.08f : 0.92f;
        float stretchY = isGrounded ? 0.92f : 1.08f;
        float facing = visual.localScale.x < 0f ? -1f : 1f;
        Vector3 targetScale = new Vector3(stretchX * facing, stretchY, 1f);
        visual.localScale = Vector3.Lerp(visual.localScale, targetScale, Time.deltaTime * 8f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EggCollectible egg = other.GetComponent<EggCollectible>();
        if (egg != null)
        {
            BounceGameManager.Instance?.CollectEgg(egg);
            return;
        }

        if (other.GetComponent<GoalTrigger>() != null)
        {
            BounceGameManager.Instance?.ReachGoal();
            return;
        }

        if (other.GetComponent<HazardZone>() != null)
        {
            BounceGameManager.Instance?.HitHazard();
        }
    }
}
