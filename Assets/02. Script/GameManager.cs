using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region UI Prefabs
    [Header("UI Prefabs")]
    public GameObject gameOverCanvasPrefab;
    public GameObject IngameCanvasPrefab;
    #endregion

    #region UI Components
    [Header("Game Over UI")]
    private GameObject gameOverCanvas;
    private TMP_Text gameOverScoreText;
    private Button homeButton;
    private Button restartButton;
    private Button quitButton;

    [Header("In-Game UI")]
    private GameObject IngameCanvas;
    private TMP_Text timerText;
    private TMP_Text scoreText;
    private Button startButton;
    #endregion

    #region Game State
    [Header("Game Data")]
    private int score = 0;
    public string currentSceneName;

    [Header("Timer Settings")]
    private Coroutine timerCoroutine;
    private float timeLimit=10f;                        // ���� �ð� ���� (�� ����)
    private bool isTimeOut = false;

    public static event Action GameStartEvent;          // ���� ���� �̺�Ʈ
    public static event Action OnTimerFinishedEvent;    // Ÿ�̸� ���� �̺�Ʈ
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeUI();
        InitializeGame();
        SetupButtons();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeUI()     // UI �ʱ�ȭ
    {
        // Game Over Canvas �ʱ�ȭ
        gameOverCanvas = Instantiate(gameOverCanvasPrefab);     // Game Over UI �������� �ν��Ͻ�ȭ
        gameOverCanvas.transform.SetParent(this.transform);     // �θ� GameManager�� ����(��� ����ϱ� ����)
        gameOverCanvas.SetActive(false);                        // Game Over UI ��Ȱ��ȭ
        gameOverScoreText = gameOverCanvas.transform.Find("GameOverScoreText").GetComponent<TMP_Text>();    // ���� �ؽ�Ʈ ������Ʈ ��������

        // In-Game Canvas �ʱ�ȭ
        IngameCanvas = Instantiate(IngameCanvasPrefab);         // In-Game UI �������� �ν��Ͻ�ȭ
        IngameCanvas.transform.SetParent(this.transform);       // �θ� GameManager�� ����(��� ����ϱ� ����)
        IngameCanvas.SetActive(false);                          // In-Game UI ��Ȱ��ȭ
        timerText = IngameCanvas.transform.Find("TimerText").GetComponent<TMP_Text>();      // Ÿ�̸� �ؽ�Ʈ ������Ʈ ��������
        scoreText = IngameCanvas.transform.Find("ScoreText").GetComponent<TMP_Text>();      // ���� �ؽ�Ʈ ������Ʈ ��������
        startButton = IngameCanvas.transform.Find("StartButton").GetComponent<Button>();    // ���� ��ư ������Ʈ ��������
        startButton.onClick.AddListener(() =>       // ���� ���� ��ư Ŭ�� �� �̺�Ʈ ó��
        {
            StartTimer(timeLimit);                  // Ÿ�̸� ����
            GameStartEvent?.Invoke();               // ���� ���� �̺�Ʈ ȣ��
            startButton.gameObject.SetActive(false);// ���� ��ư ��Ȱ��ȭ
        });
    }

    private void SetupButtons()     // ���� ���� UI ��ư ����
    {
        homeButton = gameOverCanvas.transform.Find("ButtonHome").GetComponent<Button>();        // Ȩ ��ư ������Ʈ ��������
        restartButton = gameOverCanvas.transform.Find("ButtonRestart").GetComponent<Button>();  // ����� ��ư ������Ʈ ��������
        quitButton = gameOverCanvas.transform.Find("ButtonQuit").GetComponent<Button>();        // ���� ��ư ������Ʈ ��������

        homeButton.onClick.AddListener(() => ChangeScene("MainMenu"));                          // Ȩ ȭ������ �̵�
        restartButton.onClick.AddListener(RestartGame);                                         // ���� �����
        quitButton.onClick.AddListener(QuitGame);                                               // ���� ����
    }

    private void InitializeGame()   // ���� ó�� ������ �� �ʱ�ȭ �ϴ� ����
    {
        currentSceneName = SceneManager.GetActiveScene().name;      // ���� �� �̸� ����
        ResetStage();                                               // ���� ���� �ʱ�ȭ
    }
    #endregion

    #region Scene Management
    public void ChangeScene(string sceneName)
    {
        currentSceneName = sceneName;
        ResetStage();
        SceneManager.LoadScene(currentSceneName);
    }

    public void RestartGame()
    {
        ResetStage();
        ChangeScene(currentSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Game Control
    public void GameOver()
    {
        startButton.gameObject.SetActive(true);
        IngameCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        Debug.Log("Game Over");
    }

    private void ResetStage()
    {
        
        // ���� ���� �ʱ�ȭ
        score = 0;
        isTimeOut = false;
        gameOverCanvas.SetActive(false);

        // UI ������Ʈ
        UpdateScoreUI();

        // �̺�Ʈ �ʱ�ȭ
        OnTimerFinishedEvent = null;

        // Ÿ�̸� �ʱ�ȭ
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        // UI ���� ����
        if (currentSceneName != "MainMenu")
        {
            IngameCanvas.SetActive(true);
        }

    }
    #endregion

    #region Score Management
    public void ScoreUp()
    {
        score++;
        UpdateScoreUI();
        Debug.Log("Score: " + score);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        if(timerText != null)
            timerText.text = "���� �ð�: " + timeLimit + "��";
    }
    #endregion

    #region Timer System
    public void StartTimer(float seconds)
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timeLimit = seconds;
        timerCoroutine = StartCoroutine(TimerCoroutine(seconds));
    }

    private IEnumerator TimerCoroutine(float delay)
    {
        float timeLeft = delay;

        while (timeLeft > 0)
        {
            UpdateTimerUI(timeLeft);
            yield return new WaitForSeconds(0.1f);
            timeLeft -= 0.1f;
        }
        if(IngameCanvas.activeSelf)
            IngameCanvas.SetActive(false);
        UpdateTimerUI(0);
        OnTimerFinishedEvent?.Invoke();
        HandleTimerFinished();
    }

    private void UpdateTimerUI(float timeLeft)
    {
        if (timerText != null)
        {
            if (timeLeft > 0)
                timerText.text = $"���� �ð�: {timeLeft:F1}��";
            else
                timerText.text = "0.0��";
        }
    }

    private void HandleTimerFinished()
    {
        isTimeOut = true;
        StopTimer();
        gameOverScoreText.text = "Score: " + score;
        GameOver();
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (timerText != null)
            timerText.text = "";
    }
    #endregion

    #region Properties
    public int CurrentScore => score;
    public bool IsTimedOut => isTimeOut;
    public string CurrentSceneName => currentSceneName;
    #endregion
}