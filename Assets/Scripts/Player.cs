using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Player : Base
{

    [Header("Flags")]
    public ControllerState controllerState;
    public PlayerTeam playerTeam;
    public int currentScore { get; private set; }

    [Header("References")]
    public Racket racket;
    public Flipper flipper1;
    public Flipper flipper2;
    public PlayerWall[] walls;
    public GameObject[] scoreSprites;
    public Color playerColor;
    public bool fire { get; private set; }
    public bool item { get; private set; }
    public bool flipper { get; private set; }
    public float moveAxis { get; private set; }

    public float moveAmount { get; private set; }

    [Header("AI")]
    private float aiMoveDistance;
    private float offsetFromBall;
    public float aiMoveAmount { get; private set; }
    public bool aiFire { get; private set; } = false;
    public bool aiFlipper { get; private set; } = false;
    private bool canAiFire = true;
    private float timeUntilSmash = 5f;
    private float timeToSmash = 0f;
    private float aiMoveDist = 1f;
    private float timeUntilNextMoveRand = 1f;
    private float timeToNextMoveRand = 0f;
    private float moveNoise;
    private float timeUntilMoveDistReset = .2f;
    private float timeToMoveDistReset = 0f;


    private void Start()
    {
        InitAttributes();
    }

    private void InitAttributes()
    {
        racket.SetPlayerOwner(this);
        if (flipper1) flipper1.SetPlayerOwner(this);
        if (flipper2) flipper2.SetPlayerOwner(this);

        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == null) return;
            walls[i].SetPlayerOwner(this);
        }
        
        playerColor.a = 1;
        if (playerColor == Game.blackColor) playerColor = playerTeam == PlayerTeam.Player1 ? Game.blueColor : Game.redColor;

        racket.ColorSetter(playerColor);
        if (flipper1) flipper1.ColorSetter(playerColor);
        if (flipper2) flipper2.ColorSetter(playerColor);

        ScoreResetter();
    }

    private void ScoreResetter()
    {
        for (int i = 0; i < scoreSprites.Length; i++)
        {
            //scoreSprites[i].color = playerColor;
            //scoreSprites[i].enabled = false;
            scoreSprites[i].SetActive(false);
        }

    }

    private void Update()
    {
        if (!IsInit || Game.GameState == GameState.Paused) return;

        PlayerInput();
        if (controllerState == ControllerState.AI) WaitForPlayerInput();
        ControllerStates();

        if (Game.RoundState == RoundState.Wait)
        {
            aiMoveDistance = 0;
            aiMoveAmount = 0;
            racket.body.velocity = Vector3.zero;
            return;
        }
    }

    public void SetScore(int score)
    {
        currentScore = score;

        switch (currentScore)
        {
            case 0:
                scoreSprites[0].SetActive(false);
                scoreSprites[1].SetActive(false);
                scoreSprites[2].SetActive(false);
                break;
            case 1:
                scoreSprites[0].SetActive(true);
                scoreSprites[1].SetActive(false);
                scoreSprites[2].SetActive(false);
                break;
            case 2:
                scoreSprites[0].SetActive(true);
                scoreSprites[1].SetActive(true);
                scoreSprites[2].SetActive(false);
                break;
            case 3:
                scoreSprites[0].SetActive(true);
                scoreSprites[1].SetActive(true);
                scoreSprites[2].SetActive(true);
                break;
            default:
                break;
        }

        if (currentScore == 0) ScoreResetter();

    }

    public void Score(int scoreAmount)
    {
        SetScore(currentScore + scoreAmount);

        if (currentScore >= Game.scoreToWin)
        {
            Debug.Log("Player " + (int)playerTeam + " wins the game !");
            Game.SetScoreText(Game.PlayerPrefixText + (int)playerTeam + Game.GameEndText);
            Game.EndGame();
        }

        if (currentScore == 0 && Game.gameMode == GameMode.Solo)
        {
            Debug.Log("Player " + (int)playerTeam + " lost the game !");
            Game.SetScoreText(Game.GameOverText);
            Game.EndGame();
        }

    }

    private void PlayerInput()
    {
        moveAmount = controllerState == ControllerState.Player ? moveAxis : aiMoveAmount;

        moveAxis = Input.GetAxis(Game.Vertical + (int)playerTeam);
        fire = Input.GetButtonDown(Game.Fire + (int)playerTeam);
        if (fire) Debug.Log("Player " + (int)playerTeam + " fired.");
        item = Input.GetButtonDown(Game.Item + (int)playerTeam);
        if (item) Debug.Log("Player " + (int)playerTeam + " used their item.");
        flipper = Input.GetButton(Game.Flipper + (int)playerTeam);
        if (flipper) Debug.Log("Player " + (int)playerTeam + " used their flipper.");
    }

    private void ControllerStates()
    {

        switch (controllerState)
        {
            case ControllerState.None:
                controllerState = ControllerState.AI;
                break;
            case ControllerState.Player:
                racket.isPlayerControlled = true;
                if (flipper1) flipper1.isPlayerControlled = true;
                if (flipper2) flipper2.isPlayerControlled = true;
                break;
            case ControllerState.AI:
                racket.isPlayerControlled = false;
                if (flipper1) flipper1.isPlayerControlled = false;
                if (flipper2) flipper2.isPlayerControlled = false;
                AIController();
                break;
        }
    }

    private void AIController()
    {
        timeToSmash += Game.deltaTime;

        if (timeToSmash >= timeUntilSmash)
        {
            timeToNextMoveRand = 0f;
            aiMoveDist = 2f;
            timeToMoveDistReset = 0f;
            timeToSmash = 0f;
            timeUntilSmash = Random.Range(5f, 10f);
        }

        float randomOffset = Random.Range(0.3f, 0.7f);
        
        float randomDistance = Game.Ball.currentBallSpeed > 10 ? Random.Range(.2f, 4f) : Random.Range(.8f, 1.6f);

        timeToNextMoveRand += Game.deltaTime;
        if (timeToNextMoveRand >= timeUntilNextMoveRand)
        {
            moveNoise = Random.Range(-3f, 3f);
            timeUntilNextMoveRand = Random.Range(2f, 4f);
        }

        timeToMoveDistReset += Game.deltaTime;
        if (timeToMoveDistReset >= timeUntilMoveDistReset)
        {
            moveNoise = 0f;
            timeToMoveDistReset = 0f;
            aiMoveDist = Game.Ball.currentBallSpeed > 10 ? 1.5f : .5f;
        }

        // Move up or down depending on ball's Y pos
        offsetFromBall = -(racket.body.position.y - Game.Ball.body.position.y) + moveNoise;

        if (offsetFromBall > randomOffset) aiMoveDistance = aiMoveDist;
        if (offsetFromBall < -randomOffset) aiMoveDistance = -aiMoveDist;
        if (offsetFromBall < randomOffset && offsetFromBall > -randomOffset) aiMoveDistance = Random.Range(0f, .2f);

        aiMoveAmount = Mathf.Lerp(aiMoveAmount,
            aiMoveDistance * (Game.Ball.currentBallSpeed > 10 ? Game.Ball.currentBallSpeed / 10 : 1),
            Game.deltaTime * 5f);

        // Smash if ball close to racket
        if (Vector3.Distance(Game.Ball.body.position, racket.transform.position) < 2f)
        {
            if (Vector3.Distance(Game.Ball.body.position, racket.transform.position) < randomDistance
                || Game.Ball.IsMoving == false && Game.Ball.currentOwner == racket)
            {
                if (canAiFire) aiFire = true;
                StartCoroutine(AI_FireCooldown());
                StartCoroutine(AI_FireReset());
            }
        }

        // Flip if ball too close to flipper
        if (flipper1 != null && flipper2 != null)
        {

            if (Vector3.Distance(Game.Ball.body.position, flipper1.transform.position) < randomDistance
             || Vector3.Distance(Game.Ball.body.position, flipper2.transform.position) < randomDistance)
            {
                aiFlipper = true;
                StartCoroutine(AI_FlipperReset());
            }

        }

    }


    IEnumerator AI_FireReset()
    {
        yield return new WaitForSeconds(.02f);

        aiFire = false;
    }

    IEnumerator AI_FireCooldown()
    {
        float rand = 1f;
        if (canAiFire) rand = Random.Range(2.5f, 5f);
        canAiFire = false;

        yield return new WaitForSeconds(rand);

        canAiFire = true;
    }

    IEnumerator AI_FlipperReset()
    {
        yield return new WaitForSeconds(.1f);

        aiFlipper = false;
    }

    public void ResetPlayer()
    {
        racket.RacketReset();

        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == null) return;
            walls[i].WallReset();
        }

    }

    private void WaitForPlayerInput()
    {
        // Gives Racket control to Player over AI
        if (fire || moveAxis != 0 || flipper) controllerState = ControllerState.Player;

        racket.ColorSetter(playerColor);
        if (flipper1) flipper1.ColorSetter(playerColor);
        if (flipper2) flipper2.ColorSetter(playerColor);
    }

}

public enum PlayerTeam { None, Player1, Player2 };
public enum ControllerState { None, Player, AI }