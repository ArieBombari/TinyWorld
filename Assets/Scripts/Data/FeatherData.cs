using UnityEngine;

[CreateAssetMenu(fileName = "FeatherSettings", menuName = "Game/Feather Settings")]
public class FeatherData : ScriptableObject
{
    [Header("Collection")]
    public int maxFeathersInWorld = 15; // Total feathers placed in world
    
    [Header("Recharge")]
    public float rechargeDelay = 0.5f; // Delay after landing before recharge starts
    public float rechargeRate = 0.5f; // Seconds per feather recharge
    
    [Header("Usage")]
    public bool unlimitedFeathers = false; // For testing/easy mode
    public bool instantRecharge = false; // For testing
}