using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    Fader FaderScript;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;

        FaderScript = gameObject.AddComponent<Fader>();
    }

    public float deltaTime { get; private set; }
    public float fixedDeltaTime { get; private set; }

    public RoundState RoundState { get; private set; }
    public GameState GameState { get; private set; }

    [Header("Input Manager")]
    public string Vertical = "Vertical";
    public string Fire = "Fire";
    public string Item = "Item";
    public string Flipper = "Flipper";
    public string Pause = "Cancel";

    [Header("UI")]
    public Image fader;
    public GameObject pauseMenu;
    public TMP_Text text;
    public string RoundText { get; private set; } = "Round ";
    public string RoundDrawText { get; private set; } = "DRAW!";
    public string PlayerPrefixText { get; private set; } = "Player ";
    public string RoundEndText { get; private set; } = " wins the round!";
    public string LifeLostText { get; private set; } = "You lost a life!";
    public string GameEndText { get; private set; } = " wins the game!";
    public string GameOverText { get; private set; } = "Game Over...";

    public Color blackColor { get; private set; } = new Color(0, 0, 0, 1);
    public Color redColor { get; private set; } = new Color(1, 0.5f, 0.5f, 1);
    public Color blueColor { get; private set; } = new Color(0.5f, 1, 1, 1);

    [Header("Camera")]
    private Camera MainCamera;
    private Vector3 BasePosition = new Vector3(0f,0f,-10f);
    private bool isCameraShaking = false;
    private float currentShakePower;
    private float cameraReturnSpeed = 2f;

    [Header("Players")]
    public float racketSpeed = 15f;
    public float maxRacketHeight = 3.5f;
    public float racketXPosFromOrigin = -7f;
    public float racketYPosFromOrigin = -0.5f;
    public Player[] Players { get; private set; } = new Player[2];
    public BoxCollider2D[] PlayerGoals = new BoxCollider2D[2];
    public Color AIColor = Color.gray;
    public Color HiHealthColor;
    public Color MedHealthColor;
    public Color LoHealthColor;

    [Header("Ball")]
    public Pongball Ball;
    public float baseBallSpeed = 5f;
    public float maxBallSpeed = 20f;
    public float reflectDampening { get; private set; } = 2.5f;

    [Header("Score")]
    public int scoreToWin = 3;
    public int currentRound { get; private set; } = 0;
    public float RoundStartDelay = 1;
    public float RoundEndDelay = 3;
    //public float timeBeforeNextRound = 1f;

    [Header("Level")]
    public GameMode gameMode;
    private BumperMove MiddleBumper;
    public BoxCollider2D PlayZone { get; private set; }

    #region Start

    // Unity Start
    private void Start()
    {
        InitAttributes();
        FaderScript.OutFade(fader);
        StartCoroutine(InitGameDelayer());
    }

    // Initialize attributes and sets them to default parameters
    void InitAttributes()
    {
        PlayZone = GetComponentInChildren<BoxCollider2D>();
        MainCamera = Camera.main;

        MiddleBumper = FindAnyObjectByType<BumperMove>();
        SetScoreText("");
        PauseGame(false);
    }

    // Lets other scripts initialize properly
    IEnumerator InitGameDelayer()
    {
        yield return new WaitForSeconds(.01f);

        InitGame();
    }

    // Start the game once the players and the ball are ready
    public void InitGame()
    {
        if (InitPlayers() && InitBall()) StartGame();
    }

    // Get players and set them to the Game Manager
    bool InitPlayers()
    {
        Player[] players = FindObjectsOfType<Player>();

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i])
            {
                if (players[i].playerTeam == PlayerTeam.Player1) Players[0] = players[i];
                else if (players[i].playerTeam == PlayerTeam.Player2) Players[1] = players[i];
            }
        }

        return true;
    }

    // Get the ball
    bool InitBall()
    {
        return Ball = FindObjectOfType<Pongball>();
    }

    // Sets the game according to the selected gamemode
    public void StartGame()
    {
        SetScoreText("");
        ToggleCursor(false);

        if (!Players[0].IsInit || !Ball.IsInit) return;

        switch (gameMode)
        {
            case GameMode.Versus:
                Ball.BallReset(Ball.RandomizeOwner());
                Players[0].SetScore(0);
                Players[1].SetScore(0);
                break;
            case GameMode.Solo:
                Ball.BallReset(Players[0].racket);
                Players[0].SetScore(3);
                break;
            default:
                break;
        }

        SetRound(0);
        InitRound();
    }

    // Starts a round
    public void InitRound()
    {
        SetRound(currentRound + 1);

        Players[0].ResetPlayer();
        if (Players[1] != null) Players[1].ResetPlayer();
        if (MiddleBumper) MiddleBumper.ResetBumper();

        switch (gameMode)
        {
            case GameMode.Solo:
                SetScoreText("");
                break;
            case GameMode.Versus:
                SetScoreText(RoundText + currentRound);
                break;
            default:
                break;
        }

        StartCoroutine(InitRoundDelayer());
    }

    IEnumerator InitRoundDelayer()
    {
        yield return new WaitForSeconds(RoundStartDelay);

        RoundState = RoundState.Play;
    }

    #endregion

    #region Update

    private void Update()
    {
        deltaTime = Time.deltaTime;

        CameraShakeMovement();

        PauseMenuInput();
    }

    // Plays when Camera Shake is called
    private void CameraShakeMovement()
    {
        if (GameState == GameState.Paused) return;

        if (isCameraShaking)
        {
            MainCamera.transform.position = BasePosition +
            new Vector3(Random.Range(-0.1f, 0.1f) * currentShakePower,
                        Random.Range(-0.1f, 0.1f) * currentShakePower,
                        0f);
        }
        else MainCamera.transform.position = Vector3.MoveTowards(
            MainCamera.transform.position,
            BasePosition, deltaTime * cameraReturnSpeed);
    }

    // Look for input press
    private void PauseMenuInput()
    {
        if (Input.GetButtonDown(Pause)) PauseGame(GameState == GameState.Active ? true : false);
    }

    #endregion

    #region Fixed Update

    private void FixedUpdate()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

    #endregion

    #region UI

    // Pause Menu Button (Main Menu)
    public void LoadMainMenu()
    {
        PauseGame(false);
        FaderScript.InFade(fader, "MainMenu");
    }

    // Pause Menu Button (Restart)
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Pause Menu Button (Exit)
    public void ExitGame()
    {
        Application.Quit();
    }

    // Set score UI text to game's current score
    public void SetScoreText(string textPass)
    {
        text.text = textPass;
    }

    #endregion

    #region Invoke Methods

    // Force Round parameter
    public void SetRound(int roundPass)
    {
        currentRound = roundPass;
    }

    // Trail Reset Invoke
    public void ResetTrail(TrailRenderer trail)
    {
        float trailTime = trail.time;
        trail.time = -1f;

        if (gameObject.activeSelf) StartCoroutine(TrailEnabler(trail, trailTime));
    }

    public IEnumerator TrailEnabler(TrailRenderer trail, float trailTime)
    {
        yield return new WaitForSeconds(0.04f);

        trail.time = trailTime;
    }

    // Camera Shake Invoke
    public void ShakeCamera(float shakePower, float shakeDuration)
    {
        if (gameObject.activeSelf) StartCoroutine(CameraShaker(shakePower, shakeDuration));
    }

    IEnumerator CameraShaker(float shakePower, float shakeDuration)
    {
        currentShakePower = shakePower;
        isCameraShaking = true;

        yield return new WaitForSeconds(shakeDuration);

        isCameraShaking = false;
    }

    // End of a game, returns to the main menu
    public void EndGame()
    {
        RoundState = RoundState.Wait;

        Debug.Log("Game has ended. Returning to menu in " + RoundEndDelay + " seconds...");
        StartCoroutine(EndGameTimer());
    }

    IEnumerator EndGameTimer()
    {
        yield return new WaitForSeconds(RoundEndDelay);

        LoadMainMenu();
    }

    // Invoked from outside, handles the end of a round
    public IEnumerator RoundEnd(Player winner)
    {
        Player nextServicer = winner;

        switch (gameMode)
        {
            case GameMode.Versus:
                SetScoreText(PlayerPrefixText + (int)winner.playerTeam + RoundEndText);
                if (winner != null) nextServicer = winner == Players[0] ? Players[1] : Players[0];
                else nextServicer = Ball.RandomizeOwner().player;
                break;
            case GameMode.Solo:
                if (Players[0].currentScore > 0) SetScoreText(LifeLostText);
                nextServicer = Players[0];
                break;
            default:
                break;
        }

        RoundState = RoundState.Wait;

        yield return new WaitForSeconds(RoundEndDelay);

        Ball.BallReset(nextServicer.racket);
        InitRound();
    }

    // Pauses or unpauses the game
    public void PauseGame(bool state)
    {
        ToggleCursor(state);

        string print = state == true ? "The game is now paused." : "Resuming game...";
        Debug.Log(print);
        Time.timeScale = state == true ? 0 : 1;
        pauseMenu.SetActive(state);
        GameState = state == true ? GameState.Paused : GameState.Active;
    }

    // Sets Mouse Cursor visibility
    public static void ToggleCursor(bool state)
    {
        Cursor.visible = state;
        //Cursor.lockState = state == true ? CursorLockMode.Locked : CursorLockMode.None;
    }

    #endregion

}

public enum RoundState { Wait, Play }
public enum GameState { Active, Paused }
public enum GameMode { Versus, Solo }
