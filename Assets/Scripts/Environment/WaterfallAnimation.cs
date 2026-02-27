using UnityEngine;

public class WaterfallAnimation : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WaterfallData waterfallData;
    
    private Material waterfallMaterial;
    private Vector2 textureOffset;

    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            waterfallMaterial = renderer.material;
        }
    }

    void Update()
    {
        if (waterfallMaterial != null && waterfallData != null)
        {
            // Scroll texture downward to simulate flowing water
            textureOffset += waterfallData.flowDirection * waterfallData.flowSpeed * Time.deltaTime;
            waterfallMaterial.mainTextureOffset = textureOffset;
            
            // Apply tint if specified
            if (waterfallData.tintColor != Color.white)
            {
                waterfallMaterial.color = waterfallData.tintColor;
            }
        }
    }
}