using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager_Field2 : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI taskText;
    public Image taskTextBackground;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI taskDisplay;
    public Button startButton;
    public Button restartButton;
    public Button treatButton;

    [Header("Player")]
    public GameObject playerPrefab;
    private GameObject player;
    private PlayerInventory playerInventory;
    private PlayerController playerController;

    [Header("Gameplay Components")]
    public MapGenerator mapGenerator;
    public TileManager tileManager;
    public PlantSpawner plantSpawner;
    public SnakeSpawner snakeSpawner;

    private Vector3 playerInitialPosition;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip successMusic;
    public AudioClip failureMusic;
    public AudioClip loseLifeSound;
    public AudioClip wrongPlantSound;
    private AudioSource audioSource;

    [Header("Gameplay Settings")]
    public float timeLimit = 120f;
    public ItemData[] requiredPlants;
    public Image[] hearts;
    public int maxLives = 3;

    private bool gameStarted = false;
    private bool gameEnded = false;
    private int currentLives;

    [Header("Penalty UI")]
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private CanvasGroup penaltyCanvasGroup;

    void Start()
    {

        //get required plants from GameStateManager
        requiredPlants = GameStateManager.Instance.requiredPlants;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        RestartGame();
    }

    void RestartGame()
    {
        if (player != null)
            Destroy(player);

        player = Instantiate(playerPrefab);
        player.tag = "Player";
        player.name = "Player";

        tileManager.player = player.transform;

        mapGenerator.GenerateMap();
        tileManager.RefreshTiles();
        plantSpawner.SpawnPlants();
        snakeSpawner.SpawnSnakes();

        Vector2Int spawn = mapGenerator.playerSpawnPoint;
        playerInitialPosition = new Vector3(
            spawn.x * tileManager.tileSize + tileManager.mapOffset.x,
            -spawn.y * tileManager.tileSize + tileManager.mapOffset.y,
            0f
        );

        InitializeGameState();
    }

    void InitializeGameState()
    {
        playerInventory = player.GetComponent<PlayerInventory>();
        playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
            playerController.canMove = false;

        currentLives = maxLives;
        gameStarted = false;
        gameEnded = false;
        timeLimit = 120f;
        player.transform.position = playerInitialPosition;

        ResetUI();

        StartCoroutine(WaitAndClearInventory());
    }

    IEnumerator WaitAndClearInventory()
    {
        yield return new WaitForSeconds(0.2f);
        playerInventory?.ClearInventory();
    }

    void ResetUI()
    {
        if (requiredPlants != null && requiredPlants.Length > 0)
        {
            string task = "Find ";
            for (int i = 0; i < requiredPlants.Length; i++)
            {
                task += requiredPlants[i].itemName;
                if (i < requiredPlants.Length - 1) task += " & ";
            }
            task += " in 2 minutes to finish the potion!";
            taskText.text = task;
        }
        else
        {
            taskText.text = "No task assigned.";
        }

        taskText.gameObject.SetActive(true);
        taskTextBackground.gameObject.SetActive(true);
        timerText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        taskDisplay.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        treatButton.gameObject.SetActive(false);

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

        playerController.canMove = true;
        foreach (var snake in FindObjectsOfType<SnakeController>()) snake.canMove = true;

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        UpdateTaskDisplay();
        UpdateTimer();

        if (CheckCollectedAll())
        {
            EndGame(true);
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
        if (playerInventory == null || requiredPlants == null) return;

        string display = "";
        for (int i = 0; i < requiredPlants.Length; i++)
        {
            int count = playerInventory.GetPlantCount(requiredPlants[i]);
            display += $"{requiredPlants[i].itemName}: {count}/1";
            if (i < requiredPlants.Length - 1) display += ", ";
        }
        taskDisplay.text = display;
    }

    bool CheckCollectedAll()
    {
        foreach (var plant in requiredPlants)
        {
            if (playerInventory.GetPlantCount(plant) < 1)
                return false;
        }
        return true;
    }

    void EndGame(bool success, string reason = "")
    {
        gameEnded = true;
        audioSource.Stop();
        playerController.canMove = false;
        resultText.gameObject.SetActive(true);

        if (success)
        {
            resultText.text = "You gathered all required plants!";
            PlaySound(successMusic);
            treatButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(false);
            treatButton.onClick.AddListener(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TreatmentScene");
            });
        }
        else
        {
            resultText.text = reason == "time"
                ? "Time's up! You failed to gather all plants."
                : "You failed.";
            restartButton.gameObject.SetActive(true);
            PlaySound(failureMusic);
        }

        foreach (var snake in FindObjectsOfType<SnakeController>()) snake.canMove = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    public void ApplyWrongPlantPenalty()
    {
        timeLimit -= 20f;
        if (timeLimit < 0) timeLimit = 0;

        ShowTimePenalty(20);
        PlaySound(wrongPlantSound);
    }

    void ShowTimePenalty(int seconds)
    {
        if (penaltyText == null || penaltyCanvasGroup == null) return;

        penaltyText.text = $"Wrong plant! Time -{seconds}s";
        penaltyCanvasGroup.alpha = 1f;
        StartCoroutine(FadePenaltyText());
    }

    IEnumerator FadePenaltyText()
    {
        yield return new WaitForSeconds(2f);
        penaltyCanvasGroup.alpha = 0f;
    }
}
