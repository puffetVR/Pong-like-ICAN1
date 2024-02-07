using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Base
{
    public bool fire { get; private set; }
    public bool item { get; private set; }
    public bool flipper { get; private set; }
    public float moveAxis { get; private set; }

    [Header("Flags")]
    public ControllerState controllerState;
    public PlayerTeam playerTeam;
    public int currentScore { get; private set; }

    [Header("References")]
    public Racket racket;
    public Flipper flipper1;
    public Flipper flipper2;
    public PlayerWall wall;

    private void Start()
    {
        InitAttributes();
    }

    private void InitAttributes()
    {
        racket.SetPlayerOwner(this);
        flipper1.SetPlayerOwner(this);
        flipper2.SetPlayerOwner(this);
        wall.SetPlayerOwner(this);

        racket.ColorSetter();
        flipper1.ColorSetter();
        flipper2.ColorSetter();
    }

    private void Update()
    {
        PlayerInput();
        if (controllerState == ControllerState.AI) WaitForPlayerInput();
    }

    public void SetScore(int score)
    {
        currentScore = score;
    }

    public void Score()
    {
        if (currentScore >= Game.scoreToWin)
        {
            Debug.Log("Player " + (int)playerTeam + " wins the round !");
            Game.EndGame();
        }
        else currentScore++;
    }

    private void PlayerInput()
    {
        moveAxis = Input.GetAxis(Game.Vertical + (int)playerTeam);
        fire = Input.GetButtonDown(Game.Fire + (int)playerTeam);
        if (fire) Debug.Log("Player " + (int)playerTeam + " fired.");
        item = Input.GetButtonDown(Game.Item + (int)playerTeam);
        if (item) Debug.Log("Player " + (int)playerTeam + " used their item.");
        flipper = Input.GetButton(Game.Flipper + (int)playerTeam);
        if (flipper) Debug.Log("Player " + (int)playerTeam + " used their flipper.");

        switch (controllerState)
        {
            case ControllerState.None:
                controllerState = ControllerState.AI;
                break;
            case ControllerState.Player:
                racket.isPlayerControlled = true;
                flipper1.isPlayerControlled = true;
                flipper2.isPlayerControlled = true;
                break;
            case ControllerState.AI:
                racket.isPlayerControlled = false;
                flipper1.isPlayerControlled = false;
                flipper2.isPlayerControlled = false;
                AIController();
                break;
        }
    }

    private void AIController()
    {

    }

    public void ResetPlayer()
    {
        racket.RacketReset();
        wall.WallReset();
        //flipper reset
        SetScore(0);
    }

    private void WaitForPlayerInput()
    {
        // Gives Racket control to Player over AI
        controllerState = fire ? ControllerState.Player : controllerState;
        racket.ColorSetter();
        flipper1.ColorSetter();
        flipper2.ColorSetter();
    }

}

public enum PlayerTeam { None, Player1, Player2 };
public enum ControllerState { None, Player, AI }