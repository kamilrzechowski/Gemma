using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    public Image splashImage;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(LoadAsynchronusly());
    }

    IEnumerator LoadAsynchronusly()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        //while (!operation.isDone)
        //{
        //    splashImage.GetComponent<RectTransform>().Rotate(new Vector3(0, 0, -2f));
            yield return null;
        //}
    }
}
