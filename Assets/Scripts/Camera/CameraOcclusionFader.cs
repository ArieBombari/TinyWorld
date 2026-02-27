// ============================================================================
// CameraOcclusionFader.cs — Fades objects blocking the camera's view of player
// ============================================================================
// Attach to: Main Camera (or any always-active object)
//
// How it works:
//   Raycasts from camera to player every frame. Any object hit that isn't
//   the player gets faded to transparent. When no longer blocking, it
//   fades back. Works with URP Lit/Simple Lit materials.
//
// Setup:
//   1. Put blocking objects (buildings, trees, pillars) on a specific layer
//      e.g. "Occludable" (or use Default — just set the Layer Mask)
//   2. Those objects need colliders (they probably already have them)
//   3. Materials must use URP Lit or Simple Lit shader
// ============================================================================

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class CameraOcclusionFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Detection")]
    [Tooltip("Which layers can block the player")]
    [SerializeField] private LayerMask occludableLayers = ~0;

    [Tooltip("Offset from player's feet to target (usually mid-body)")]
    [SerializeField] private float playerHeightOffset = 1.0f;

    [Tooltip("How wide the detection area is (uses SphereCast radius)")]
    [SerializeField] private float detectionRadius = 0.5f;

    [Header("Fading")]
    [Tooltip("How transparent blocking objects become (0 = invisible, 1 = opaque)")]
    [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.2f;

    [Tooltip("How fast objects fade in/out")]
    [SerializeField] private float fadeSpeed = 5f;

    // Track faded objects
    private Dictionary<Renderer, FadeData> fadedObjects = new Dictionary<Renderer, FadeData>();
    private List<Renderer> currentBlockers = new List<Renderer>();
    private List<Renderer> toRestore = new List<Renderer>();
    private RaycastHit[] hitBuffer = new RaycastHit[20];

    private class FadeData
    {
        public float currentAlpha;
        public float targetAlpha;
        public Material[] originalMaterials; // store originals for cleanup
        public Material[] fadeMaterials;     // cloned instances we modify
        public bool wasTransparent;
        public ShadowCastingMode originalShadowMode;
    }

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null || cam == null) return;

        currentBlockers.Clear();
        DetectBlockers();
        UpdateFading();
    }

    private void DetectBlockers()
    {
        Vector3 playerTarget = player.position + Vector3.up * playerHeightOffset;
        Vector3 camPos = cam.transform.position;
        Vector3 direction = playerTarget - camPos;
        float distance = direction.magnitude;

        int hitCount = Physics.SphereCastNonAlloc(
            camPos,
            detectionRadius,
            direction.normalized,
            hitBuffer,
            distance,
            occludableLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit.transform == player) continue;
            if (hit.transform.IsChildOf(player)) continue;

            // Get all renderers on this object (and children for compound objects)
            var renderers = hit.transform.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (rend is ParticleSystemRenderer) continue;
                if (!currentBlockers.Contains(rend))
                    currentBlockers.Add(rend);
            }
        }
    }

    private void UpdateFading()
    {
        // Mark blockers to fade
        foreach (var rend in currentBlockers)
        {
            if (!fadedObjects.ContainsKey(rend))
            {
                var data = CreateFadeData(rend);
                if (data != null)
                    fadedObjects[rend] = data;
            }

            if (fadedObjects.ContainsKey(rend))
                fadedObjects[rend].targetAlpha = fadedAlpha;
        }

        // Mark non-blockers to restore
        toRestore.Clear();
        foreach (var kvp in fadedObjects)
        {
            if (!currentBlockers.Contains(kvp.Key))
                kvp.Value.targetAlpha = 1f;
        }

        // Animate all
        foreach (var kvp in fadedObjects)
        {
            var rend = kvp.Key;
            var data = kvp.Value;

            if (rend == null)
            {
                toRestore.Add(rend);
                continue;
            }

            data.currentAlpha = Mathf.MoveTowards(data.currentAlpha, data.targetAlpha, fadeSpeed * Time.deltaTime);

            // Apply alpha to all fade materials
            foreach (var mat in data.fadeMaterials)
            {
                Color c = mat.color;
                c.a = data.currentAlpha;
                mat.color = c;
            }

            // Switch surface type based on alpha
            if (data.currentAlpha < 0.99f)
            {
                SetMaterialsTransparent(data.fadeMaterials);
                rend.shadowCastingMode = ShadowCastingMode.Off;
            }
            else
            {
                if (!data.wasTransparent)
                {
                    SetMaterialsOpaque(data.fadeMaterials);
                    rend.shadowCastingMode = data.originalShadowMode;
                }
            }

            // Fully restored — clean up
            if (data.currentAlpha >= 1f && data.targetAlpha >= 1f)
                toRestore.Add(rend);
        }

        // Remove fully restored objects
        foreach (var rend in toRestore)
        {
            if (rend != null && fadedObjects.ContainsKey(rend))
            {
                var data = fadedObjects[rend];
                // Restore original materials
                if (!data.wasTransparent)
                    rend.materials = data.originalMaterials;
                rend.shadowCastingMode = data.originalShadowMode;
            }
            fadedObjects.Remove(rend);
        }
    }

    private FadeData CreateFadeData(Renderer rend)
    {
        if (rend == null) return null;

        var originalMats = rend.sharedMaterials;
        if (originalMats == null || originalMats.Length == 0) return null;

        // Clone materials so we don't modify shared ones
        var fadeMats = new Material[originalMats.Length];
        for (int i = 0; i < originalMats.Length; i++)
        {
            if (originalMats[i] == null) continue;
            fadeMats[i] = new Material(originalMats[i]);
        }

        rend.materials = fadeMats;

        bool wasTransparent = false;
        if (originalMats[0] != null && originalMats[0].HasProperty("_Surface"))
            wasTransparent = originalMats[0].GetFloat("_Surface") > 0.5f;

        return new FadeData
        {
            currentAlpha = 1f,
            targetAlpha = fadedAlpha,
            originalMaterials = originalMats,
            fadeMaterials = fadeMats,
            wasTransparent = wasTransparent,
            originalShadowMode = rend.shadowCastingMode
        };
    }

    private void SetMaterialsTransparent(Material[] mats)
    {
        foreach (var mat in mats)
        {
            if (mat == null) continue;

            // URP surface type: 0 = Opaque, 1 = Transparent
            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 1);

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)RenderQueue.Transparent;

            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }

    private void SetMaterialsOpaque(Material[] mats)
    {
        foreach (var mat in mats)
        {
            if (mat == null) continue;

            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 0);

            mat.SetOverrideTag("RenderType", "Opaque");
            mat.SetInt("_SrcBlend", (int)BlendMode.One);
            mat.SetInt("_DstBlend", (int)BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.renderQueue = (int)RenderQueue.Geometry;

            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
    }

    private void OnDisable()
    {
        // Restore all materials on disable
        foreach (var kvp in fadedObjects)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value.originalMaterials;
                kvp.Key.shadowCastingMode = kvp.Value.originalShadowMode;
            }
        }
        fadedObjects.Clear();
    }
}
