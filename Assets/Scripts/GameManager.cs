using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TextMeshProUGUI taskText;
    public Image taskTextBackground;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI taskDisplay;
    public Button startButton;
    public Button restartButton;

    public GameObject player;
    private PlayerInventory playerInventory;
    private PlayerController playerController;

    public MapGenerator mapGenerator;
    public TileManager tileManager;
    public PlantSpawner plantSpawner; // ✅ NEW: Reference to plant spawner

    private Vector3 playerInitialPosition;

    public AudioClip backgroundMusic;
    public AudioClip successMusic;
    public AudioClip failureMusic;
    public AudioClip loseLifeSound;
    public AudioClip wrongPlantSound;

    private float timeLimit = 120f;
    private bool gameStarted = false;
    private bool gameEnded = false;

    public ItemData requiredPlant;

    public Image[] hearts;
    public int maxLives = 3;
    private int currentLives;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        playerInventory = player.GetComponent<PlayerInventory>();
        playerController = player.GetComponent<PlayerController>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        mapGenerator.GenerateMap();

        Vector2Int spawn = mapGenerator.playerSpawnPoint;
        playerInitialPosition = new Vector3(
            spawn.x * tileManager.tileSize + tileManager.mapOffset.x,
            -spawn.y * tileManager.tileSize + tileManager.mapOffset.y,
            0f
        );
        Debug.Log($"[Spawn] Player spawn at: {spawn.x}, {spawn.y}");

        InitializeGameState();
    }

    void InitializeGameState()
    {
        if (playerController != null)
            playerController.canMove = false;

        currentLives = maxLives;
        UpdateHeartsUI();

        ResetUIElements();

        timeLimit = 120f;
        gameStarted = false;
        gameEnded = false;

        player.transform.position = playerInitialPosition;
        playerInventory.ClearInventory();
    }

    void ResetUIElements()
    {
        taskText.text = $"Find the {requiredPlant.itemName} in 2 minutes to finish the potion!";
        taskText.gameObject.SetActive(true);
        taskTextBackground.gameObject.SetActive(true);

        timerText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        taskDisplay.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        startButton.gameObject.SetActive(true);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartGame);

        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartGame);
    }

    void StartGame()
    {
        startButton.gameObject.SetActive(false);
        taskText.gameObject.SetActive(false);
        taskTextBackground.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);
        taskDisplay.gameObject.SetActive(true);
        gameStarted = true;

        if (playerController != null)
            playerController.canMove = true;

        foreach (var snake in FindObjectsOfType<SnakeController>())
        {
            if (snake != null)
                snake.canMove = true;
        }

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (gameStarted && !gameEnded)
        {
            UpdateTaskDisplay();
            UpdateTimer();

            if (CheckForRequiredPlant())
            {
                EndGame(true);
            }
        }
    }

    void UpdateTimer()
    {
        timeLimit -= Time.deltaTime;

        if (timeLimit <= 0)
        {
            timeLimit = 0;
            EndGame(false, "time");
        }

        int minutes = Mathf.FloorToInt(timeLimit / 60);
        int seconds = Mathf.FloorToInt(timeLimit % 60);
        timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
    }

    void UpdateTaskDisplay()
    {
        if (playerInventory != null)
        {
            int collectedPlant = playerInventory.GetPlantCount(requiredPlant);
            taskDisplay.text = $"{requiredPlant.itemName} x {collectedPlant}";
        }
    }

    bool CheckForRequiredPlant()
    {
        int collectedPlant = playerInventory.GetPlantCount(requiredPlant);
        return collectedPlant >= 1;
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            PlaySound(loseLifeSound);
            UpdateHeartsUI();

            if (currentLives <= 0)
            {
                EndGame(false, "lives");
            }
        }
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentLives;
        }
    }

    void EndGame(bool success, string failureReason = "")
    {
        gameEnded = true;

        audioSource.Stop();
        if (playerController != null)
            playerController.canMove = false;

        resultText.gameObject.SetActive(true);

        if (success)
        {
            resultText.text = "Congratulations!\nYou have successfully gathered the plant!";
            PlaySound(successMusic);
        }
        else
        {
            if (failureReason == "time")
            {
                resultText.text = "Time's up!\nYou failed to gather the plant.";
            }
            else if (failureReason == "lives")
            {
                resultText.text = "You ran out of lives!\nYou failed to gather the plant.";
            }
            PlaySound(failureMusic);
        }

        foreach (var snake in FindObjectsOfType<SnakeController>())
        {
            if (snake != null)
                snake.canMove = false;
        }

        restartButton.gameObject.SetActive(true);
    }

    void RestartGame()
    {
        // ✅ Step 1: regenerate the map
        mapGenerator.GenerateMap();

        // ✅ Step 1.5: refresh the tilemap
        if (tileManager != null)
            tileManager.RefreshTiles();

        // ✅ Step 2: regenerate plants
        if (plantSpawner != null)
            plantSpawner.SpawnPlants();

        // ✅ Step 3: update player spawn point
        Vector2Int spawn = mapGenerator.playerSpawnPoint;
        playerInitialPosition = new Vector3(
            spawn.x * tileManager.tileSize + tileManager.mapOffset.x,
            -spawn.y * tileManager.tileSize + tileManager.mapOffset.y,
            0f
        );

        // ✅ Step 4: reset the rest
        InitializeGameState();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ApplyWrongPlantPenalty()
    {
        timeLimit -= 20f;
        if (timeLimit < 0) timeLimit = 0;

        Debug.LogWarning("[Penalty] Collected wrong plant! -20s");

        if (timerText != null)
        {
            StartCoroutine(FlashTimerRed());
        }

        if (wrongPlantSound != null)
        {
            audioSource.PlayOneShot(wrongPlantSound);
        }
    }

    private IEnumerator FlashTimerRed()
    {
        Color originalColor = timerText.color;
        Vector3 originalScale = timerText.transform.localScale;

        timerText.color = Color.red;
        timerText.transform.localScale = originalScale * 1.3f;

        yield return new WaitForSeconds(0.3f);

        timerText.color = originalColor;
        timerText.transform.localScale = originalScale;
    }
}
