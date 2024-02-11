using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    Fader FaderScript;
    public Image fader;

    public void Awake()
    {
        GameManager.SetTimeScale(1);
        GameManager.ToggleCursor(true);
        FaderScript = gameObject.AddComponent<Fader>();
        FaderScript.OutFade(fader);
    }

    public void LoadLevel(string levelName)
    {
        FaderScript.InFade(fader, levelName);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}
