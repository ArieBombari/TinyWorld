using UnityEngine;
using Inventory;

/// <summary>
/// Central manager that applies item effects based on inventory contents.
/// Attach to the Player GameObject.
/// 
/// Passive items (Boots, Wings, Hat): effect active when item is anywhere in inventory.
/// Active items (Lantern, Map): effect active only when item is selected in inventory.
/// </summary>
public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Lantern")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private float lanternRange = 8f;
    [SerializeField] private float lanternIntensity = 1.5f;
    [SerializeField] private Color lanternColor = new Color(1f, 0.85f, 0.5f);
    [SerializeField] private float lanternFadeSpeed = 4f;

    [Header("Wings")]
    [SerializeField] private float wingsGlideMultiplier = 1.2f;

    [Header("Map")]
    [SerializeField] private WorldMapUI worldMapUI;

    // Public state — other scripts check these
    public bool HasBoots { get; private set; }
    public bool HasWings { get; private set; }
    public bool HasHat { get; private set; }
    public bool HasLantern { get; private set; }
    public bool HasMap { get; private set; }
    public bool IsLanternActive { get; private set; }
    public float GlideMultiplier => HasWings ? wingsGlideMultiplier : 1f;

    private float currentLanternIntensity = 0f;
    private ItemData selectedItem;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (lanternLight != null)
        {
            lanternLight.range = lanternRange;
            lanternLight.color = lanternColor;
            lanternLight.intensity = 0f;
            lanternLight.enabled = false;
        }
    }

    void OnEnable()
    {
        if (inventoryData != null)
        {
            inventoryData.OnInventoryChanged += RefreshPassiveEffects;
            inventoryData.OnActiveSlotChanged += OnSlotChanged;
        }
        if (inventoryManager != null)
        {
            inventoryManager.OnItemSelected += OnItemSelected;
        }
    }

    void OnDisable()
    {
        if (inventoryData != null)
        {
            inventoryData.OnInventoryChanged -= RefreshPassiveEffects;
            inventoryData.OnActiveSlotChanged -= OnSlotChanged;
        }
        if (inventoryManager != null)
        {
            inventoryManager.OnItemSelected -= OnItemSelected;
        }
    }

    void Update()
    {
        UpdateLantern();
    }

    // ---- Passive Effects (checked whenever inventory changes) ----

    void RefreshPassiveEffects()
    {
        HasBoots = inventoryData.HasItem("item_boots");
        HasWings = inventoryData.HasItem("item_wings");
        HasHat = inventoryData.HasItem("item_hat");
        HasLantern = inventoryData.HasItem("item_lantern");
        HasMap = inventoryData.HasItem("item_map");

        RefreshActiveEffects();
    }

    // ---- Active Effects (checked when selected slot changes) ----

    void OnSlotChanged(int newIndex)
    {
        RefreshActiveEffects();
    }

    void OnItemSelected(ItemData item)
    {
        // "Select" triggers active use — for Map, open it
        if (item != null && item.ability == ItemAbility.Map)
        {
            ToggleMap();
        }
    }

    void RefreshActiveEffects()
    {
        var activeSlot = inventoryData.GetActiveSlot();
        selectedItem = (activeSlot != null && !activeSlot.IsEmpty) ? activeSlot.item : null;

        // Lantern: active when selected in the active slot
        bool lanternSelected = selectedItem != null && selectedItem.ability == ItemAbility.Lantern;
        IsLanternActive = lanternSelected;
    }

    // ---- Lantern Light ----

    void UpdateLantern()
    {
        if (lanternLight == null) return;

        float targetIntensity = IsLanternActive ? lanternIntensity : 0f;
        currentLanternIntensity = Mathf.MoveTowards(currentLanternIntensity, targetIntensity, lanternFadeSpeed * Time.deltaTime);

        if (currentLanternIntensity > 0.01f)
        {
            lanternLight.enabled = true;
            lanternLight.intensity = currentLanternIntensity;
        }
        else
        {
            lanternLight.enabled = false;
            lanternLight.intensity = 0f;
        }
    }

    // ---- Map ----

    void ToggleMap()
    {
        if (worldMapUI != null)
        {
            worldMapUI.Toggle();
        }
        else
        {
            Debug.LogWarning("[ItemEffectManager] WorldMapUI not assigned.");
        }
    }

    // ---- Public Queries (for other scripts) ----

    /// <summary>Can the player resist river currents?</summary>
    public bool CanResistCurrent()
    {
        return HasBoots;
    }

    /// <summary>Is the player protected from cold/feather freezing?</summary>
    public bool IsProtectedFromCold()
    {
        return HasHat;
    }

    /// <summary>Force a refresh (call after giving items via script).</summary>
    public void ForceRefresh()
    {
        RefreshPassiveEffects();
    }
}
