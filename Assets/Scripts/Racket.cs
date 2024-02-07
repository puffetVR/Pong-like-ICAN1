using System.Collections;
using UnityEngine;

public class Racket : Base
{
    public bool isPlayerControlled = false;
    public Player player { get; private set; }
    public bool IsControllable { get; private set; }

    [Header("Flags")]
    private bool isSmashAlt = false;
    private bool canSmash = true;
    [Header("References")]
    public Rigidbody2D body;
    public Transform pivot;
    public SpriteRenderer racketSprite;
    public TrailRenderer racketTrail;
    public BoxCollider2D racketCollider;
    public Transform racketCenter;
    private float targetAngle = -15f;
    private float currentAngle = -15f;
    private float targetOffset;
    private float currentOffset;
    private float colliderYOffset;
    public Color racketColor { get; private set; }

    [Header("Vectors")]
    private Vector2 moveVector;

    void Start()
    {
        InitAttributes();
    }

    private void InitAttributes()
    {
        IsControllable = false;
        colliderYOffset = racketCollider.offset.y;
    }

    public void SetPlayerOwner(Player owner)
    {
        player = owner;
    }

    public void ColorSetter()
    {
        Color color = Game.AIColor;

        switch (player.controllerState)
        {
            case ControllerState.Player:
                color = player.playerTeam == PlayerTeam.Player1 ? Game.Player1Color : Game.Player2Color;
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

        body.position = new Vector2(body.position.x, Mathf.Clamp(body.position.y, -Game.maxRacketHeight, Game.maxRacketHeight));

        if (player.fire && canSmash) RacketSmash();
        RacketSmashUpdate();

        if (isPlayerControlled) RacketMovement();

    }

    private void RacketSmash()
    {
        Debug.Log("Poc");
        isSmashAlt = !isSmashAlt;
        targetAngle = isSmashAlt ? -165f : -15f;
        targetOffset = isSmashAlt ? -colliderYOffset : colliderYOffset;
        racketCollider.enabled = true;
        if (gameObject.activeSelf) StartCoroutine(SmashDelay());
    }
    
    IEnumerator SmashDelay()
    {
        canSmash = false;

        yield return new WaitForSeconds(.25f);

        canSmash = true;
        racketCollider.enabled = false;
    }

    private void RacketSmashUpdate()
    {
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Game.racketSpeed / 3);
        Quaternion angle = Quaternion.Euler(new Vector3(0f, 0f, currentAngle));
        pivot.localRotation = angle;

        currentOffset = Mathf.MoveTowards(currentOffset, targetOffset, Game.racketSpeed / 300);
        racketCollider.offset = new Vector2(racketCollider.offset.x, currentOffset);
    }

    private void RacketMovement()
    {
        float moveAmount = player.moveAxis;
        if (body.position.y >= Game.maxRacketHeight) moveAmount = Mathf.Clamp(moveAmount, -1f, 0f);
        if (body.position.y <= -Game.maxRacketHeight) moveAmount = Mathf.Clamp(moveAmount, 0f, 1f);

        body.velocity = new Vector2(0f, moveAmount * Game.racketSpeed);

        if (body.position.y > Game.maxRacketHeight
            || body.position.y < -Game.maxRacketHeight) body.velocity = Vector2.zero;
    }

    public void RacketReset()
    {
        body.velocity = Vector2.zero;
        racketCollider.enabled = false;
        
        float posX = Game.racketXPosFromOrigin;
        float posY = Game.racketYPosFromOrigin;
        switch (player.playerTeam)
        {
            case PlayerTeam.Player1:
                body.position = new Vector2(posX, posY);
                break;
            case PlayerTeam.Player2:
                body.position = new Vector2(-posX, posY);
                break;
            default:
                break;
        }

        Game.ResetTrail(racketTrail);
    }

}