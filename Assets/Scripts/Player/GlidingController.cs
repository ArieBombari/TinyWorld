using UnityEngine;
using UnityEngine.InputSystem;

public class GlidingController : MonoBehaviour
{
    [Header("Movement Data")]
    [SerializeField] private PlayerMovementData movementData;
    
    [Header("Visual")]
    [SerializeField] private GameObject glideSquare;
    
    private PlayerInputActions playerInput;
    private bool isGliding = false;
    private CharacterController controller;
    private Transform cameraTransform;
    private Transform playerModel;
    private Vector3 currentGlideDirection;
    private float currentTilt = 0f;
    
    private ClimbingController climbingController;
    private SwimmingController swimmingController;
    
    private float jumpButtonHeldTime = 0f;
    private bool wasJumpPressed = false;
    
    // Helper properties
    private float GlideSpeed
    {
        get
        {
            float baseSpeed = movementData != null ? movementData.glideSpeed : 8f;
            // Apply Wings multiplier if player has Wings in inventory
            if (ItemEffectManager.Instance != null)
                baseSpeed *= ItemEffectManager.Instance.GlideMultiplier;
            return baseSpeed;
        }
    }
    private float GlideFallSpeed => movementData != null ? movementData.glideFallSpeed : 2f;
    private float GlideRotationSpeed => movementData != null ? movementData.glideRotationSpeed : 5f;
    private float MaxTiltAngle => movementData != null ? movementData.maxTiltAngle : 30f;
    private float TiltSpeed => movementData != null ? movementData.tiltSpeed : 6f;
    private float MinHoldTimeToGlide => movementData != null ? movementData.minHoldTimeToGlide : 0.2f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        climbingController = GetComponent<ClimbingController>();
        swimmingController = GetComponent<SwimmingController>();
        
        playerModel = transform.Find("Player");
        if (playerModel == null && transform.childCount > 0)
        {
            playerModel = transform.GetChild(0);
        }
        
        if (glideSquare != null)
        {
            glideSquare.SetActive(false);
        }
        
        playerInput = new PlayerInputActions();
        playerInput.Player.Enable();
    }
    
    void Update()
    {
        bool canGlide = true;
        
        if (climbingController != null && climbingController.IsClimbing())
            canGlide = false;
        
        if (swimmingController != null && swimmingController.IsInWater())
            canGlide = false;
        
        if (controller.isGrounded)
            canGlide = false;
        
        bool jumpPressed = playerInput.Player.Jump.IsPressed();
        
        if (jumpPressed)
        {
            if (!wasJumpPressed)
            {
                jumpButtonHeldTime = 0f;
                wasJumpPressed = true;
            }
            else
            {
                jumpButtonHeldTime += Time.deltaTime;
            }
        }
        else
        {
            wasJumpPressed = false;
            jumpButtonHeldTime = 0f;
        }
        
        bool shouldGlide = jumpPressed && 
                          jumpButtonHeldTime >= MinHoldTimeToGlide && 
                          canGlide;
        
        if (shouldGlide)
        {
            if (!isGliding)
                StartGliding();
            HandleGliding();
        }
        else if (isGliding)
        {
            StopGliding();
        }
    }
    
    void HandleGliding()
    {
        Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 targetDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        
        if (targetDirection.magnitude > 0.1f)
        {
            currentGlideDirection = Vector3.Slerp(currentGlideDirection, targetDirection, GlideRotationSpeed * Time.deltaTime);
            
            Vector3 playerRight = transform.right;
            float turnDirection = Vector3.Dot(targetDirection - currentGlideDirection, playerRight);
            float targetTilt = turnDirection * MaxTiltAngle * 10f;
            targetTilt = Mathf.Clamp(targetTilt, -MaxTiltAngle, MaxTiltAngle);
            targetTilt = -targetTilt;
            
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, TiltSpeed * Time.deltaTime);
        }
        else
        {
            currentTilt = Mathf.Lerp(currentTilt, 0f, TiltSpeed * Time.deltaTime);
        }
        
        if (playerModel != null)
        {
            playerModel.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
        }
        
        if (currentGlideDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentGlideDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, GlideRotationSpeed * Time.deltaTime);
        }
        
        Vector3 movement = currentGlideDirection * GlideSpeed * Time.deltaTime;
        movement.y = -GlideFallSpeed * Time.deltaTime;
        
        controller.Move(movement);
    }
    
    void StartGliding()
    {
        isGliding = true;
        
        currentGlideDirection = transform.forward;
        currentGlideDirection.y = 0f;
        currentGlideDirection.Normalize();
        
        if (glideSquare != null)
            glideSquare.SetActive(true);
        
        Debug.Log("Started gliding!" + (ItemEffectManager.Instance != null && ItemEffectManager.Instance.HasWings ? " (Wings boost active!)" : ""));
    }
    
    void StopGliding()
    {
        isGliding = false;
        
        currentTilt = 0f;
        if (playerModel != null)
            playerModel.localRotation = Quaternion.identity;
        
        if (glideSquare != null)
            glideSquare.SetActive(false);
        
        Debug.Log("Stopped gliding!");
    }
    
    public bool IsGliding()
    {
        return isGliding;
    }
    
    void OnDestroy()
    {
        if (playerInput != null)
            playerInput.Player.Disable();
    }
}
