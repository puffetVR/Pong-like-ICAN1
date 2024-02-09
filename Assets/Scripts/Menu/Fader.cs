using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{

    public void InFade(Image fader, string sceneToLoad)
    {
        Color fadeCol = fader.color;
        fadeCol.a = 0f;
        StartCoroutine(InFadeCoroutine(fadeCol, fader, sceneToLoad));
    }

    public void OutFade(Image fader) 
    {
        Color fadeCol = Color.black;
        fadeCol.a = 1f;
        StartCoroutine(OutFadeCoroutine(fadeCol, fader));
    }

    public IEnumerator InFadeCoroutine(Color fadeCol, Image fader, string sceneToLoad)
    {

        for (float alpha = 0f; alpha <= 1; alpha += 2f * Time.deltaTime)
        {
            fadeCol.a = alpha;
            fader.color = fadeCol;
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);

    }

    public IEnumerator OutFadeCoroutine(Color fadeCol, Image fader)
    {
        for (float alpha = 1f; alpha <= 1; alpha -= 1f * Time.deltaTime)
        {
            if (alpha <= 0f) yield break;

            fadeCol.a = alpha;
            fader.color = fadeCol;
            yield return null;
        }

    }
}
