using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    public Image[] scoreSprites;
    public Color playerColor;

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

        playerColor.a = 1;
        if (playerColor == Game.blackColor) playerColor = playerTeam == PlayerTeam.Player1 ? Game.blueColor : Game.redColor;

        racket.ColorSetter(playerColor);
        flipper1.ColorSetter(playerColor);
        flipper2.ColorSetter(playerColor);

        ScoreResetter();
    }

    private void ScoreResetter()
    {
        for (int i = 0; i < scoreSprites.Length; i++)
        {
            scoreSprites[i].color = playerColor;
            scoreSprites[i].enabled = false;
        }
    }

    private void Update()
    {
        PlayerInput();
        if (controllerState == ControllerState.AI) WaitForPlayerInput();
    }

    public void SetScore(int score)
    {
        currentScore = score;

        if (currentScore > 0)
        {
            if (scoreSprites[currentScore - 1] != null) scoreSprites[currentScore - 1].enabled = true;
        }

        if (currentScore == 0) ScoreResetter();

    }

    public void Score()
    {
        SetScore(currentScore + 1);

        if (currentScore >= Game.scoreToWin)
        {
            Debug.Log("Player " + (int)playerTeam + " wins the round !");
            Game.EndGame();
        }

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
    }

    private void WaitForPlayerInput()
    {
        // Gives Racket control to Player over AI
        if (fire || moveAxis != 0 || flipper) controllerState = ControllerState.Player;

        racket.ColorSetter(playerColor);
        flipper1.ColorSetter(playerColor);
        flipper2.ColorSetter(playerColor);
    }

}

public enum PlayerTeam { None, Player1, Player2 };
public enum ControllerState { None, Player, AI }