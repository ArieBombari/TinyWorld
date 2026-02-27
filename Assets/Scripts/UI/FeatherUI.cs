// ============================================================================
// FeatherUI.cs — Shows feather icons, greyed out when used
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FeatherUI : MonoBehaviour
{
    public static FeatherUI Instance;

    [Header("Feather Icon")]
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private float iconSize = 40f;
    [SerializeField] private float spacing = 8f;

    [Header("Colors")]
    [Tooltip("Color when feather is available")]
    [SerializeField] private Color activeColor = Color.white;

    [Tooltip("Color when feather is used (greyed out)")]
    [SerializeField] private Color usedColor = new Color(0.5f, 0.5f, 0.5f, 0.35f);

    [Header("Animation")]
    [SerializeField] private float popInDuration = 0.3f;
    [SerializeField] private float popOvershoot = 1.3f;
    [SerializeField] private float colorFadeDuration = 0.25f;

    private List<RectTransform> featherIcons = new List<RectTransform>();
    private List<Image> featherImages = new List<Image>();
    private int totalCollected = 0;
    private int currentAvailable = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        SetupLayout();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void SetupLayout()
    {
        var oldTexts = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (var t in oldTexts)
            Destroy(t.gameObject);

        var layoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();

        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.spacing = spacing;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        var fitter = GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void UpdateFeatherCount(int count)
    {
        int collected = 0;
        if (FeatherManager.Instance != null)
            collected = FeatherManager.Instance.GetFeathersCollected();

        // Ensure enough icons for all collected feathers
        while (featherIcons.Count < collected)
            CreateFeatherIcon(featherIcons.Count);

        for (int i = 0; i < featherIcons.Count; i++)
        {
            bool isCollected = i < collected;
            bool isAvailable = i < count;

            featherIcons[i].gameObject.SetActive(isCollected);

            if (isCollected)
            {
                // Newly collected — pop in with active color
                if (i >= totalCollected && i < collected)
                {
                    featherImages[i].color = activeColor;
                    if (popInDuration > 0)
                        StartCoroutine(PopIn(featherIcons[i]));
                }
                else
                {
                    // Fade to correct color based on available/used
                    Color targetColor = isAvailable ? activeColor : usedColor;
                    if (featherImages[i].color != targetColor)
                        StartCoroutine(FadeColor(featherImages[i], targetColor));
                }
            }
        }

        totalCollected = collected;
        currentAvailable = count;
    }

    private void CreateFeatherIcon(int index)
    {
        GameObject iconObj = new GameObject($"Feather_{index}", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(transform, false);

        var rt = iconObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(iconSize, iconSize);

        var img = iconObj.GetComponent<Image>();
        img.sprite = iconSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = activeColor;

        var le = iconObj.AddComponent<LayoutElement>();
        le.preferredWidth = iconSize;
        le.preferredHeight = iconSize;

        iconObj.SetActive(false);

        featherIcons.Add(rt);
        featherImages.Add(img);
    }

    private IEnumerator PopIn(RectTransform icon)
    {
        float elapsed = 0f;
        icon.localScale = Vector3.zero;

        while (elapsed < popInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / popInDuration;

            float scale;
            if (t < 0.6f)
                scale = Mathf.Lerp(0f, popOvershoot, t / 0.6f);
            else
                scale = Mathf.Lerp(popOvershoot, 1f, (t - 0.6f) / 0.4f);

            icon.localScale = Vector3.one * scale;
            yield return null;
        }

        icon.localScale = Vector3.one;
    }

    private IEnumerator FadeColor(Image img, Color targetColor)
    {
        Color startColor = img.color;
        float elapsed = 0f;

        while (elapsed < colorFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / colorFadeDuration;
            img.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        img.color = targetColor;
    }
}
