using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
    }

    public float deltaTime { get; private set; }
    public float fixedDeltaTime { get; private set; }

    [Header("Input Manager")]
    public string Vertical = "Vertical";
    public string Fire = "Fire";
    public string Item = "Item";
    public string Flipper = "Flipper";

    public BoxCollider2D PlayZone { get; private set; }

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
    public Color Player1Color = Color.cyan;
    public Color Player2Color = Color.red;
    public Color AIColor = Color.gray;
    public PlayerWall[] PlayerWalls { get; private set; } = new PlayerWall[2];
    public Color HiHealthColor;
    public Color MedHealthColor;
    public Color LoHealthColor;
    [Header("Ball")]
    public Pongball Ball;
    public float timeBeforeBallMove = 1f;
    public float baseBallSpeed = 5f;
    public float maxBallSpeed = 20f;
    public float reflectDampening { get; private set; } = 2.5f;

    [Header("Score")]
    public int scoreToWin = 3;
    public float timeBeforeGameStart = 3;

    private void Start()
    {
        InitAttributes();
        StartCoroutine(InitGameDelayer());
    }

    IEnumerator InitGameDelayer()
    {
        yield return new WaitForSeconds(0.1f);

        InitGame();
    }

    private void Update()
    {
        deltaTime = Time.deltaTime;

        CameraShakeMovement();
    }

    private void FixedUpdate()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

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

    private void CameraShakeMovement()
    {
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

    public void InitGame()
    {
        InitPlayers();
        InitBall();

        StartGame();
    }

    void InitAttributes()
    {
        PlayZone = GetComponentInChildren<BoxCollider2D>();
        MainCamera = Camera.main;

        PlayerWalls = FindObjectsOfType<PlayerWall>();
    }

    void InitPlayers()
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
    }

    void InitBall()
    {
        Ball = FindObjectOfType<Pongball>();
    }

    public void ResetTrail(TrailRenderer trail)
    {
        float trailTime = trail.time;
        trail.time = -1f;

        if (gameObject.activeSelf) StartCoroutine(TrailEnabler(trail, trailTime));
    }

    public IEnumerator TrailEnabler(TrailRenderer trail, float trailTime)
    {
        yield return new WaitForSecondsRealtime(0.04f);

        trail.time = trailTime;
    }

    public void StartGame()
    {
        if (!Players[1].IsInit || !Players[0].IsInit || !Ball.IsInit) return;
       
        Ball.BallReset(Ball.RandomizeOwner());
        InitRound();
    }

    public void InitRound()
    {
        Players[0].ResetPlayer();
        Players[1].ResetPlayer();
    }

    public void EndGame()
    {
        Debug.Log("Game has ended. Restarting in " + timeBeforeGameStart + " seconds...");
        StartCoroutine(EndGameTimer());
    }

    IEnumerator EndGameTimer()
    {
        yield return new WaitForSeconds(timeBeforeGameStart);

        StartGame();
    }
    public IEnumerator RoundEnd(Player winner)
    {
        yield return new WaitForSeconds(timeBeforeBallMove);

        Player otherPlayer;

        if (winner != null) otherPlayer = winner == Players[0] ? Players[1] : Players[0];
        else otherPlayer = Ball.RandomizeOwner().player;

        Ball.BallReset(otherPlayer.racket);
        InitRound();
    }
}

//public enum GameState { }
