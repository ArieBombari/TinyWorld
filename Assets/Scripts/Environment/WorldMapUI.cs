using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Zelda-style world map that fills most of the screen.
/// Shows a map image with the player's current position and highlight markers.
///
/// Setup:
/// 1. Create a Canvas → Panel (dark semi-transparent background)
/// 2. Inside it: Image for the map, assign your hand-drawn map sprite
/// 3. Create small Image prefab for markers (player dot, highlight icons)
/// 4. Assign everything in Inspector
/// 5. The map converts world XZ positions to map UV coordinates
/// </summary>
public class WorldMapUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform mapImage;
    [SerializeField] private RectTransform playerMarker;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;

    [Header("World Bounds (XZ plane)")]
    [Tooltip("Bottom-left corner of the world in world coordinates")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [Tooltip("Top-right corner of the world in world coordinates")]
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Map Highlights")]
    [SerializeField] private List<MapHighlight> highlights = new List<MapHighlight>();
    [SerializeField] private GameObject highlightMarkerPrefab;

    [Header("Input")]
    [SerializeField] private InputActionReference closeMapAction;

    [Header("Animation")]
    [SerializeField] private float openSpeed = 8f;

    private bool isOpen = false;
    private CanvasGroup canvasGroup;
    private List<RectTransform> highlightInstances = new List<RectTransform>();

    [System.Serializable]
    public class MapHighlight
    {
        public string label = "Point of Interest";
        public Vector3 worldPosition;
        public Sprite icon;
        public Color color = Color.white;
    }

    void Awake()
    {
        canvasGroup = mapPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = mapPanel.AddComponent<CanvasGroup>();

        mapPanel.SetActive(false);
        canvasGroup.alpha = 0f;

        SpawnHighlightMarkers();
    }

    void OnEnable()
    {
        if (closeMapAction != null)
        {
            closeMapAction.action.Enable();
            closeMapAction.action.performed += OnCloseMap;
        }
    }

    void OnDisable()
    {
        if (closeMapAction != null)
        {
            closeMapAction.action.performed -= OnCloseMap;
        }
    }

    void Update()
    {
        if (!isOpen) return;

        UpdatePlayerMarker();

        // Fade in
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, openSpeed * Time.unscaledDeltaTime);
    }

    // ---- Public API ----

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        mapPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        // Time.timeScale = 0f; // Pause game while map is open

        UpdatePlayerMarker();
        UpdateHighlightMarkers();

        Debug.Log("[WorldMap] Opened.");
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        mapPanel.SetActive(false);
        // Time.timeScale = 1f;

        Debug.Log("[WorldMap] Closed.");
    }

    // ---- Input ----

    void OnCloseMap(InputAction.CallbackContext ctx)
    {
        if (isOpen) Close();
    }

    // ---- Positioning ----

    /// <summary>
    /// Converts a world XZ position to a local position on the map RectTransform.
    /// </summary>
    Vector2 WorldToMapPosition(Vector3 worldPos)
    {
        // Normalize world position to 0-1 range
        float u = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float v = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.z);

        // Convert to map rect local position
        Rect rect = mapImage.rect;
        float x = Mathf.Lerp(rect.xMin, rect.xMax, u);
        float y = Mathf.Lerp(rect.yMin, rect.yMax, v);

        return new Vector2(x, y);
    }

    void UpdatePlayerMarker()
    {
        if (playerTransform == null || playerMarker == null || mapImage == null) return;

        Vector2 mapPos = WorldToMapPosition(playerTransform.position);
        playerMarker.anchoredPosition = mapPos;
    }

    // ---- Highlights ----

    void SpawnHighlightMarkers()
    {
        if (highlightMarkerPrefab == null || mapImage == null) return;

        foreach (var highlight in highlights)
        {
            GameObject marker = Instantiate(highlightMarkerPrefab, mapImage);
            RectTransform rt = marker.GetComponent<RectTransform>();

            // Set icon and color if available
            Image img = marker.GetComponent<Image>();
            if (img != null)
            {
                if (highlight.icon != null) img.sprite = highlight.icon;
                img.color = highlight.color;
            }

            // Set label if there's a child Text
            var label = marker.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null) label.text = highlight.label;

            highlightInstances.Add(rt);
        }
    }

    void UpdateHighlightMarkers()
    {
        for (int i = 0; i < highlights.Count && i < highlightInstances.Count; i++)
        {
            Vector2 mapPos = WorldToMapPosition(highlights[i].worldPosition);
            highlightInstances[i].anchoredPosition = mapPos;
        }
    }

    // ---- Editor: Add/Remove highlights easily ----

    /// <summary>Call from editor or script to add a new highlight at runtime.</summary>
    public void AddHighlight(string label, Vector3 worldPosition, Sprite icon = null, Color? color = null)
    {
        var h = new MapHighlight
        {
            label = label,
            worldPosition = worldPosition,
            icon = icon,
            color = color ?? Color.white
        };
        highlights.Add(h);

        // Spawn marker if prefab exists
        if (highlightMarkerPrefab != null && mapImage != null)
        {
            GameObject marker = Instantiate(highlightMarkerPrefab, mapImage);
            RectTransform rt = marker.GetComponent<RectTransform>();

            Image img = marker.GetComponent<Image>();
            if (img != null)
            {
                if (h.icon != null) img.sprite = h.icon;
                img.color = h.color;
            }

            var labelText = marker.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (labelText != null) labelText.text = h.label;

            Vector2 mapPos = WorldToMapPosition(h.worldPosition);
            rt.anchoredPosition = mapPos;

            highlightInstances.Add(rt);
        }
    }

    /// <summary>Remove a highlight by label.</summary>
    public void RemoveHighlight(string label)
    {
        for (int i = highlights.Count - 1; i >= 0; i--)
        {
            if (highlights[i].label == label)
            {
                highlights.RemoveAt(i);
                if (i < highlightInstances.Count)
                {
                    Destroy(highlightInstances[i].gameObject);
                    highlightInstances.RemoveAt(i);
                }
                return;
            }
        }
    }
}
