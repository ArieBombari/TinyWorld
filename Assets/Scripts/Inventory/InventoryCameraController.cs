using UnityEngine;

namespace Inventory
{
    public class InventoryCameraController : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float zoomAmount = 0.2f;
        [SerializeField] private float zoomSpeed = 4f;

        [Header("Camera Reference")]
        [SerializeField] private Camera targetCamera;

        private float originalFOV;
        private float originalOrthoSize;
        private float targetFOV;
        private float targetOrthoSize;
        private bool isOrthographic;
        private bool isZoomedIn = false;
        private bool initialized = false;

        private void Start() => Initialize();

        private void Initialize()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("[InventoryCameraController] No camera found!");
                return;
            }

            isOrthographic = targetCamera.orthographic;
            originalFOV = targetCamera.fieldOfView;
            originalOrthoSize = targetCamera.orthographicSize;
            targetFOV = originalFOV;
            targetOrthoSize = originalOrthoSize;
            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized || targetCamera == null) return;

            if (isOrthographic)
            {
                if (!Mathf.Approximately(targetCamera.orthographicSize, targetOrthoSize))
                    targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, targetOrthoSize, Time.deltaTime * zoomSpeed);
            }
            else
            {
                if (!Mathf.Approximately(targetCamera.fieldOfView, targetFOV))
                    targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
            }
        }

        public void ZoomIn()
        {
            if (!initialized) { Initialize(); if (!initialized) return; }

            if (!isZoomedIn)
            {
                originalFOV = targetCamera.fieldOfView;
                originalOrthoSize = targetCamera.orthographicSize;
            }
            isZoomedIn = true;

            if (isOrthographic)
                targetOrthoSize = originalOrthoSize * (1f - zoomAmount);
            else
                targetFOV = originalFOV * (1f - zoomAmount);
        }

        public void ZoomOut()
        {
            if (!initialized) return;
            isZoomedIn = false;
            targetFOV = originalFOV;
            targetOrthoSize = originalOrthoSize;
        }

        public void RecaptureDefaults()
        {
            if (targetCamera == null) return;
            originalFOV = targetCamera.fieldOfView;
            originalOrthoSize = targetCamera.orthographicSize;
            isOrthographic = targetCamera.orthographic;
        }
    }
}
