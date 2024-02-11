using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainMenuOnStart : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene(1);
    }

}
