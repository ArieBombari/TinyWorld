using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Data")]
    [SerializeField] private PlayerMovementData movementData;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    // Input System
    private PlayerInputActions playerInput;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintPressed;

    // Components
    private CharacterController controller;
    private Transform cameraTransform;
    private ClimbingController climbingController;
    private GlidingController glidingController;
    private SwimmingController swimmingController;
    private DialogueRunner dialogueRunner;

    // Movement variables
    private Vector3 velocity;
    private bool isGrounded;
    private int jumpsRemaining;
    private int maxJumps;
    private float lastLandTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        glidingController = GetComponent<GlidingController>();
        cameraTransform = Camera.main.transform;
        climbingController = GetComponent<ClimbingController>();
        swimmingController = GetComponent<SwimmingController>();
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        
        playerInput = new PlayerInputActions();
        playerInput.Player.Enable();
    }

    void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.Player.Enable();
        }
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.Player.Disable();
        }
    }

    void Update()
    {
    // Don't run if dialogue is active
    if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        return;

    // Don't run if gliding
    if (glidingController != null && glidingController.IsGliding())
        return;
    
    // Don't run if climbing
    if (climbingController != null && climbingController.IsClimbing())
        return;

    // Handle swimming
    if (swimmingController != null && swimmingController.IsSwimming())
    {
        HandleSwimming();
        return;
    }

    // Normal ground/air movement
    ReadInput();
    CheckGrounded();
    HandleMovement();
    HandleJump();
    ApplyGravity();
    }

    void ReadInput()
    {
        moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        jumpPressed = playerInput.Player.Jump.WasPressedThisFrame();
        sprintPressed = playerInput.Player.Sprint.IsPressed();
    }

    void CheckGrounded()
    {
        bool wasGroundedBefore = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded && !wasGroundedBefore)
        {
            velocity.y = -2f;
            
            maxJumps = 1 + (FeatherManager.Instance != null ? FeatherManager.Instance.GetFeathersCollected() : 0);
            
            int currentFeathers = FeatherManager.Instance != null ? FeatherManager.Instance.GetCurrentFeathers() : 0;
            jumpsRemaining = 1 + currentFeathers;
            
            if (FeatherManager.Instance != null)
            {
                FeatherManager.Instance.AllowRecharge();
            }
            
            if (Time.time - lastLandTime > 0.3f)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLand();
                    lastLandTime = Time.time;
                }
            }
        }
        else if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMovement()
    {
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        float currentSpeed = sprintPressed ? movementData.sprintSpeed : movementData.moveSpeed;

        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, movementData.rotationSpeed * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        if (jumpPressed && jumpsRemaining > 0)
        {
            bool isAirJump = !isGrounded;
            
            if (isAirJump && FeatherManager.Instance != null)
            {
                FeatherManager.Instance.UseFeather();
            }
            
            if (FeatherManager.Instance != null)
            {
                FeatherManager.Instance.BlockRecharge();
            }
            
            velocity.y = Mathf.Sqrt(movementData.jumpForce * -2f * movementData.gravity);
            jumpsRemaining--;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJump();
            }
        }
    }

    void ApplyGravity()
    {
        velocity.y += movementData.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    public void ResetVelocity()
    {
        velocity.y = -10f;
    }

    public void RestoreJumpsBasedOnFeathers()
    {
    // Allow restoration both on ground AND while swimming
    bool canRestore = isGrounded || (swimmingController != null && swimmingController.IsSwimming());
    
    if (canRestore)
    {
        int currentFeathers = FeatherManager.Instance != null ? FeatherManager.Instance.GetCurrentFeathers() : 0;
        jumpsRemaining = 1 + currentFeathers;
        
        Debug.Log($"Jumps restored! Now have {jumpsRemaining} jumps available");
    }
    }

    void HandleSwimming()
    {
    ReadInput();
    
    if (FeatherManager.Instance != null)
    {
        int currentFeathers = FeatherManager.Instance.GetCurrentFeathers();
        int totalCollected = FeatherManager.Instance.GetFeathersCollected();
        
        maxJumps = 1 + totalCollected;
        jumpsRemaining = 1 + currentFeathers;
    }
    
    float horizontal = moveInput.x;
    float vertical = moveInput.y;

    Vector3 cameraForward = cameraTransform.forward;
    Vector3 cameraRight = cameraTransform.right;
    
    cameraForward.y = 0f;
    cameraRight.y = 0f;
    cameraForward.Normalize();
    cameraRight.Normalize();

    Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

    float currentSpeed = sprintPressed ? swimmingController.GetSwimSprintSpeed() : swimmingController.GetSwimSpeed();

    if (moveDirection.magnitude >= 0.1f)
    {
        // Horizontal movement
        Vector3 horizontalMovement = moveDirection * currentSpeed * Time.deltaTime;
        horizontalMovement.y = 0;
        controller.Move(horizontalMovement);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, movementData.rotationSpeed * Time.deltaTime);
    }
    
    // Handle jump from water
    if (jumpPressed && jumpsRemaining > 0)
    {
        bool isFirstJump = (jumpsRemaining == maxJumps);
        
        if (!isFirstJump && FeatherManager.Instance != null)
        {
            FeatherManager.Instance.UseFeather();
        }
        
        if (FeatherManager.Instance != null)
        {
            FeatherManager.Instance.BlockRecharge();
        }
        
        velocity.y = Mathf.Sqrt(movementData.jumpForce * -2f * movementData.gravity);
        jumpsRemaining--;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJump();
        }
        
        swimmingController.OnJumpFromWater();
        return;
    }
    
    // Keep player at swimming depth - NO CharacterController disabling!
    float waterSurfaceY = swimmingController.GetWaterSurfaceY();
    float submersionDepth = swimmingController.GetSubmersionDepth();
    float targetY = waterSurfaceY - submersionDepth;
    float currentY = transform.position.y;
    
    // Smooth vertical movement toward target depth
    float yDifference = targetY - currentY;
    Vector3 verticalCorrection = new Vector3(0, yDifference * 5f * Time.deltaTime, 0);
    controller.Move(verticalCorrection);
    
    // Clear Y velocity
    velocity.y = 0f;
    }

}