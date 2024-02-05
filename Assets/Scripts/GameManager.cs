using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
    }

    public float deltaTime { get; private set; }
    public float fixedDeltaTime { get; private set; }

    [Header("Input Manager")]
    public string Vertical = "Vertical";
    public string Fire = "Fire";
    public string Item = "Item";
    public string Flipper = "Flipper";

    [Header("Rackets")]
    public Racket LeftRacket;
    public Racket RightRacket;
    public float racketSpeed = 2f;

    private void Update()
    {
        deltaTime = Time.deltaTime;
    }

    private void FixedUpdate()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

}
