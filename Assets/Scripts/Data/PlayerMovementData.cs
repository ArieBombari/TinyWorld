using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovement", menuName = "Game/Player Movement Data")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Ground Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 5f;
    
    [Header("Jumping")]
    public float jumpForce = 8f;
    public float gravity = -40f;
    
    [Header("Climbing")]
    public float climbSpeed = 3f;
    public float climbCheckDistance = 0.6f;
    public float featherDrainRate = 0.5f;
    public float minClimbAngle = 40f;
    
    [Header("Swimming")]
    public float swimSpeed = 3f;
    public float swimSprintSpeed = 5f;
    public float submersionDepth = 0.8f;
    
    [Header("Gliding")]
    public float glideSpeed = 8f;
    public float glideFallSpeed = 2f;
    public float glideRotationSpeed = 5f;
    public float maxTiltAngle = 30f;
    public float tiltSpeed = 6f;
    public float minHoldTimeToGlide = 0.2f;
}