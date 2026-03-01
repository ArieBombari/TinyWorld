using UnityEngine;

/// <summary>
/// Place on a trigger volume at the mountain top / cold area.
/// Without the Hat, feathers drain rapidly (frozen).
/// With the Hat, feathers behave normally.
///
/// Setup:
/// 1. Create trigger volume covering the cold area
/// 2. Add this script
/// 3. Optionally add particle effects (snow) as children
/// </summary>
public class ColdZone : MonoBehaviour
{
    [Header("Cold Settings")]
    [Tooltip("Feathers lost per second when in cold without Hat")]
    [SerializeField] private float featherDrainRate = 1f;

    [Tooltip("Block feather recharge while in cold without Hat")]
    [SerializeField] private bool blockRechargeWithoutHat = true;

    [Header("Visual Feedback")]
    [Tooltip("Optional frost overlay (CanvasGroup on a white/blue UI image)")]
    [SerializeField] private CanvasGroup frostOverlay;
    [SerializeField] private float maxFrostAlpha = 0.4f;
    [SerializeField] private float frostFadeSpeed = 2f;

    private bool playerInCold = false;
    private float drainAccumulator = 0f;
    private float targetFrost = 0f;

    void Update()
    {
        if (playerInCold)
        {
            bool isProtected = ItemEffectManager.Instance != null && ItemEffectManager.Instance.IsProtectedFromCold();

            if (!isProtected)
            {
                // Drain feathers
                drainAccumulator += Time.deltaTime;
                if (drainAccumulator >= 1f / featherDrainRate)
                {
                    if (FeatherManager.Instance != null && FeatherManager.Instance.GetCurrentFeathers() > 0)
                    {
                        FeatherManager.Instance.UseFeather();
                        Debug.Log("[ColdZone] Feather frozen! Remaining: " + FeatherManager.Instance.GetCurrentFeathers());
                    }
                    drainAccumulator = 0f;
                }

                // Block recharge
                if (blockRechargeWithoutHat && FeatherManager.Instance != null)
                {
                    FeatherManager.Instance.BlockRecharge();
                }

                targetFrost = maxFrostAlpha;
            }
            else
            {
                // Hat protects — allow normal recharge
                if (FeatherManager.Instance != null)
                {
                    FeatherManager.Instance.AllowRecharge();
                }
                targetFrost = 0.05f; // Tiny hint of frost for atmosphere
            }
        }
        else
        {
            targetFrost = 0f;
        }

        // Fade frost overlay
        if (frostOverlay != null)
        {
            frostOverlay.alpha = Mathf.MoveTowards(frostOverlay.alpha, targetFrost, frostFadeSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInCold = true;
        drainAccumulator = 0f;
        Debug.Log("[ColdZone] Player entered cold area.");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInCold = false;

        // Restore recharge when leaving
        if (FeatherManager.Instance != null)
        {
            FeatherManager.Instance.AllowRecharge();
        }
        Debug.Log("[ColdZone] Player left cold area.");
    }
}
