using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour
{
    public Button levelButton;
    public Image[] stars;

    public void GoToLevel(int levelNum)
    {
        //currLevel = levelNum;
        SceneManager.LoadScene("GameLevel");
    }
}
