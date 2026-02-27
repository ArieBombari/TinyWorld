using UnityEngine;

public class SwimmingController : MonoBehaviour
{
    [Header("Movement Data")]
    [SerializeField] private PlayerMovementData movementData;
    
    private bool isSwimming = false;
    private bool isInWater = false;
    private float waterSurfaceY = 0f;
    private int waterContactCount = 0;
    private float jumpExitTime = 0f;
    private float jumpExitDuration = 0.5f;
    
    private PlayerController playerController;
    private CharacterController controller;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
        
        isSwimming = false;
        isInWater = false;
        waterContactCount = 0;
        
        Debug.Log("SwimmingController: Initialized");
    }
    
    void OnTriggerEnter(Collider other)
    {
    Debug.Log($"[SWIM] ===== TRIGGER ENTER =====");
    Debug.Log($"[SWIM] Object name: {other.gameObject.name}");
    Debug.Log($"[SWIM] Object layer: {other.gameObject.layer}");
    Debug.Log($"[SWIM] Layer name: {LayerMask.LayerToName(other.gameObject.layer)}");
    Debug.Log($"[SWIM] Water layer number: {LayerMask.NameToLayer("Water")}");
    Debug.Log($"[SWIM] Comparison: {other.gameObject.layer} == {LayerMask.NameToLayer("Water")}?");
    
    if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
    {
        waterContactCount++;
        Debug.Log($"[SWIM] ✅ MATCH! Entered water. Count: {waterContactCount}");
        EnterWater(other);
    }
    else
    {
        Debug.Log($"[SWIM] ❌ NO MATCH! Expected layer {LayerMask.NameToLayer("Water")}, got {other.gameObject.layer}");
    }
    Debug.Log($"[SWIM] ===========================");
    }
    
    void OnTriggerStay(Collider other)
    {
    if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
    {
        isInWater = true;
        waterSurfaceY = other.bounds.max.y;
        
        // CHANGED: Use player's FEET position, not center
        float playerFeetY = transform.position.y - (controller.height * 0.5f);
        
        bool recentlyJumped = (Time.time - jumpExitTime) < jumpExitDuration;
        
        // CHANGED: Check if feet are below water surface
        if (playerFeetY < waterSurfaceY && !recentlyJumped)
        {
            isSwimming = true;
            
            if (playerController != null)
            {
                playerController.RestoreJumpsBasedOnFeathers();
            }
        }
        else
        {
            isSwimming = false;
        }
    }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            waterContactCount--;
            Debug.Log($"Exited water trigger! Count: {waterContactCount}");
            
            if (waterContactCount <= 0)
            {
                waterContactCount = 0;
                ExitWater();
            }
        }
    }
    
    void Update()
    {
        if (waterContactCount <= 0)
        {
            isSwimming = false;
            isInWater = false;
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"Status - InWater: {isInWater}, Swimming: {isSwimming}, Contact: {waterContactCount}");
            
            if (FeatherManager.Instance != null)
            {
                Debug.Log($"Feathers: {FeatherManager.Instance.GetCurrentFeathers()}/{FeatherManager.Instance.GetFeathersCollected()}");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            FeatherManager[] managers = FindObjectsOfType<FeatherManager>();
            Debug.Log($"FeatherManager instances: {managers.Length}");
        }
    }
    
    void EnterWater(Collider waterCollider)
    {
        Debug.Log("Player entered water! Starting feather recharge...");
        isInWater = true;
        waterSurfaceY = waterCollider.bounds.max.y;
        
        if (FeatherManager.Instance != null)
        {
            FeatherManager.Instance.AllowRecharge();
        }
    }
    
    void ExitWater()
    {
        Debug.Log("Player exited water!");
        isInWater = false;
        isSwimming = false;
    }
    
    public void OnJumpFromWater()
    {
        jumpExitTime = Time.time;
        isSwimming = false;
        Debug.Log("Jumped from water - blocking recharge until land");
    }
    
    public bool IsSwimming() { return isSwimming && waterContactCount > 0; }
    public bool IsInWater() { return isInWater && waterContactCount > 0; }
    public float GetWaterSurfaceY() { return waterSurfaceY; }
    
    public float GetSwimSpeed() 
    { 
        return movementData != null ? movementData.swimSpeed : 3f; 
    }
    
    public float GetSwimSprintSpeed() 
    { 
        return movementData != null ? movementData.swimSprintSpeed : 5f; 
    }
    
    public float GetSubmersionDepth() 
    { 
        return movementData != null ? movementData.submersionDepth : 0.8f; 
    }
}