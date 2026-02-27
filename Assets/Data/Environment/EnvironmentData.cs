using UnityEngine;

[CreateAssetMenu(fileName = "OceanSettings", menuName = "Game/Environment/Ocean Settings")]
public class OceanData : ScriptableObject
{
    [Header("Wave Animation")]
    public float waveSpeed = 0.1f;
    public float waveScale = 0.2f;
    
    [Header("Vertex Waves")]
    public float waveHeight = 0.5f;
    public float vertexWaveSpeed = 1f;
    public float waveFrequency = 1f;
}

[CreateAssetMenu(fileName = "WaterfallSettings", menuName = "Game/Environment/Waterfall Settings")]
public class WaterfallData : ScriptableObject
{
    [Header("Flow Animation")]
    public float flowSpeed = 0.5f;
    public Vector2 flowDirection = new Vector2(0, -1);
    
    [Header("Visual")]
    public float opacity = 0.8f;
    public Color tintColor = Color.white;
}

[CreateAssetMenu(fileName = "WindSettings", menuName = "Game/Environment/Wind Settings")]
public class WindData : ScriptableObject
{
    [Header("Wind Settings")]
    public float windSpeed = 1f;
    public float windStrength = 0.1f;
    public float windVariation = 0.5f;
    
    [Header("Per-Plant Multipliers")]
    public float treeMultiplier = 1f;
    public float bushMultiplier = 1.5f;
    public float grassMultiplier = 2f;
}