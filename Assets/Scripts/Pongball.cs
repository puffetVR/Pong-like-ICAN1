using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class Pongball : Base
{
    [Header("Flags")]
    private bool LastHitBumper = false;
    public bool IsMoving { get; private set; } = false;

    [Header("References")]
    public Rigidbody2D body;
    public TrailRenderer ballTrail;
    public SpriteRenderer[] ballSprites;
    public Animator ballAnimator;
    public ParticleSystem ballCollisionParticles;
    public ParticleSystem ballExplosionParticles;
    public ParticleSystem ballWinParticles;

    public float currentBallSpeed { get; private set; }
    private float speedBeforeBumper;

    // Change Ball color on owner change
    public Racket currentOwner
    {
        get { return _currentOwner; }
        private set
        {
            _currentOwner = value;
            ColorSetter(_currentOwner);
        }
    }
    private Racket _currentOwner;

    [Header("Physics")]
    Vector2 moveDirection;
    Vector2 ballVelocity;
    Vector2 lastPos;
    Vector2 newPos;
    float timeSinceStuck;

    void Start()
    {
        InitAttributes();
    }

    public void InitAttributes()
    {
        body = GetComponent<Rigidbody2D>();
        ballTrail = GetComponentInChildren<TrailRenderer>();
    }

    public Racket RandomizeOwner()
    {
        int rand = Random.Range(1, 3);
        Racket randOwner = rand == 1 ? Game.Players[0].racket : Game.Players[1].racket;

        SetBallOwner(randOwner);

        return randOwner;
    }

    public void ColorSetter(Racket owner)
    {
        Color colorTransparent = owner.racketColor;
        colorTransparent.a = 0;

        for (int i = 0; i < ballSprites.Length; i++)
        {
            ballSprites[i].color = owner.racketColor;
        }

        ballTrail.startColor = owner.racketColor;
        ballTrail.endColor = colorTransparent;
        var main = ballCollisionParticles.main;
        main.startColor = owner.racketColor;
    }

    public void IsBallMoving(bool state)
    {
        IsMoving = state;
    }

    private void Update()
    {
        if (!IsInit) return;

        BallMover();
        AnimatorUpdater();
    }

    public Vector2 SetMoveDirection(float hitterXPos)
    {
        if (currentOwner == null) return Vector2.zero;

        //Vector2 dir = currentOwner == Game.Players[0] ? Vector2.right : Vector2.left;
        Vector2 dir = hitterXPos < 0 ? Vector2.right : Vector2.left;

        return dir;
    }

    public void BallMover()
    {
        currentBallSpeed = LastHitBumper ?
                        currentBallSpeed = Mathf.Clamp(currentBallSpeed, Game.baseBallSpeed, Game.maxBallSpeed * 2) :
                        currentBallSpeed = Mathf.Clamp(currentBallSpeed, Game.baseBallSpeed, Game.maxBallSpeed);

        if (IsMoving == false || Game.RoundState == RoundState.Wait) return;

        ballVelocity = moveDirection * currentBallSpeed;
        body.velocity = ballVelocity;


        StuckDetect();

    }

    void StuckDetect()
    {
        newPos = body.position;

        if (lastPos == newPos)
        {
            Debug.Log("Ball seems stuck");

            timeSinceStuck += Game.deltaTime;

            if (timeSinceStuck > .02f)
            {
                Debug.LogWarning("DOOR STUCK!");
                // how fix plz help
            }

        }

        else if (lastPos != newPos)
        {
            Debug.Log("Ball is currently moving");
            timeSinceStuck = 0f;
        }

        lastPos = newPos;


    }

    private void AnimatorUpdater()
    {
        ballAnimator.SetFloat("currentBallSpeed", currentBallSpeed);
    }

    public void SetBallSpeed(float speed)
    {
        currentBallSpeed = speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        if (!collision.gameObject.CompareTag("Bumper") && LastHitBumper) SetBallSpeed(speedBeforeBumper * 0.75f);

        if (collision.gameObject.CompareTag("Bumper"))
        {
            LastHitBumper = true;
            Animator anim = collision.gameObject.GetComponent<Animator>();
            if (anim) anim.SetTrigger("Hit");
            speedBeforeBumper = currentBallSpeed * 1.5f;
            SetBallSpeed(speedBeforeBumper);
            Game.ShakeCamera(currentBallSpeed / 10, .1f);
        }
        else
        {         
            speedBeforeBumper = currentBallSpeed;
            LastHitBumper = false;
        }

        if (collision.gameObject.CompareTag("PlayerWall"))
        {
            PlayerWall hitWall = collision.gameObject.GetComponent<PlayerWall>();

            Game.ShakeCamera(currentBallSpeed / 6, .2f);
            hitWall.DamageWall();

            if (Game.gameMode == GameMode.Versus)
            {
                Racket ownerSwap = hitWall.player == Game.Players[0] ? Game.Players[0].racket : Game.Players[1].racket;
                SetBallOwner(ownerSwap);
            }
           
        }

        ContactPoint2D[] contacts = collision.contacts;
        moveDirection = Vector2.Reflect(moveDirection, contacts[0].normal);

        ballAnimator.SetTrigger("Hit");
        ballCollisionParticles.Play();
        RandomizeBallRotation();

    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger == null) return;

        if (trigger.gameObject.CompareTag("Racket"))
        {
            Racket hitRacket = trigger.gameObject.GetComponent<Racket>();
            Debug.Log(hitRacket.gameObject.name + " hit");
            SetBallOwner(hitRacket);
            if (currentBallSpeed < Game.maxBallSpeed) SetBallSpeed(currentBallSpeed * 1.2f);

            Vector2 moveDirectionA;
            moveDirectionA = SetMoveDirection(hitRacket.body.position.x);

            Vector2 moveDirectionB;
            moveDirectionB = new Vector2(0f, currentOwner.player.moveAmount / Game.reflectDampening);

            moveDirection = moveDirectionA + moveDirectionB;

            ballAnimator.SetTrigger("Hit");
            ballCollisionParticles.Play();

            hitRacket.racketAnimator.SetTrigger("Hit");
            RandomizeBallRotation();
            Game.ShakeCamera(currentBallSpeed / 10, .1f);
            IsBallMoving(true);
            hitRacket.racketCollider.enabled = false;
        }

        if (trigger.gameObject.CompareTag("PlayerGoal"))
        {
            int scorer = trigger == Game.PlayerGoals[0] ? 1 : 0;

            BallInGoal();
            Game.Players[scorer].Score((int)trigger.transform.localScale.z);
            if (Game.Players[scorer].currentScore < 3 && gameObject.activeSelf) StartCoroutine(Game.RoundEnd(Game.Players[scorer]));

        }

        if (trigger.gameObject.CompareTag("WinTrigger"))
        {
            BallInGoal();
            Debug.Log("Player " + (int)currentOwner.player.playerTeam + " wins the game !");
            Game.EndGame();
        }

    }

    private void OnTriggerExit2D(Collider2D trigger)
    {
        if (trigger == null || trigger != Game.PlayZone) return;
     
        BallExplode();
        if (gameObject.activeSelf) StartCoroutine(Game.RoundEnd(currentOwner.player));

    }

    void RandomizeBallRotation()
    {
        float angle = Random.Range(0, 360);

        body.SetRotation(angle);
    }

    public void BallReset(Racket owner)
    {
        IsBallMoving(false);

        Game.ResetTrail(ballTrail);
        ballAnimator.SetTrigger("Reset");

        float xPosition = owner.body.position.x < 0 ? owner.body.position.x + 1f : owner.body.position.x - 1f;
        body.position = new Vector2(xPosition, 0f);
        SetBallOwner(owner);
        ColorSetter(owner);

        LastHitBumper = false;

        SetBallSpeed(Game.baseBallSpeed);
        moveDirection = SetMoveDirection(owner.body.position.x);
    }

    public void SetBallOwner(Racket pass)
    {
        currentOwner = pass;
    }

    public void StopBall()
    {
        IsBallMoving(false);
        body.velocity = Vector2.zero;
    }

    public void BallInGoal()
    {
        StopBall();
        Debug.Log("Goal!");
        Game.ShakeCamera(1f, .2f);

        ballAnimator.SetTrigger("Explode");
        ballWinParticles.Play();
    }

    public void BallExplode()
    {
        StopBall();
        Debug.Log("Boom");
        Game.ShakeCamera(1f, .2f);

        ballAnimator.SetTrigger("Explode");
        ballExplosionParticles.Play();

    }

}
