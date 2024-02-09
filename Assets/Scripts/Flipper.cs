using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : Base
{
    public bool isPlayerControlled = false;
    public Player player { get; private set; }
    public Rigidbody2D body;

    public Transform flipperPivot;
    public float restAngle = -15f, flippedAngle = 45f;
    public PlayerTeam team;

    public SpriteRenderer flipperSprite;
    public TrailRenderer flipperTrail;

    private float targetAngle = -15f;
    private float currentAngle = -15f;
    private bool canFlip = true;
    private bool canUnflip = true;

    private void Start()
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

        Color colorTransparent = color;
        colorTransparent.a = 0;

        color.a = 1;

        flipperSprite.color = color;
        flipperTrail.startColor = color;
        flipperTrail.endColor = colorTransparent;
    }

    private void Update()
    {
        if (!IsInit) return;

        if (Game.RoundState == RoundState.Play)
        {
            if (player.flipper && isPlayerControlled || player.aiFlipper) Flip();        
        }

        FlipUpdate();
    }


    private void Flip()
    {

        if (canFlip && !canUnflip)
        {
            Debug.Log("Flip!");
            targetAngle = flippedAngle;
            flipperPivot.tag = "Bumper";
            if (gameObject.activeSelf) StartCoroutine(FlipDelay());
        }

    }

    private IEnumerator FlipDelay()
    {
        canFlip = false;

        yield return new WaitForSeconds(.7f);

        canUnflip = true;
        flipperPivot.tag = "Flipper";
    }

    private void FlipUpdate()
    {
        float t = targetAngle == restAngle ? .5f : 1.5f;
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, t * Game.deltaTime * 400f);
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
