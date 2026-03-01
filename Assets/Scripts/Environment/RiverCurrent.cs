using UnityEngine;

/// <summary>
/// Place on a trigger volume covering a river section.
/// Pushes the player in flowDirection unless they have Boots.
/// 
/// Setup:
/// 1. Create an empty GameObject, add BoxCollider (Is Trigger = true)
/// 2. Shape it to cover the river area
/// 3. Add this script, set flowDirection to point downstream
/// 4. Layer doesn't matter — it uses OnTriggerStay with tag check
/// </summary>
public class RiverCurrent : MonoBehaviour
{
    [Header("Current Settings")]
    [Tooltip("Direction the river flows (local space). Will be normalized.")]
    [SerializeField] private Vector3 flowDirection = Vector3.forward;

    [Tooltip("How strong the current pushes the player (units/sec)")]
    [SerializeField] private float currentStrength = 6f;

    [Tooltip("Strength when player has boots (reduced but not zero for feel)")]
    [SerializeField] private float bootsReducedStrength = 1f;

    [Header("Visual")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.5f, 1f, 0.3f);

    private Vector3 worldFlowDirection;

    void Start()
    {
        worldFlowDirection = transform.TransformDirection(flowDirection.normalized);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null) return;

        float strength = currentStrength;

        // Check if player has boots
        if (ItemEffectManager.Instance != null && ItemEffectManager.Instance.CanResistCurrent())
        {
            strength = bootsReducedStrength;
        }

        // Apply current force
        Vector3 push = worldFlowDirection * strength * Time.deltaTime;
        cc.Move(push);
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.DrawCube(box.center, box.size);

            // Draw flow arrow
            Gizmos.color = Color.cyan;
            Vector3 center = box.center;
            Vector3 dir = flowDirection.normalized * Mathf.Max(box.size.x, box.size.z) * 0.4f;
            Gizmos.DrawLine(center - dir, center + dir);
            Gizmos.DrawSphere(center + dir, 0.2f);
        }
    }
}
