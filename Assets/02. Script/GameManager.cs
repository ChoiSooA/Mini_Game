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
    private float timeLimit=10f;                        // 게임 시간 제한 (초 단위)
    private bool isTimeOut = false;

    public static event Action GameStartEvent;          // 게임 시작 이벤트
    public static event Action OnTimerFinishedEvent;    // 타이머 종료 이벤트
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

    private void InitializeUI()     // UI 초기화
    {
        // Game Over Canvas 초기화
        gameOverCanvas = Instantiate(gameOverCanvasPrefab);     // Game Over UI 프리팹을 인스턴스화
        gameOverCanvas.transform.SetParent(this.transform);     // 부모를 GameManager로 설정(계속 사용하기 위해)
        gameOverCanvas.SetActive(false);                        // Game Over UI 비활성화
        gameOverScoreText = gameOverCanvas.transform.Find("GameOverScoreText").GetComponent<TMP_Text>();    // 점수 텍스트 컴포넌트 가져오기

        // In-Game Canvas 초기화
        IngameCanvas = Instantiate(IngameCanvasPrefab);         // In-Game UI 프리팹을 인스턴스화
        IngameCanvas.transform.SetParent(this.transform);       // 부모를 GameManager로 설정(계속 사용하기 위해)
        IngameCanvas.SetActive(false);                          // In-Game UI 비활성화
        timerText = IngameCanvas.transform.Find("TimerText").GetComponent<TMP_Text>();      // 타이머 텍스트 컴포넌트 가져오기
        scoreText = IngameCanvas.transform.Find("ScoreText").GetComponent<TMP_Text>();      // 점수 텍스트 컴포넌트 가져오기
        startButton = IngameCanvas.transform.Find("StartButton").GetComponent<Button>();    // 시작 버튼 컴포넌트 가져오기
        startButton.onClick.AddListener(() =>       // 게임 시작 버튼 클릭 시 이벤트 처리
        {
            StartTimer(timeLimit);                  // 타이머 시작
            GameStartEvent?.Invoke();               // 게임 시작 이벤트 호출
            startButton.gameObject.SetActive(false);// 시작 버튼 비활성화
        });
    }

    private void SetupButtons()     // 게임 오버 UI 버튼 설정
    {
        homeButton = gameOverCanvas.transform.Find("ButtonHome").GetComponent<Button>();        // 홈 버튼 컴포넌트 가져오기
        restartButton = gameOverCanvas.transform.Find("ButtonRestart").GetComponent<Button>();  // 재시작 버튼 컴포넌트 가져오기
        quitButton = gameOverCanvas.transform.Find("ButtonQuit").GetComponent<Button>();        // 종료 버튼 컴포넌트 가져오기

        homeButton.onClick.AddListener(() => ChangeScene("MainMenu"));                          // 홈 화면으로 이동
        restartButton.onClick.AddListener(RestartGame);                                         // 게임 재시작
        quitButton.onClick.AddListener(QuitGame);                                               // 게임 종료
    }

    private void InitializeGame()   // 제일 처음 시작할 때 초기화 하는 내용
    {
        currentSceneName = SceneManager.GetActiveScene().name;      // 현재 씬 이름 저장
        ResetStage();                                               // 게임 상태 초기화
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
        
        // 게임 상태 초기화
        score = 0;
        isTimeOut = false;
        gameOverCanvas.SetActive(false);

        // UI 업데이트
        UpdateScoreUI();

        // 이벤트 초기화
        OnTimerFinishedEvent = null;

        // 타이머 초기화
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        // UI 상태 설정
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
            timerText.text = "남은 시간: " + timeLimit + "초";
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
                timerText.text = $"남은 시간: {timeLeft:F1}초";
            else
                timerText.text = "0.0초";
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