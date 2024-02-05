using UnityEngine;

public class Base : MonoBehaviour
{

    public GameManager Game {  get; private set; }
    public bool IsInit { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        Game = GameManager.Instance;
        
        if (Game) IsInit = true;
    }

}
