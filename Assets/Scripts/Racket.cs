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
    public Animator racketAnimator;
    private float targetAngle;
    private float currentAngle;
    private float upAngle = -15f;
    private float downAngle = -165f;
    private float targetOffset;
    private float currentOffset;
    private float colliderYOffset;
    private float colliderXOffset;
    public Color racketColor { get; private set; }

    void Start()
    {
        InitAttributes();
    }

    private void InitAttributes()
    {
        IsControllable = false;
        colliderYOffset = racketCollider.offset.y;

        bool IsPlayer1 = player.playerTeam == PlayerTeam.Player1 ? true : false;

        colliderXOffset = IsPlayer1 ? racketCollider.offset.x : -racketCollider.offset.x;
        upAngle = IsPlayer1 ? upAngle : -upAngle;
        downAngle = IsPlayer1 ? downAngle : -downAngle;
        currentAngle = upAngle;
        targetAngle = currentAngle;
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

        racketColor = color;
        racketSprite.color = color;
        racketTrail.startColor = color;
        racketTrail.endColor = colorTransparent;
    }

    public void IsControllableSet(bool state)
    {
        IsControllable = state;
    }

    void Update()
    {
        if (IsInit == false) return;

        body.position = new Vector2(body.position.x, Mathf.Clamp(body.position.y, -Game.maxRacketHeight, Game.maxRacketHeight));

        if (Game.RoundState == RoundState.Play)
        {
            if (player.fire || player.aiFire) RacketSmash();
           
            RacketMovement();
        }

        RacketSmashUpdate();
    }

    private void RacketSmash()
    {
        if (!canSmash) return;

        Debug.Log("Poc");
        isSmashAlt = !isSmashAlt;
        targetAngle = isSmashAlt ? downAngle : upAngle;
        targetOffset = isSmashAlt ? -colliderYOffset : colliderYOffset;
        racketCollider.enabled = true;
        if (gameObject.activeSelf) StartCoroutine(SmashDelay());
    }
    
    IEnumerator SmashDelay()
    {
        canSmash = false;

        yield return new WaitForSeconds(1.75f / Game.Ball.currentBallSpeed);

        canSmash = true;
        racketCollider.enabled = false;
    }

    private void RacketSmashUpdate()
    {
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, (Game.racketSpeed / 3) * (Game.deltaTime * 400f));
        Quaternion angle = Quaternion.Euler(new Vector3(0f, 0f, currentAngle));
        pivot.localRotation = angle;

        currentOffset = Mathf.MoveTowards(currentOffset, targetOffset, (Game.racketSpeed / 300) * (Game.deltaTime * 400f));
        racketCollider.offset = new Vector2(colliderXOffset, currentOffset);
    }

    private void RacketMovement()
    {
        float moveAmount = player.moveAmount;

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

        isSmashAlt = false;
        targetAngle = upAngle;
        targetOffset = colliderYOffset;

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