using UnityEngine;
using System.Collections;

public class FeatherManager : MonoBehaviour
{
    public static FeatherManager Instance;
    
    [Header("Data")]
    [SerializeField] private FeatherData featherData;
    
    [Header("Fallback Settings (used if data not assigned)")]
    [SerializeField] private int maxFeathersInWorld = 15;
    [SerializeField] private float rechargeDelay = 0.5f;
    [SerializeField] private float rechargeRate = 0.5f;
    [SerializeField] private bool unlimitedFeathers = false;
    [SerializeField] private bool instantRecharge = false;
    
    private int feathersCollected = 0;
    private int currentFeathers = 0;
    private float rechargeTimer = 0f;
    private bool isRecharging = false;
    private bool rechargeBlocked = false;
    private bool isWaitingToRecharge = false; // ADD THIS FLAG

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (FeatherUI.Instance != null)
        {
            FeatherUI.Instance.UpdateFeatherCount(currentFeathers);
        }
    }

    void Update()
    {
        HandleRecharge();
    }

    void HandleRecharge()
    {
    float actualRechargeRate = featherData != null ? featherData.rechargeRate : rechargeRate;
    bool actualInstantRecharge = featherData != null ? featherData.instantRecharge : instantRecharge;
    
    // DEBUG: Log recharge status every 60 frames (~1 second)
    if (Time.frameCount % 60 == 0)
    {
        Debug.Log($"HandleRecharge Status - Blocked: {rechargeBlocked}, Current: {currentFeathers}, Max: {feathersCollected}, Timer: {rechargeTimer:F2}, Rate: {actualRechargeRate}");
    }
    
    if (rechargeBlocked || currentFeathers >= feathersCollected)
    {
        isRecharging = false;
        rechargeTimer = 0f;
        return;
    }

    if (actualInstantRecharge)
    {
        currentFeathers = feathersCollected;
        if (FeatherUI.Instance != null)
        {
            FeatherUI.Instance.UpdateFeatherCount(currentFeathers);
        }
        
        RestoreJumpsToPlayer();
        return;
    }

    if (currentFeathers < feathersCollected)
    {
        isRecharging = true;
        rechargeTimer += Time.deltaTime;

        if (rechargeTimer >= actualRechargeRate)
        {
            currentFeathers++;
            rechargeTimer = 0f;
            
            if (FeatherUI.Instance != null)
            {
                FeatherUI.Instance.UpdateFeatherCount(currentFeathers);
            }
            
            RestoreJumpsToPlayer();
            
            Debug.Log("Feather recharged! Current: " + currentFeathers + "/" + feathersCollected);
        }
    }
    else
    {
        isRecharging = false;
        rechargeTimer = 0f;
    }
    }

    void RestoreJumpsToPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.RestoreJumpsBasedOnFeathers();
        }
    }

    public void CollectFeather()
    {
        int actualMaxFeathers = featherData != null ? featherData.maxFeathersInWorld : maxFeathersInWorld;
        
        if (feathersCollected >= actualMaxFeathers)
        {
            Debug.Log("Already collected all feathers!");
            return;
        }
        
        feathersCollected++;
        currentFeathers++;
        
        if (FeatherUI.Instance != null)
        {
            FeatherUI.Instance.UpdateFeatherCount(currentFeathers);
        }
        
        Debug.Log("Feather collected! Total: " + feathersCollected);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFeatherCollect();
        }
    }

    public void UseFeather()
    {
    bool actualUnlimited = featherData != null ? featherData.unlimitedFeathers : unlimitedFeathers;
    
    if (actualUnlimited)
    {
        return;
    }
    
    if (currentFeathers > 0)
    {
        currentFeathers--;
        
        // DEBUG: Show what's calling UseFeather
        Debug.Log($"Feather used! Current: {currentFeathers}/{feathersCollected} - Called from: {new System.Diagnostics.StackTrace()}");
        
        if (FeatherUI.Instance != null)
        {
            FeatherUI.Instance.UpdateFeatherCount(currentFeathers);
        }
    }
    }

    public int GetFeathersCollected()
    {
        return feathersCollected;
    }

    public int GetCurrentFeathers()
    {
        return currentFeathers;
    }

    public bool HasFeathers()
    {
        bool actualUnlimited = featherData != null ? featherData.unlimitedFeathers : unlimitedFeathers;
        
        if (actualUnlimited)
        {
            return true;
        }
        
        return currentFeathers > 0;
    }

    public void BlockRecharge()
    {
        rechargeBlocked = true;
        rechargeTimer = 0f;
        isWaitingToRecharge = false; // RESET FLAG
        Debug.Log("FeatherManager: Recharge BLOCKED");
    }

    public void AllowRecharge()
    {
        // CHANGED: Only start coroutine if not already waiting
        if (!isWaitingToRecharge)
        {
            StartCoroutine(StartRechargeAfterDelay());
        }
    }
    
    IEnumerator StartRechargeAfterDelay()
    {
        isWaitingToRecharge = true; // SET FLAG
        rechargeBlocked = true;
        
        float actualDelay = featherData != null ? featherData.rechargeDelay : rechargeDelay;
        
        Debug.Log($"FeatherManager: Waiting {actualDelay}s before allowing recharge...");
        yield return new WaitForSeconds(actualDelay);
        
        rechargeBlocked = false;
        isWaitingToRecharge = false; // CLEAR FLAG
        
        Debug.Log("FeatherManager: Recharge ALLOWED - feathers will now recharge");
    }
}