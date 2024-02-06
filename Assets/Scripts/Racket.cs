using UnityEngine;

public class Racket : Base
{
    public bool IsControllable { get; private set; }

    [Header("Flags")]
    public RacketTeam racketTeam;
    public ControllerState controllerState;
    public int currentScore {  get; private set; }
    [Header("References")]
    public Rigidbody2D body;
    public SpriteRenderer racketSprite;
    public TrailRenderer racketTrail;
    public Color racketColor { get; private set; }

    [Header("Inputs")]
    private bool fire;
    private bool item;
    private bool flipper;
    public float moveAxis { get; private set; }

    [Header("Vectors")]
    private Vector2 moveVector;

    
    void Start()
    {
        InitAttributes();
    }

    private void InitAttributes()
    {
        IsControllable = false;
    }

    public void ColorSetter()
    {
        Color color = Game.AIColor;

        switch (controllerState)
        {
            case ControllerState.Player:
                color = racketTeam == RacketTeam.Player1 ? Game.Player1Color : Game.Player2Color;
                break;
            case ControllerState.AI:
                color = Game.AIColor;
                break;
        }

        color.a = 1;

        racketColor = color;
        racketSprite.color = color;
        racketTrail.startColor = color;
        racketTrail.endColor = color;
    }

    public void IsControllableSet(bool state)
    {
        IsControllable = state;
    }

    void Update()
    {
        if (IsInit == false) return;

        PlayerInput();

        switch (controllerState)
        {
            case ControllerState.None:
                controllerState = ControllerState.AI;
                break;
            case ControllerState.Player:
                PlayerMovement();
                break;
            case ControllerState.AI:
                WaitForPlayerInput();
                AIController();
                break;
        }

    }

    private void WaitForPlayerInput()
    {
        // Gives Racket control to Player over AI
        controllerState = fire ? ControllerState.Player : controllerState;
        ColorSetter();
    }

    private void PlayerInput()
    {
        moveAxis = Input.GetAxis(Game.Vertical + (int)racketTeam);
        fire = Input.GetButtonDown(Game.Fire + (int)racketTeam);
        if (fire) Debug.Log("Player " + (int)racketTeam + " fired.");
        item = Input.GetButtonDown(Game.Item + (int)racketTeam);
        if (item) Debug.Log("Player " + (int)racketTeam + " used their item.");
        flipper = Input.GetButtonDown(Game.Flipper + (int)racketTeam);
        if (flipper) Debug.Log("Player " + (int)racketTeam + " used their flipper.");
    }

    private void PlayerMovement()
    {
        body.velocity = new Vector2(0f, moveAxis * Game.racketSpeed);
    }

    private void AIController()
    {

    }

    public void Score()
    {
        if (currentScore >= Game.scoreToWin)
        {
            Debug.Log("Player " + (int)racketTeam + " wins the round !");
            Game.EndGame();
        }
        else currentScore++;
    }

    public void RacketReset()
    {
        body.velocity = Vector2.zero;
        
        float pos = Game.racketXPosFromOrigin;
        switch (racketTeam)
        {
            case RacketTeam.Player1:
                body.position = new Vector2(pos, 0f);
                break;
            case RacketTeam.Player2:
                body.position = new Vector2(-pos, 0f);
                break;
            default:
                break;
        }

        Game.ResetTrail(racketTrail);
        SetScore(0);
    }

    public void SetScore(int score)
    {
        currentScore = score;
    }

}

public enum RacketTeam { None, Player1, Player2 };
public enum ControllerState { None, Player, AI }