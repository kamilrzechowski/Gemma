using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtLoadNextScene : MonoBehaviour
{
    //public int SceneNr = 2;
    public string SceneName = "MainMenu";

    public void sceneSwitcher()
    {
        SceneManager.LoadScene(SceneName);  //MainMenu is scene nr 3
    }
}
