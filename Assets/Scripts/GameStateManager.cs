using UnityEngine;

/// <summary>
/// Stores persistent data across scenes, including player progress,
/// collected plant, patient status, gold, supply packs, etc.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [Header("Treatment Data")]
    public ItemData collectedPlant;        // The plant currently selected or collected
    public string currentDisease;          // Name of the current diagnosis

    [Header("Progress & Economy")]
    public int gold = 0;                   // Current gold count
    public int supplyPacks = 0;            // Current supply pack count
    public int patientsCured = 0;          // Number of patients successfully treated

    [Header("Map Unlock Progress")]
    public bool mountainMapUnlocked = false;
    public bool canyonMapUnlocked = false;

    public ItemData[] requiredPlants;

    void Awake()
    {
        // Ensure only one instance exists across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads
    }

    /// <summary>
    /// Resets all gameplay-related progress.
    /// Call this if the player starts a new game or wants to reset data.
    /// </summary>
    public void ResetProgress()
    {
        collectedPlant = null;
        currentDisease = "";
        gold = 0;
        supplyPacks = 0;
        patientsCured = 0;
        mountainMapUnlocked = false;
        canyonMapUnlocked = false;
    }

    /// <summary>
    /// Set up default values specifically for the tutorial.
    /// Call this in PostTreatmentScene Start() if needed.
    /// </summary>
    public void SetTutorialDefaults()
    {
        gold = 5;
        supplyPacks = 1;
        patientsCured = 0;

        Debug.Log("[Init] Tutorial defaults applied: 5 gold, 1 supply pack.");
    }

    /// <summary>
    /// Add gold to the player's inventory.
    /// </summary>
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"[Economy] Gained {amount} gold. Total = {gold}");
    }

    /// <summary>
    /// Add supply packs to the player's inventory.
    /// </summary>
    public void AddSupply(int amount)
    {
        supplyPacks += amount;
        Debug.Log($"[Economy] Gained {amount} supply pack(s). Total = {supplyPacks}");
    }

    /// <summary>
    /// Spend supply packs. Returns true if successful, false if not enough.
    /// </summary>
    public bool SpendSupply(int amount)
    {
        if (supplyPacks >= amount)
        {
            supplyPacks -= amount;
            Debug.Log($"[Economy] Spent {amount} supply pack(s). Remaining = {supplyPacks}");
            return true;
        }
        else
        {
            Debug.LogWarning("[Economy] Not enough supply packs!");
            return false;
        }
    }

    /// <summary>
    /// Call this when a patient is successfully cured.
    /// </summary>
    public void OnPatientCured(bool rewardGold)
    {
        patientsCured++;
        AddSupply(1);

        if (rewardGold)
        {
            AddGold(5); // Adjust as needed
        }

        Debug.Log($"[Treatment] Patient cured. Total cured: {patientsCured}");
    }

    public void SetRequiredPlants(ItemData[] plants)
    {
        requiredPlants = plants;
    }
}
