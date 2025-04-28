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
    public int supplyPacks = 3;            // Current supply pack count
    public int patientsCured = 0;          // Number of patients successfully treated

    [Header("Map Unlock Progress")]
    public bool mountainMapUnlocked = false;
    public bool canyonMapUnlocked = false;

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
        supplyPacks = 3;
        patientsCured = 0;
        mountainMapUnlocked = false;
        canyonMapUnlocked = false;
    }
}
