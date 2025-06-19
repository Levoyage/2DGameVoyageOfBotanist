using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager1 : MonoBehaviour
{
    public static GameManager1 Instance { get; private set; }

    public TextMeshProUGUI taskText;
    public Image taskTextBackground;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI taskDisplay;
    public Button startButton;
    public Button restartButton;
    public Button treatButton;

    [Header("Penalty UI")]
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private CanvasGroup penaltyCanvasGroup;

    public GameObject playerPrefab; // 👈 通过 Inspector 挂上 prefab
    public GameObject player;
    private PlayerInventory playerInventory;
    private PlayerController playerController;

    public MapGenerator mapGenerator;
    public TileManager tileManager;
    public PlantSpawner plantSpawner;
    public SnakeSpawner snakeSpawner;

    private Vector3 playerInitialPosition;

    public AudioClip backgroundMusic;
    public AudioClip successMusic;
    public AudioClip failureMusic;
    public AudioClip loseLifeSound;
    public AudioClip wrongPlantSound;

    private float timeLimit = 120f;
    private bool gameStarted = false;
    private bool gameEnded = false;

    public ItemData[] requiredPlants;

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
        //DontDestroyOnLoad(gameObject);
        Debug.Log("🧠 GameManager Awake: " + name);
    }

    void Start()
    {


        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        RestartGame();
    }

    void InitializeGameState()
    {
        playerInventory = PlayerInventory.Instance;
        playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.canMove = false;
            playerController.SetFacingDown();
        }

        currentLives = maxLives;
        UpdateHeartsUI();

        ResetUIElements();

        timeLimit = 120f;
        gameStarted = false;
        gameEnded = false;

        player.transform.position = playerInitialPosition;

        if (penaltyText != null)
            penaltyText.gameObject.SetActive(false);

        if (penaltyCanvasGroup != null)
            penaltyCanvasGroup.alpha = 0f;

        StartCoroutine(WaitAndClearInventory());
    }

    private IEnumerator WaitAndClearInventory()
    {
        yield return new WaitForSeconds(0.2f); // 等待 BackpackUI 注册完成

        if (playerInventory != null)
        {
            playerInventory.ClearInventory();
            Debug.Log("✅ Inventory cleared after UI ready.");
        }
    }

    void ResetUIElements()
    {
        // 显示任务UI
        if (requiredPlants != null && requiredPlants.Length > 0)
        {
            string task = "Find ";
            for (int i = 0; i < requiredPlants.Length; i++)
            {
                task += $"<b><color=red>{requiredPlants[i].itemName}</color></b>";
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

            if (CheckCollectedAll())
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
        if (playerInventory == null || requiredPlants == null) return;

        string display = "";
        for (int i = 0; i < requiredPlants.Length; i++)
        {
            int count = playerInventory.GetPlantCount(requiredPlants[i]);
            display += $"{requiredPlants[i].itemName}: {count}/1";
            if (i < requiredPlants.Length - 1) display += ", ";
        }

        // 添加植物提示
        bool hasFoxglove = false;
        bool hasGinger = false;

        foreach (var plant in requiredPlants)
        {
            string name = plant.itemName.ToLower();
            if (name.Contains("foxglove")) hasFoxglove = true;
            if (name.Contains("ginger")) hasGinger = true;
        }

        if (hasFoxglove && hasGinger)
        {
            display += "\n(Look for tall pink bells; also look for a bulbous root near the soil!)";
        }
        else
        {
            if (hasFoxglove)
                display += "\n(Look for tall pink bells!)";
            if (hasGinger)
                display += "\n(Look for a bulbous root near the soil!)";
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
            treatButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(false);

            treatButton.onClick.RemoveAllListeners();
            treatButton.onClick.AddListener(GoToTreatment);
        }
        else
        {
            if (failureReason == "time")
                resultText.text = "Time's up!\nYou failed to gather the plant.";
            else if (failureReason == "lives")
                resultText.text = "You ran out of lives!\nYou failed to gather the plant.";

            PlaySound(failureMusic);
            restartButton.gameObject.SetActive(true);
            treatButton.gameObject.SetActive(false);
        }

        foreach (var snake in FindObjectsOfType<SnakeController>())
        {
            if (snake != null)
                snake.canMove = false;
        }
    }

    void RestartGame()
    {
        // 1️⃣ 优先复用跨场景保留下来的玩家
        if (PlayerInventory.Instance != null)
        {
            player = PlayerInventory.Instance.gameObject;
        }

        // 2️⃣ 只有在极端情况下（第一次游戏、或手滑删了玩家）才生成
        if (player == null)
        {
            player = Instantiate(playerPrefab);
            DontDestroyOnLoad(player);          // 让它继续跨场景
        }

        // 3️⃣ 统一设置好 Tag / 名字，方便别的脚本用 Find
        player.tag = "Player";
        player.name = "Player";

        // 4️⃣ 把 Transform 递给 TileManager
        tileManager.player = player.transform;

        mapGenerator.GenerateMap();

        if (tileManager != null)
            tileManager.RefreshTiles();

        if (plantSpawner != null)
            plantSpawner.SpawnPlants();

        if (snakeSpawner != null)
            snakeSpawner.SpawnSnakes();

        Vector2Int spawn = mapGenerator.playerSpawnPoint;
        playerInitialPosition = new Vector3(
            spawn.x * tileManager.tileSize + tileManager.mapOffset.x,
            -spawn.y * tileManager.tileSize + tileManager.mapOffset.y,
            0f
        );

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

        ShowTimePenalty(20);
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

    public void ShowTimePenalty(int seconds)
    {
        if (penaltyText == null || penaltyCanvasGroup == null)
            return;

        penaltyText.text = $"Collected wrong plant! Time penalty -{seconds}s";
        penaltyText.gameObject.SetActive(true);
        penaltyCanvasGroup.alpha = 1f;

        StartCoroutine(FadePenaltyText());
    }

    private IEnumerator FadePenaltyText()
    {
        float showDuration = 1.8f;
        float fadeDuration = 0.5f;

        yield return new WaitForSeconds(showDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            penaltyCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        penaltyCanvasGroup.alpha = 0f;
        penaltyText.gameObject.SetActive(false);
    }

    public void GoToTreatment()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TreatmentScene-1");
    }

    public Transform PlayerTransform => player != null ? player.transform : null;
    private IEnumerator CheckIfPlayerSurvives()
    {
        yield return new WaitForSeconds(0.1f);

        if (player == null)
        {
            Debug.LogError("❌ Player was destroyed after instantiation!");
        }
        else
        {
            Debug.Log("✅ Player still exists after 0.1s");
        }
    }
}
