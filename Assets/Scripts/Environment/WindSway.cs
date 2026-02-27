using UnityEngine;

public class WindSway : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WindData windData;
    
    [Header("Plant Type")]
    [SerializeField] private PlantType plantType = PlantType.Tree;
    
    public enum PlantType { Tree, Bush, Grass }
    
    private MeshFilter meshFilter;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;
    private float randomOffset;
    private float typeMultiplier;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        
        if (meshFilter != null && meshFilter.mesh != null)
        {
            originalVertices = meshFilter.mesh.vertices;
            displacedVertices = new Vector3[originalVertices.Length];
            randomOffset = Random.Range(0f, 100f);
        }
        
        // Set multiplier based on plant type
        typeMultiplier = plantType switch
        {
            PlantType.Tree => windData != null ? windData.treeMultiplier : 1f,
            PlantType.Bush => windData != null ? windData.bushMultiplier : 1.5f,
            PlantType.Grass => windData != null ? windData.grassMultiplier : 2f,
            _ => 1f
        };
    }

    void Update()
    {
        if (meshFilter == null || originalVertices == null || windData == null) return;

        float speed = windData.windSpeed * typeMultiplier;
        float strength = windData.windStrength * typeMultiplier;

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            float heightFactor = Mathf.Clamp01(vertex.y / 2f);
            
            float windX = Mathf.Sin(Time.time * speed + randomOffset + vertex.x * windData.windVariation) * strength * heightFactor;
            float windZ = Mathf.Cos(Time.time * speed * 0.8f + randomOffset + vertex.z * windData.windVariation) * strength * heightFactor;
            
            displacedVertices[i] = vertex;
            displacedVertices[i].x += windX;
            displacedVertices[i].z += windZ;
        }

        meshFilter.mesh.vertices = displacedVertices;
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
    }
}