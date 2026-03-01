using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Place on a trigger volume at the cave entrance.
/// When the player enters, the screen goes dark unless the Lantern is active.
/// Uses a full-screen UI overlay for the darkness effect.
///
/// Setup:
/// 1. Create trigger volume at cave entrance
/// 2. Add this script
/// 3. Create a UI Image (Canvas → Image) covering the full screen, black color
///    Assign it to darknessOverlay
/// 4. The overlay starts fully transparent
/// </summary>
public class CaveDarkness : MonoBehaviour
{
    [Header("Darkness Settings")]
    [SerializeField] private CanvasGroup darknessOverlay;
    [SerializeField] private float maxDarkness = 0.95f;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("With Lantern")]
    [Tooltip("Darkness level even with lantern — slight ambient darkness for mood")]
    [SerializeField] private float lanternDarkness = 0.3f;

    [Header("Hide Geometry")]
    [SerializeField] private GameObject[] objectsToHide;

    [Header("Ambient Light")]
    [Tooltip("Reduce ambient light intensity inside cave")]
    [SerializeField] private float caveAmbientIntensity = 0.1f;
    private float originalAmbientIntensity;

    private bool playerInCave = false;
    private float targetAlpha = 0f;

    void Start()
    {
        originalAmbientIntensity = RenderSettings.ambientIntensity;

        if (darknessOverlay != null)
        {
            darknessOverlay.alpha = 0f;
            darknessOverlay.blocksRaycasts = false;
            darknessOverlay.interactable = false;
        }
    }

    void Update()
    {
        if (!playerInCave)
        {
            targetAlpha = 0f;
        }
        else
        {
            bool hasLantern = ItemEffectManager.Instance != null && ItemEffectManager.Instance.IsLanternActive;
            targetAlpha = hasLantern ? lanternDarkness : maxDarkness;
        }

        if (darknessOverlay != null)
        {
            darknessOverlay.alpha = Mathf.MoveTowards(darknessOverlay.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
    if (!other.CompareTag("Player")) return;

    playerInCave = true;
    RenderSettings.ambientIntensity = caveAmbientIntensity;

    // Hide walls/roof
    foreach (var obj in objectsToHide)
    {
        if (obj != null)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }

    Debug.Log("[Cave] Player entered dark cave.");
    }

    void OnTriggerExit(Collider other)
    {
    if (!other.CompareTag("Player")) return;

    playerInCave = false;
    RenderSettings.ambientIntensity = originalAmbientIntensity;

    // Show walls/roof again
    foreach (var obj in objectsToHide)
    {
        if (obj != null)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = true;
        }
    }

    Debug.Log("[Cave] Player left cave.");
    }
}
