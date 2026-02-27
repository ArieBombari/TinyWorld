using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbingController : MonoBehaviour
{
    [Header("Climb Settings")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private LayerMask climbableMask;
    [SerializeField] private float climbCheckDistance = 0.6f;
    [SerializeField] private float ledgeCheckHeight = 2f;
    [SerializeField] private float ledgeClimbSpeed = 5f;
    
    [Header("Stamina/Feather System")]
    [SerializeField] private bool requireFeathersToClimb = true;
    [SerializeField] private float featherDrainRate = 2f; // Drain 1 feather every 2 seconds
    [SerializeField] private float minClimbTimeWithoutFeathers = 1f; // Can climb 1s with 0 feathers
    
    [Header("Detection")]
    [SerializeField] private Transform climbCheck;
    
    private bool isClimbing = false;
    private bool isClimbingOverLedge = false;
    private bool canStartClimbing = false;
    private CharacterController controller;
    private PlayerInputActions playerInput;
    private Vector3 climbDirection;
    private Vector3 ledgeTopPosition;
    private float ledgeClimbProgress = 0f;
    private Vector3 ledgeStartPosition;
    
    private float climbTimeAccumulator = 0f;
    private float timeWithoutFeathers = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        playerInput = new PlayerInputActions();
        playerInput.Player.Enable();
    }
    
    void Update()
    {
        if (isClimbingOverLedge)
        {
            HandleLedgeClimb();
            return;
        }
        
        if (isClimbing)
        {
            HandleClimbing();
            HandleFeatherConsumption();
        }
        else
        {
            CheckForClimbableSurface();
            
            // DEBUG: Check if E is pressed
            if (playerInput.Player.Interact.WasPressedThisFrame())
            {
                Debug.Log($"E pressed! canStartClimbing: {canStartClimbing}");
            }
            
            if (canStartClimbing && playerInput.Player.Interact.WasPressedThisFrame())
            {
                if (CanStartClimbing())
                {
                    StartClimbing();
                }
                else
                {
                    Debug.Log("Not enough stamina to climb!");
                }
            }
        }
    }
    
    bool CanStartClimbing()
    {
        if (!requireFeathersToClimb)
        {
            return true;
        }
        
        if (FeatherManager.Instance != null)
        {
            return FeatherManager.Instance.GetCurrentFeathers() > 0 || minClimbTimeWithoutFeathers > 0f;
        }
        
        return minClimbTimeWithoutFeathers > 0f;
    }
    
    void HandleFeatherConsumption()
    {
        if (!requireFeathersToClimb || FeatherManager.Instance == null)
        {
            return;
        }
        
        climbTimeAccumulator += Time.deltaTime;
        
        int currentFeathers = FeatherManager.Instance.GetCurrentFeathers();
        
        if (currentFeathers > 0)
        {
            timeWithoutFeathers = 0f;
            
            if (climbTimeAccumulator >= featherDrainRate)
            {
                // Consume 1 feather - THIS UPDATES THE UI!
                FeatherManager.Instance.UseFeather();
                climbTimeAccumulator = 0f;
                
                Debug.Log($"Used feather while climbing. Remaining: {FeatherManager.Instance.GetCurrentFeathers()}");
            }
        }
        else
        {
            // No feathers - limited time left
            timeWithoutFeathers += Time.deltaTime;
            
            if (timeWithoutFeathers >= minClimbTimeWithoutFeathers)
            {
                Debug.Log("Out of stamina! Falling off wall.");
                StopClimbing();
                
                Vector3 pushAway = -climbDirection * 2f;
                pushAway.y = -5f;
                controller.Move(pushAway * Time.deltaTime);
            }
        }
    }
    
    void CheckForClimbableSurface()
    {
        Vector3 checkPosition = climbCheck != null ? climbCheck.position : transform.position;
        
        RaycastHit hit;
        if (Physics.Raycast(checkPosition, transform.forward, out hit, climbCheckDistance, climbableMask))
        {
            canStartClimbing = true;
            climbDirection = -hit.normal;
            
            Debug.DrawRay(checkPosition, transform.forward * climbCheckDistance, Color.green);
            
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"Can start climbing! Press E. Hit object: {hit.collider.gameObject.name}");
            }
        }
        else
        {
            canStartClimbing = false;
            Debug.DrawRay(checkPosition, transform.forward * climbCheckDistance, Color.red);
        }
    }
    
    void StartClimbing()
    {
    isClimbing = true;
    canStartClimbing = false;
    climbTimeAccumulator = 0f;
    timeWithoutFeathers = 0f;
    
    if (FeatherManager.Instance != null)
    {
        FeatherManager.Instance.BlockRecharge();
    }
    
    // NEW: Rotate player to face the wall
    if (climbDirection != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(climbDirection);
        transform.rotation = targetRotation;
    }
    
    Debug.Log("Started climbing!");
    
    if (AudioManager.Instance != null)
    {
        // Play climb start sound
    }
    }
    
    void HandleClimbing()
    {
        Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        
        Vector3 checkPosition = climbCheck != null ? climbCheck.position : transform.position;
        RaycastHit hit;
        bool stillTouchingWall = Physics.Raycast(checkPosition, transform.forward, out hit, climbCheckDistance, climbableMask);
        
        if (!stillTouchingWall)
        {
            if (CheckForLedge())
            {
                StartLedgeClimb();
                return;
            }
            else
            {
                StopClimbing();
                return;
            }
        }
        
        Vector3 climbMovement = Vector3.zero;
        climbMovement.y = moveInput.y * climbSpeed * Time.deltaTime;
        
        Vector3 right = Vector3.Cross(Vector3.up, climbDirection).normalized;
        climbMovement += right * moveInput.x * (climbSpeed * 0.5f) * Time.deltaTime;
        
        controller.Move(climbMovement);
        
        if (playerInput.Player.Jump.WasPressedThisFrame())
        {
            StopClimbing();
            
            Vector3 pushAway = -climbDirection * 3f;
            pushAway.y = 5f;
            controller.Move(pushAway * Time.deltaTime);
        }
    }
    
    bool CheckForLedge()
    {
        Vector3 headPosition = transform.position + Vector3.up * ledgeCheckHeight;
        Vector3 checkPosition = headPosition + transform.forward * (climbCheckDistance + 0.5f);
        
        RaycastHit hit;
        if (Physics.Raycast(checkPosition, Vector3.down, out hit, ledgeCheckHeight + 1f, climbableMask))
        {
            ledgeTopPosition = hit.point + Vector3.up * 0.1f;
            ledgeTopPosition += -climbDirection * 0.5f;
            
            Debug.Log("Found ledge at top!");
            return true;
        }
        
        return false;
    }
    
    void StartLedgeClimb()
    {
        isClimbingOverLedge = true;
        isClimbing = false;
        ledgeClimbProgress = 0f;
        ledgeStartPosition = transform.position;
        
        Debug.Log("Climbing over ledge!");
    }
    
    void HandleLedgeClimb()
    {
        ledgeClimbProgress += Time.deltaTime * ledgeClimbSpeed;
        
        if (ledgeClimbProgress >= 1f)
        {
            transform.position = ledgeTopPosition;
            isClimbingOverLedge = false;
            
            if (FeatherManager.Instance != null)
            {
                FeatherManager.Instance.AllowRecharge();
            }
            
            Debug.Log("Finished ledge climb!");
            
            return;
        }
        
        float t = Mathf.SmoothStep(0f, 1f, ledgeClimbProgress);
        Vector3 newPosition = Vector3.Lerp(ledgeStartPosition, ledgeTopPosition, t);
        
        Vector3 movement = newPosition - transform.position;
        controller.Move(movement);
    }
    
    void StopClimbing()
    {
        isClimbing = false;
        climbTimeAccumulator = 0f;
        timeWithoutFeathers = 0f;
        
        if (FeatherManager.Instance != null)
        {
            FeatherManager.Instance.AllowRecharge();
        }
        
        Debug.Log("Stopped climbing!");
    }
    
    public bool IsClimbing()
    {
        return isClimbing || isClimbingOverLedge;
    }
    
    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.Player.Disable();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (climbCheck != null)
        {
            Gizmos.color = canStartClimbing ? Color.green : Color.red;
            Gizmos.DrawWireSphere(climbCheck.position, climbCheckDistance);
            
            if (isClimbing)
            {
                Gizmos.color = Color.yellow;
                Vector3 headPos = transform.position + Vector3.up * ledgeCheckHeight;
                Gizmos.DrawWireSphere(headPos, 0.3f);
            }
        }
    }
}