using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Canvas canvas;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 3.5f, 0);
    [SerializeField] private float baseWorldSpaceScale = 0.01f; // Base scale
    [SerializeField] private float referenceOrthoSize = 9f; // Your exterior camera size
    
    private Transform target;
    private Camera mainCamera;
    private float currentScale;
    
    void Awake()
    {
    if (canvas == null)
    {
        canvas = GetComponent<Canvas>();
    }
    
    mainCamera = Camera.main;
    
    if (canvas != null)
    {
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
    }
    
    // Force all UI elements to render on top of 3D geometry
    SetAlwaysOnTop();
    
    gameObject.SetActive(false);
    }

    void SetAlwaysOnTop()
    {
    foreach (var graphic in GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
    {
        Material mat = new Material(graphic.materialForRendering);
        mat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
        graphic.material = mat;
    }
    
    foreach (var tmp in GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
    {
        tmp.materialForRendering.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
    }
    }
    
    void LateUpdate()
    {
        if (target != null && mainCamera != null)
        {
            // Position above target
            transform.position = target.position + offset;
            
            // Face camera
            transform.rotation = mainCamera.transform.rotation;
            
            // CRITICAL: Scale based on camera orthographic size
            if (mainCamera.orthographic)
            {
                // Scale inversely with ortho size
                // When ortho size is smaller (zoomed in), scale down bubbles
                float scaleMultiplier = mainCamera.orthographicSize / referenceOrthoSize;
                currentScale = baseWorldSpaceScale * scaleMultiplier;
                transform.localScale = Vector3.one * currentScale;
            }
            else
            {
                // For perspective camera, use base scale
                transform.localScale = Vector3.one * baseWorldSpaceScale;
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void SetText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }
    
    public void AddWord(string word)
    {
        if (dialogueText != null)
        {
            dialogueText.text += word;
        }
    }
}