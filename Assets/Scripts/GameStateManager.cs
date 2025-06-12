using UnityEngine;

/// <summary>
/// Stores persistent data across scenes, including player progress,
/// collected plant, patient status, gold, supply packs, etc.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Treatment Data")]
    public ItemData collectedPlant;        // The plant currently selected or collected
    public string currentDisease;          // Name of the current diagnosis

    [Header("Progress & Economy")]
    public int gold = 0;                   // Current gold count
    public int patientsCured = 0;          // Number of patients successfully treated
    public int currentDay = 1;             // Current day number
    public bool isTutorial = true;         // Whether we're in tutorial mode

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

    void Start()
    {
        if (isTutorial)
        {
            InitializeTutorialDefaults();
        }
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
        patientsCured = 0;
        currentDay = 1;
        mountainMapUnlocked = false;
        canyonMapUnlocked = false;
    }

    /// <summary>
    /// Set up default values specifically for the tutorial.
    /// Call this in PostTreatmentScene Start() if needed.
    /// </summary>
    public void InitializeTutorialDefaults()
    {
        gold = 5;
        Debug.Log("[Init] Tutorial defaults applied: 5 gold.");
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
    /// Spend gold. Returns true if successful, false if not enough.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"[Economy] Spent {amount} gold. Remaining = {gold}");
            return true;
        }
        Debug.LogWarning("[Economy] Not enough gold!");
        return false;
    }

    /// <summary>
    /// Call this when a patient is successfully cured.
    /// </summary>
    public void OnPatientCured(bool rewardGold)
    {
        patientsCured++;

        if (rewardGold)
        {
            AddGold(10);
        }

        Debug.Log($"[Treatment] Patient cured. Total cured: {patientsCured}");
    }

    public void SetRequiredPlants(ItemData[] plants)
    {
        requiredPlants = plants;
    }

    public void CompleteTutorial()
    {
        isTutorial = false;
        Debug.Log("[Progress] Tutorial completed!");
    }
}
