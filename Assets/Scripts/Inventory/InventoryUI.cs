// ============================================================================
// InventoryUI.cs — Manages the inventory UI panel
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private InventoryData inventoryData;

        [Header("Player Tracking")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float abovePlayerOffset = 3.0f;
        [SerializeField] private Camera gameCamera;

        [Header("Slot UI")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private InventorySlotUI slotPrefab;

        [Header("Item Name Bubble")]
        [SerializeField] private GameObject nameBubble;
        [SerializeField] private TextMeshProUGUI nameBubbleText;
        [SerializeField] private float nameBubbleOffset = 50f;
        [SerializeField] private float nameBubbleDelay = 0.3f;

        [Header("Screen UI (toggled on open/close)")]
        [SerializeField] private GameObject goldDisplay;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private GameObject promptsDisplay;

        [Header("Animation")]
        [SerializeField] private float showDuration = 0.25f;
        [SerializeField] private float hideDuration = 0.15f;
        [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private List<InventorySlotUI> slotInstances = new List<InventorySlotUI>();
        private CanvasGroup canvasGroup;
        private RectTransform panelRect;
        private Canvas parentCanvas;
        private RectTransform canvasRect;
        private bool isVisible = false;
        private float animationTimer = 0f;
        private bool animating = false;
        private bool targetVisible = false;
        private bool showNameBubble = false;
        private bool nameBubbleReady = false;
        private Coroutine nameBubbleDelayCoroutine;
        private int previousActiveSlot = -1;

        private void Awake()
        {
            panelRect = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                canvasRect = parentCanvas.GetComponent<RectTransform>();

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (gameCamera == null)
                gameCamera = Camera.main;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(true);

            if (nameBubble != null) nameBubble.SetActive(false);
            if (goldDisplay != null) goldDisplay.SetActive(false);
            if (promptsDisplay != null) promptsDisplay.SetActive(false);
        }

        private void Start()
        {
            if (goldDisplay != null) goldDisplay.SetActive(false);
            if (promptsDisplay != null) promptsDisplay.SetActive(false);
        }

        private void OnEnable()
        {
            if (inventoryData != null)
            {
                inventoryData.OnActiveSlotChanged += HandleActiveSlotChanged;
                inventoryData.OnInventoryChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (inventoryData != null)
            {
                inventoryData.OnActiveSlotChanged -= HandleActiveSlotChanged;
                inventoryData.OnInventoryChanged -= Refresh;
            }
        }

        private void LateUpdate()
        {
            if (animating)
            {
                animationTimer += Time.unscaledDeltaTime;
                float duration = targetVisible ? showDuration : hideDuration;
                float t = Mathf.Clamp01(animationTimer / duration);
                float evaluated = showCurve.Evaluate(t);
                canvasGroup.alpha = targetVisible ? evaluated : (1f - evaluated);

                if (t >= 1f)
                {
                    animating = false;
                    isVisible = targetVisible;
                    canvasGroup.interactable = isVisible;
                    canvasGroup.blocksRaycasts = isVisible;
                }
            }

            if ((isVisible || animating) && playerTransform != null && gameCamera != null)
                PositionAbovePlayer();

            if (isVisible && showNameBubble && nameBubbleReady)
                PositionNameBubble();
        }

        private void PositionAbovePlayer()
        {
            Vector3 worldPoint = playerTransform.position + Vector3.up * abovePlayerOffset;
            Vector3 screenPoint = gameCamera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z < 0) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                new Vector2(screenPoint.x, screenPoint.y),
                null,
                out localPoint
            );
            panelRect.anchoredPosition = localPoint;
        }

        public void Show()
        {
            if (this == null) return;
            if (isVisible && !animating) return;
            if (gameCamera == null) gameCamera = Camera.main;

            EnsureSlots();
            nameBubbleReady = false;
            previousActiveSlot = inventoryData != null ? inventoryData.ActiveSlotIndex : 0;

            targetVisible = true;
            animating = true;
            animationTimer = 0f;

            if (goldDisplay != null) goldDisplay.SetActive(true);
            if (promptsDisplay != null) promptsDisplay.SetActive(true);
            if (nameBubble != null) nameBubble.SetActive(false);

            if (nameBubbleDelayCoroutine != null)
                StopCoroutine(nameBubbleDelayCoroutine);

            nameBubbleDelayCoroutine = StartCoroutine(ShowAfterDelay());
        }

        private IEnumerator ShowAfterDelay()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(slotContainer as RectTransform);
            yield return null;
            RefreshSlotsOnly();
            yield return new WaitForSecondsRealtime(nameBubbleDelay);
            nameBubbleReady = true;
            UpdateNameBubbleState();
        }

        public void Hide()
        {
            targetVisible = false;
            animating = true;
            animationTimer = 0f;
            showNameBubble = false;
            nameBubbleReady = false;

            if (nameBubbleDelayCoroutine != null)
                StopCoroutine(nameBubbleDelayCoroutine);

            if (nameBubble != null) nameBubble.SetActive(false);
            if (goldDisplay != null) goldDisplay.SetActive(false);
            if (promptsDisplay != null) promptsDisplay.SetActive(false);
        }

        private void EnsureSlots()
        {
            if (inventoryData == null || slotPrefab == null || slotContainer == null) return;
            if (slotInstances.Count == inventoryData.Slots.Count) return;

            foreach (var slot in slotInstances)
                if (slot != null) Destroy(slot.gameObject);
            slotInstances.Clear();

            for (int i = 0; i < inventoryData.Slots.Count; i++)
            {
                var instance = Instantiate(slotPrefab, slotContainer);
                instance.SetSlotIndex(i);
                slotInstances.Add(instance);
            }
        }

        public void Refresh()
        {
            if (inventoryData == null) return;
            RefreshSlotsOnly();
            if (nameBubbleReady) UpdateNameBubbleState();
            UpdateGoldDisplay();
        }

        private void RefreshSlotsOnly()
        {
            if (inventoryData == null) return;
            for (int i = 0; i < slotInstances.Count && i < inventoryData.Slots.Count; i++)
            {
                var slotData = inventoryData.Slots[i];
                bool isActive = (i == inventoryData.ActiveSlotIndex);
                slotInstances[i].UpdateVisuals(slotData, isActive);
            }
            UpdateGoldDisplay();
        }

        private void HandleActiveSlotChanged(int newIndex)
        {
            if (!isVisible && !animating) return;

            // Determine bump direction from previous slot
            int direction = 0;
            if (previousActiveSlot >= 0 && previousActiveSlot != newIndex)
            {
                direction = (newIndex > previousActiveSlot) ? 1 : -1;

                // Handle wrap-around
                int slotCount = inventoryData.Slots.Count;
                if (previousActiveSlot == slotCount - 1 && newIndex == 0)
                    direction = 1; // wrapped right
                else if (previousActiveSlot == 0 && newIndex == slotCount - 1)
                    direction = -1; // wrapped left
            }

            previousActiveSlot = newIndex;

            Refresh();

            // Play bump on the newly active slot
            if (direction != 0 && newIndex < slotInstances.Count)
            {
                slotInstances[newIndex].PlayBump(direction);
            }
        }

        private void UpdateNameBubbleState()
        {
            if (nameBubble == null || nameBubbleText == null) return;
            var activeSlot = inventoryData.GetActiveSlot();
            if (activeSlot != null && !activeSlot.IsEmpty)
            {
                showNameBubble = true;
                nameBubble.SetActive(true);
                nameBubbleText.text = activeSlot.item.itemName;
                PositionNameBubble();
            }
            else
            {
                showNameBubble = false;
                nameBubble.SetActive(false);
            }
        }

        private void PositionNameBubble()
        {
            if (nameBubble == null || !nameBubble.activeSelf) return;
            if (inventoryData.ActiveSlotIndex >= slotInstances.Count) return;

            var activeSlotUI = slotInstances[inventoryData.ActiveSlotIndex];
            var bubbleRT = nameBubble.GetComponent<RectTransform>();
            var slotRT = activeSlotUI.GetComponent<RectTransform>();
            if (bubbleRT == null || slotRT == null) return;

            Vector3 slotPos = slotRT.position;
            float slotHeight = slotRT.rect.height * slotRT.lossyScale.y;

            bubbleRT.position = new Vector3(
                slotPos.x,
                slotPos.y + (slotHeight * 0.5f) + (nameBubbleOffset * slotRT.lossyScale.y),
                slotPos.z
            );
        }

        private void UpdateGoldDisplay()
        {
            if (goldText == null) return;
            goldText.text = "0";
        }

        public void SetGoldAmount(int amount)
        {
            if (goldText != null) goldText.text = amount.ToString("N0");
        }
    }
}
