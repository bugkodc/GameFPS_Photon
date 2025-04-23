using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// GameState: Defines the possible states of the game.
/// GameState: Định nghĩa các trạng thái có thể của game.
/// </summary>
public enum GameState
{
    inGame,
    pause,
    gameOver,
    menu,
    shop
}

/// <summary>
/// GameManager: Quản lý trạng thái game, sinh quái, xử lý UI và điều khiển luồng chơi.
/// GameManager: Manages game states, spawns enemies, handles UI, and controls game flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private string menuScene, mainScene;
    [SerializeField] private int maxFrames = 90;

    [Header("Spawners")]
    [SerializeField] private GameObject[] spawners;
    [SerializeField] private GameObject[] spawnersBoss;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI roundsSurvivedText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject fadeInGamePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Prefabs")]
    [SerializeField] private GameObject zombiePrefab;

    [Header("Game Variables")]
    public int currentRound;
    public GameObject erorBoss;
    public GameObject recorder;
    public GameObject muteRecorder;
    public GameObject keyE;
    [HideInInspector] public VendingMachine vendingMachine;
    private GameState currentLocalGameState;
    public GameState CurrentLocalGameState => currentLocalGameState;
    public GameState currentOnlineGameState;
    private bool isOnlineMasterAndMine;
    private int numberSpawnBoss = 0;


    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxFrames;
        spawners = GameObject.FindGameObjectsWithTag("Spawner");
        spawnersBoss = GameObject.FindGameObjectsWithTag("SpawnerBoss");
        StartGame();
    }

    void StartGame()
    {
        currentLocalGameState = GameState.inGame;
        StartCoroutine(FadeInOrOutPanel(fadeInGamePanel, 3f, false, false));
        StartCoroutine(StartNextRound());
    }

    void Update()
    {
        if (Input.GetButtonDown("Pause"))
            BackGame();
    }

    public void BackGame()
    {
        switch (currentLocalGameState)
        {
            case GameState.pause:
                Resume();
                break;
            case GameState.inGame:
                Pause();
                break;
            case GameState.shop:
                vendingMachine.ExitShop();
                break;
        }
    }

    void SetRound(int round)
    {
        currentRound = round;
        roundText.text = $"Round: {currentRound}";
    }

    public IEnumerator StartNextRound()
    {
        Debug.Log("StartNextRound");
        SetRound(currentRound + 1);
        yield return new WaitForSeconds(2f);

        if (currentRound % 5 == 0)
        {
            StartCoroutine(ShowEror());
            numberSpawnBoss++;
            int idx = Random.Range(0, spawnersBoss.Length);
            InstantiateZombieBoss(false, idx);
        }
        else
        {
            for (int i = 0; i < currentRound; i++)
            {
                int idx = Random.Range(0, spawners.Length);
                InstantiateZombie(false, idx);
            }
        }
    }

    IEnumerator ShowEror()
    {
        erorBoss.SetActive(true);
        yield return new WaitForSeconds(3f);
        erorBoss.SetActive(false);
    }

    public void InstantiateZombie(bool isOnline, int spawnIndex)
    {
        GameObject enemy = Instantiate(zombiePrefab, spawners[spawnIndex].transform.position, Quaternion.identity);
        if (enemy) enemy.GetComponent<ZombieManager>().gameManager = this;
    }

    public void InstantiateZombieEnenmy(bool isOnline, int spawnIndex, GameObject[] spawnerArray)
    {
        GameObject enemy = Instantiate(zombiePrefab, spawnerArray[spawnIndex].transform.position, Quaternion.identity);
        if (enemy) enemy.GetComponent<ZombieManager>().gameManager = this;
    }

    public void InstantiateZombieBoss(bool isOnline, int spawnIndex)
    {
        GameObject boss = Instantiate(Resources.Load("Boss"), spawnersBoss[spawnIndex].transform.position, Quaternion.identity) as GameObject;
        if (boss)
        {
            var zm = boss.GetComponent<ZombieManager>();
            zm.gameManager = this;
            zm.maxHealth = 1000 * numberSpawnBoss;
        }
    }

    public void LookForEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            var zm = enemy.GetComponent<ZombieManager>();
            if (zm != null && zm.isAlive)
                return;
        }
        StartCoroutine(StartNextRound());
    }
    public void GameOver()
    {
        currentLocalGameState = GameState.gameOver;
        StartCoroutine(FadeInOrOutPanel(gameOverPanel, 2f, true, true));
        roundsSurvivedText.text = $"ROUNDS SURVIVED: {currentRound}";
    }

    IEnumerator FadeInOrOutPanel(GameObject panel, float time, bool fadeIn, bool stopTime)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        cg.gameObject.SetActive(true);

        if (fadeIn)
            while (cg.alpha < 1f) { cg.alpha += Time.deltaTime / time; yield return null; }
        else
            while (cg.alpha > 0f) { cg.alpha -= Time.deltaTime / time; yield return null; }

        if (stopTime)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        currentLocalGameState = GameState.pause;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        currentLocalGameState = GameState.menu;
        SceneManager.LoadScene(menuScene);
    }

    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        currentLocalGameState = GameState.pause;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }


    public void Shop()
    {
        Cursor.lockState = CursorLockMode.None;
        currentLocalGameState = GameState.shop;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentLocalGameState = GameState.inGame;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void DestroyPlayerGO()
    {
        Destroy(gameObject);
    }
}
