using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class Pongball : Base
{
    [Header("Flags")]
    private bool IsMoving = false;
    private bool LastHitBumper = false;

    [Header("References")]
    public Rigidbody2D body;
    public TrailRenderer ballTrail;
    public SpriteRenderer ballSprite;

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

    [Header("Vectors")]
    Vector2 moveDirection;
    Vector2 ballVelocity;

    void Start()
    {
        InitAttributes();
    }

    public void InitAttributes()
    {
        body = GetComponent<Rigidbody2D>();
        ballTrail = GetComponentInChildren<TrailRenderer>();
        ballSprite = GetComponentInChildren<SpriteRenderer>();
    }

    public Racket RandomizeOwner()
    {
        int rand = Random.Range(1, 3);

        return currentOwner = rand == 1 ? Game.Players[0].racket : Game.Players[1].racket;
    }

    public void ColorSetter(Racket owner)
    {
        ballSprite.color = owner.racketColor;
        ballTrail.startColor = owner.racketColor;
        ballTrail.endColor = owner.racketColor;
    }

    public void IsBallMoving(bool state)
    {
        IsMoving = state;
    }

    private void Update()
    {
        if (!IsInit) return;

        BallMover();
    }

    public void SetMoveDirection()
    {
        if (currentOwner == null) return;

        moveDirection = currentOwner == Game.Players[0] ? Vector2.right : Vector2.left;
    }

    public void BallMover()
    {
        if (IsMoving == false) return;

        //Debug.Log("Pushing ball");
        ballVelocity = moveDirection * currentBallSpeed;
        body.velocity = ballVelocity;
    }

    public void SetBallSpeed(float speed)
    {
        currentBallSpeed = speed;

        speedBeforeBumper = LastHitBumper ? speedBeforeBumper : Game.baseBallSpeed;

        currentBallSpeed = LastHitBumper ?
            currentBallSpeed = Mathf.Clamp(currentBallSpeed, Game.baseBallSpeed, Game.maxBallSpeed * 2) :
            currentBallSpeed = Mathf.Clamp(currentBallSpeed, Game.baseBallSpeed, Game.maxBallSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D[] contacts = collision.contacts;

        Vector2 moveDirectionA;
        moveDirectionA = Vector2.Reflect(moveDirection, contacts[0].normal);

        Vector2 moveDirectionB;
        moveDirectionB = Vector2.zero;

        if (!collision.gameObject.CompareTag("Bumper")) SetBallSpeed(speedBeforeBumper);

        //if (collision.gameObject.CompareTag("Racket"))
        //{
        //    currentOwner = collision.gameObject.GetComponentInParent<Racket>();
        //    collision.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        //    if (currentBallSpeed < Game.maxBallSpeed) SetBallSpeed(currentBallSpeed * 1.2f);
        //    moveDirectionB = new Vector2(0f, currentOwner.moveAxis / Game.reflectDampening);
        //}

        if (collision.gameObject.CompareTag("Bumper"))
        {
            LastHitBumper = true;
            SetBallSpeed(currentBallSpeed * 1.5f);
            Game.ShakeCamera(currentBallSpeed / 10, .1f);
        }
        else
        {         
            speedBeforeBumper = currentBallSpeed;
            LastHitBumper = false;
        }

        if (collision.gameObject.CompareTag("PlayerWall"))
        {
            Game.ShakeCamera(currentBallSpeed / 6, .2f);
            collision.gameObject.GetComponent<PlayerWall>().DamageWall();
        }

        moveDirection = moveDirectionA + moveDirectionB;

    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger == null) return;

        if (trigger.gameObject.CompareTag("Racket"))
        {
            Racket hitRacket = trigger.gameObject.GetComponent<Racket>();
            //Debug.Log("RACKET HIT");
            currentOwner = hitRacket;
            hitRacket.racketCollider.enabled = false;
            if (currentBallSpeed < Game.maxBallSpeed) SetBallSpeed(currentBallSpeed * 1.2f);

            Vector2 moveDirectionA;
            moveDirectionA = -moveDirection;
            //Vector2 point = trigger.ClosestPoint(transform.position);
            //moveDirectionA = Vector2.Reflect(moveDirection, point);

            Vector2 moveDirectionB;
            moveDirectionB = new Vector2(0f, currentOwner.player.moveAxis / Game.reflectDampening);

            moveDirection = moveDirectionA + moveDirectionB;

            Game.ShakeCamera(currentBallSpeed / 10, .1f);
            IsBallMoving(true);
        }

        if (!trigger.gameObject.CompareTag("PlayerGoal")) return;

        int scorer = trigger == Game.PlayerGoals[0] ? 1 : 0;

        BallInGoal();
        Game.Players[scorer].Score();
        if (gameObject.activeSelf) StartCoroutine(Game.RoundEnd(Game.Players[scorer]));
    }

    private void OnTriggerExit2D(Collider2D trigger)
    {
        if (trigger != Game.PlayZone) return;
     
        BallExplode();
        if (gameObject.activeSelf) StartCoroutine(Game.RoundEnd(null));

    }

    public void BallReset(Racket owner)
    {
        IsBallMoving(false);

        Game.ResetTrail(ballTrail);
        ballSprite.enabled = true;

        float xPosition = owner.body.position.x < 0 ? -owner.body.position.x - 1f : -owner.body.position.x + 1f;
        body.position = new Vector2(xPosition, 0f);
        currentOwner = owner;
        ColorSetter(owner);

        LastHitBumper = false;

        SetBallSpeed(Game.baseBallSpeed);
        SetMoveDirection();
        //StartCoroutine(StartBallMovement());
    }

    IEnumerator StartBallMovement()
    {
        yield return new WaitForSeconds(Game.timeBeforeBallMove);

        IsBallMoving(true);
    }

    public void StopBall()
    {
        IsBallMoving(false);
        ballSprite.enabled = false;
        body.velocity = Vector2.zero;
    }

    public void BallInGoal()
    {
        StopBall();
        Debug.Log("Goal!");
        Game.ShakeCamera(1f, .2f);

    }

    public void BallExplode()
    {
        StopBall();
        Debug.Log("Boom");
        Game.ShakeCamera(1f, .2f);
    }

}
