using UnityEngine;
using Cinemachine;

public class HouseInterior : MonoBehaviour
{
    [Header("Interior Objects")]
    [SerializeField] private GameObject interior;

    [Header("Objects to Hide When Inside")]
    [SerializeField] private GameObject roof;
    [SerializeField] private GameObject wallFront;
    [SerializeField] private GameObject wallLeft;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera interiorCamera;
    [SerializeField] private CinemachineVirtualCamera exteriorCamera;

    private MeshRenderer roofRenderer;
    private MeshRenderer wallFrontRenderer;
    private MeshRenderer wallLeftRenderer;
    private bool playerInside = false;

    void Start()
    {
        if (roof != null)
            roofRenderer = roof.GetComponent<MeshRenderer>();
        if (wallFront != null)
            wallFrontRenderer = wallFront.GetComponent<MeshRenderer>();
        if (wallLeft != null)
            wallLeftRenderer = wallLeft.GetComponent<MeshRenderer>();

        if (interior != null)
            interior.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered house");
            EnterHouse();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited house");
            ExitHouse();
        }
    }

    void EnterHouse()
    {
        playerInside = true;

        if (interior != null)
            interior.SetActive(true);

        // Simply disable renderers — completely invisible, no ghost edges
        if (roofRenderer != null) roofRenderer.enabled = false;
        if (wallFrontRenderer != null) wallFrontRenderer.enabled = false;
        if (wallLeftRenderer != null) wallLeftRenderer.enabled = false;

        if (interiorCamera != null && exteriorCamera != null)
        {
            interiorCamera.Priority = 20;
            exteriorCamera.Priority = 10;
        }
    }

    void ExitHouse()
    {
        playerInside = false;

        if (interior != null)
            interior.SetActive(false);

        // Re-enable renderers
        if (roofRenderer != null) roofRenderer.enabled = true;
        if (wallFrontRenderer != null) wallFrontRenderer.enabled = true;
        if (wallLeftRenderer != null) wallLeftRenderer.enabled = true;

        if (interiorCamera != null && exteriorCamera != null)
        {
            interiorCamera.Priority = 10;
            exteriorCamera.Priority = 20;
        }
    }
}
