using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtLoadNextScene : MonoBehaviour
{
    public int SceneNr = 3;

    public void sceneSwitcher()
    {
        SceneManager.LoadScene(SceneNr);  //MainMenu is scene nr 3
    }
}
