using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    void Start()
    {
        MainMenuLoad();
    }

    public static void MainMenuLoad()
    {
        SceneManager.LoadScene(1);
    }

}
