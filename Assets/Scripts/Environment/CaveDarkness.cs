using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place on a trigger volume at the cave entrance.
/// When the player enters, the screen goes dark unless the Lantern is active.
/// With lantern: Diablo 2 style - pitch black with a circular light radius around the player.
/// Without lantern: tiny radius with near-full darkness so player is barely visible.
///
/// Setup:
/// 1. Create trigger volume at cave entrance
/// 2. Add this script
/// 3. Create a UI Image (Canvas → Image) covering the full screen
/// 4. Create Material using "UI/CaveDarkness" shader, assign to Image
/// 5. Assign the Image to darknessImage
/// 6. Assign player's LanternLight to lanternLight
/// </summary>
public class CaveDarkness : MonoBehaviour
{
    [Header("Darkness Settings")]
    [SerializeField] private Image darknessImage;
    [SerializeField] private float maxDarkness = 1f;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("Lantern Light Radius")]
    [Tooltip("The player's lantern point light")]
    [SerializeField] private Light lanternLight;
    [Tooltip("Radius of the light circle on screen (0-1, where 1 = full screen)")]
    [SerializeField] private float lanternRadius = 0.15f;
    [Tooltip("Softness of the light circle edge")]
    [SerializeField] private float lanternSoftness = 0.1f;

    [Header("Without Lantern")]
    [Tooltip("Small radius visible even without lantern so player isn't fully blind")]
    [SerializeField] private float noLanternRadius = 0.05f;
    [Tooltip("Darkness inside the no-lantern radius (0.95 = barely visible)")]
    [SerializeField] private float noLanternDarkness = 0.95f;
    [SerializeField] private float noLanternSoftness = 0.05f;

    [Header("Hide Geometry")]
    [SerializeField] private GameObject[] objectsToHide;

    [Header("Ambient Light")]
    [Tooltip("Reduce ambient light intensity inside cave")]
    [SerializeField] private float caveAmbientIntensity = 0.1f;
    private float originalAmbientIntensity;

    private bool playerInCave = false;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private Material darknessMaterial;
    private Camera mainCamera;
    private Transform playerTransform;

    void Start()
    {
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        mainCamera = Camera.main;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Get material instance so we don't modify the shared one
        if (darknessImage != null)
        {
            darknessMaterial = darknessImage.material;

            if (darknessMaterial != null && darknessMaterial.HasProperty("_Darkness"))
            {
                darknessMaterial.SetFloat("_Darkness", 0f);
                darknessMaterial.SetFloat("_LanternActive", 0f);
                darknessMaterial.SetFloat("_LanternRadius", lanternRadius);
                darknessMaterial.SetFloat("_LanternSoftness", lanternSoftness);
                darknessMaterial.SetFloat("_CircleMinAlpha", 0f);
            }
            else
            {
                // Fallback: no shader material, use color alpha
                Color c = darknessImage.color;
                c.a = 0f;
                darknessImage.color = c;
            }
        }
    }

    void Update()
    {
        bool hasLantern = ItemEffectManager.Instance != null && ItemEffectManager.Instance.IsLanternActive;

        if (!playerInCave)
        {
            targetAlpha = 0f;
        }
        else
        {
            targetAlpha = maxDarkness;
        }

        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        if (darknessImage != null && darknessMaterial != null && darknessMaterial.HasProperty("_Darkness"))
        {
            darknessMaterial.SetFloat("_Darkness", currentAlpha);

            if (playerInCave && playerTransform != null && mainCamera != null)
                {
                    // Only show the circle once darkness has fully faded in
                    bool fadeComplete = currentAlpha >= targetAlpha - 0.05f;

                    if (fadeComplete)
                    {
                    Vector3 screenPos = mainCamera.WorldToViewportPoint(playerTransform.position);
                    darknessMaterial.SetVector("_LanternScreenPos", new Vector4(screenPos.x, screenPos.y, 0, 0));

                    if (hasLantern)
                    {
                    darknessMaterial.SetFloat("_LanternActive", 1f);
                    darknessMaterial.SetFloat("_LanternRadius", lanternRadius);
                    darknessMaterial.SetFloat("_LanternSoftness", lanternSoftness);
                    darknessMaterial.SetFloat("_CircleMinAlpha", 0f);
                    }
                    else
                    {
                    darknessMaterial.SetFloat("_LanternActive", 1f);
                    darknessMaterial.SetFloat("_LanternRadius", noLanternRadius);
                    darknessMaterial.SetFloat("_LanternSoftness", noLanternSoftness);
                    darknessMaterial.SetFloat("_CircleMinAlpha", noLanternDarkness);
                    }
                    }  
                    else
                    {
                    darknessMaterial.SetFloat("_LanternActive", 0f);
                    }
                }
        }
        else if (darknessImage != null)
        {
            // Fallback: simple color alpha (no shader)
            Color c = darknessImage.color;
            c.a = currentAlpha;
            darknessImage.color = c;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInCave = true;
        RenderSettings.ambientIntensity = caveAmbientIntensity;

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

        if (darknessMaterial != null && darknessMaterial.HasProperty("_LanternActive"))
        {
        darknessMaterial.SetFloat("_LanternActive", 0f);
        }

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

    void OnDestroy()
    {
        if (darknessMaterial != null && darknessMaterial.HasProperty("_Darkness"))
        {
            darknessMaterial.SetFloat("_Darkness", 0f);
            darknessMaterial.SetFloat("_LanternActive", 0f);
            darknessMaterial.SetFloat("_CircleMinAlpha", 0f);
        }
    }
}
