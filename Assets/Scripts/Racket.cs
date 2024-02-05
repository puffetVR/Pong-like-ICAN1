using UnityEngine;

public class Racket : Base
{
    public bool IsControllable { get; private set; }

    [Header("Flags")]
    public RacketTeam racketTeam;
    public ControllerState controllerState;

    [Header("References")]
    Rigidbody2D body;

    [Header("Inputs")]
    private float moveAxis;
    private bool fire;
    private bool item;
    private bool flipper;

    [Header("Vectors")]
    private Vector2 moveVector;

    void Start()
    {
        InitAttributes();
        PassReferences();
    }

    private void InitAttributes()
    {
        IsControllable = false;
        body = GetComponent<Rigidbody2D>();
    }

    private void PassReferences()
    {
        switch (racketTeam)
        {
            case RacketTeam.Player1:
                Game.LeftRacket = this;
                break;
            case RacketTeam.Player2:
                Game.RightRacket = this;
                break;
            default:
                break;
        }
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

}

public enum RacketTeam { None, Player1, Player2 };
public enum ControllerState { None, Player, AI }