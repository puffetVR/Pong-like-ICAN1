using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : Base
{
    public bool isPlayerControlled = false;
    public Player player { get; private set; }

    public Transform flipperPivot;
    public float restAngle = -15f, flippedAngle = 45f;
    public PlayerTeam team;

    public SpriteRenderer flipperSprite;
    public TrailRenderer flipperTrail;

    private float targetAngle = -15f;
    private float currentAngle = -15f;
    private bool canFlip = true;
    private bool canUnflip = true;

    // Start is called before the first frame update
    void Start()
    {
        InitAttributes();
    }


    private void InitAttributes()
    {

    }

    public void SetPlayerOwner(Player owner)
    {
        player = owner;
    }
    public void ColorSetter(Color color)
    {
        if (player.controllerState == ControllerState.AI) color = Game.AIColor;

        //Color color = Game.AIColor;

        //switch (player.controllerState)
        //{
        //    case ControllerState.Player:
        //        color = player.playerTeam == PlayerTeam.Player1 ? Game.Player1Color : Game.Player2Color;
        //        break;
        //    case ControllerState.AI:
        //        color = Game.AIColor;
        //        break;
        //}

        color.a = 1;

        flipperSprite.color = color;
        flipperTrail.startColor = color;
        flipperTrail.endColor = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.flipper && canFlip && !canUnflip && isPlayerControlled) Flip();
        FlipUpdate();
    }

    void Flip()
    {
        Debug.Log("Flip!");
        targetAngle = flippedAngle;
        flipperPivot.tag = "Bumper";
        if (gameObject.activeSelf) StartCoroutine(FlipDelay());
    }

    IEnumerator FlipDelay()
    {
        canFlip = false;

        yield return new WaitForSeconds(.7f);

        canUnflip = true;
        flipperPivot.tag = "Flipper";
    }

    private void FlipUpdate()
    {
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, 
            targetAngle == restAngle ? .5f : 1.5f);
        Quaternion angle = Quaternion.Euler(new Vector3(0f, 0f, currentAngle));
        flipperPivot.localRotation = angle;

        if (canUnflip && !player.flipper)
        {
            canUnflip = false;
            canFlip = true;
            targetAngle = restAngle;
        }
    }
}
