// ============================================================================
// InventorySlotUI.cs — Visual behaviour for a single inventory slot
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Inventory
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("Child References (assign in prefab)")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image highlightCircle;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("Icon")]
        [SerializeField] private float iconSize = 55f;
        [SerializeField] private float activeIconMultiplier = 1.2f;

        [Header("Circle (behind active item)")]
        [SerializeField] private float circleToIconPadding = 1.35f;

        [Header("Dots (empty slots)")]
        [SerializeField] private float dotSize = 8f;
        [SerializeField] private Color dotColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        [SerializeField] private Color activeDotColor = new Color(0.4f, 0.85f, 0.8f, 0.85f);

        [Header("Animation")]
        [SerializeField] private float animSpeed = 10f;

        [Header("Bump Effect")]
        [Tooltip("How far the slot bumps in pixels")]
        [SerializeField] private float bumpDistance = 12f;
        [Tooltip("How long the bump takes in seconds")]
        [SerializeField] private float bumpDuration = 0.2f;

        private int slotIndex;
        private RectTransform rectTransform;
        private RectTransform circleRect;
        private RectTransform iconRect;

        private Vector2 targetCircleSize;
        private Vector2 targetIconSize;

        private Coroutine bumpCoroutine;
        private float currentBumpOffset = 0f;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (highlightCircle != null)
                circleRect = highlightCircle.GetComponent<RectTransform>();

            if (iconImage != null)
                iconRect = iconImage.GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime * animSpeed;

            if (circleRect != null)
                circleRect.sizeDelta = Vector2.Lerp(circleRect.sizeDelta, targetCircleSize, dt);

            if (iconRect != null)
                iconRect.sizeDelta = Vector2.Lerp(iconRect.sizeDelta, targetIconSize, dt);
        }

        public void SetSlotIndex(int index) => slotIndex = index;

        /// <summary>
        /// Plays a bump in the navigation direction.
        /// direction: -1 = navigated left, 1 = navigated right
        /// </summary>
        public void PlayBump(int direction)
        {
            if (bumpCoroutine != null)
                StopCoroutine(bumpCoroutine);

            bumpCoroutine = StartCoroutine(BumpAnimation(direction));
        }

        private IEnumerator BumpAnimation(int direction)
        {
            // Reset any leftover offset
            ApplyBumpOffset(-currentBumpOffset);
            currentBumpOffset = 0f;

            float peakOffset = bumpDistance * direction;
            float halfTime = bumpDuration * 0.3f;
            float returnTime = bumpDuration * 0.7f;

            // Phase 1: Quick move in direction
            float elapsed = 0f;
            while (elapsed < halfTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfTime);
                float eased = 1f - (1f - t) * (1f - t); // ease out
                float newOffset = peakOffset * eased;
                ApplyBumpOffset(newOffset - currentBumpOffset);
                currentBumpOffset = newOffset;
                yield return null;
            }

            // Phase 2: Spring back with slight overshoot
            elapsed = 0f;
            float startOffset = currentBumpOffset;
            while (elapsed < returnTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / returnTime);
                // Elastic return
                float decay = 1f - t;
                float spring = Mathf.Sin(t * Mathf.PI) * 0.2f * startOffset;
                float newOffset = startOffset * decay * decay + spring * (1f - t);
                ApplyBumpOffset(newOffset - currentBumpOffset);
                currentBumpOffset = newOffset;
                yield return null;
            }

            // Snap clean
            ApplyBumpOffset(-currentBumpOffset);
            currentBumpOffset = 0f;
            bumpCoroutine = null;
        }

        private void ApplyBumpOffset(float delta)
        {
            if (rectTransform != null)
            {
                var pos = rectTransform.anchoredPosition;
                pos.x += delta;
                rectTransform.anchoredPosition = pos;
            }
        }

        public void UpdateVisuals(InventorySlot slotData, bool active)
        {
            bool hasItem = slotData != null && !slotData.IsEmpty;

            if (active && hasItem)
            {
                float activeSize = iconSize * activeIconMultiplier;
                float circleSize = activeSize * circleToIconPadding;

                targetIconSize = Vector2.one * activeSize;
                targetCircleSize = Vector2.one * circleSize;

                highlightCircle.gameObject.SetActive(true);
                highlightCircle.color = slotData.item.highlightColor;
                iconImage.enabled = true;
                iconImage.sprite = slotData.item.icon;
                iconImage.color = Color.white;
            }
            else if (!active && hasItem)
            {
                targetIconSize = Vector2.one * iconSize;
                targetCircleSize = Vector2.one * 0f;

                highlightCircle.gameObject.SetActive(false);
                iconImage.enabled = true;
                iconImage.sprite = slotData.item.icon;
                iconImage.color = Color.white;
            }
            else if (active && !hasItem)
            {
                targetCircleSize = Vector2.one * (dotSize * 1.2f);
                targetIconSize = Vector2.one * iconSize;

                highlightCircle.gameObject.SetActive(true);
                highlightCircle.color = activeDotColor;
                iconImage.enabled = false;
            }
            else
            {
                targetCircleSize = Vector2.one * dotSize;
                targetIconSize = Vector2.one * iconSize;

                highlightCircle.gameObject.SetActive(true);
                highlightCircle.color = dotColor;
                iconImage.enabled = false;
            }

            if (countText != null)
            {
                bool showCount = hasItem && slotData.count > 1;
                countText.gameObject.SetActive(showCount);
                if (showCount) countText.text = "x" + slotData.count;
            }
        }
    }
}
