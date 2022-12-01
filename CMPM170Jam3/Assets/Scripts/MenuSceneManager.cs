using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneManager : MonoBehaviour
{
    public void LoadStoryScene() {
        SceneManager.LoadScene("Story-Scene");
    }
    
    public void LoadLevel1() {
        SceneManager.LoadScene("Level-1");
    }

    public void LoadStartScene() {
        SceneManager.LoadScene("Start-Scene");
    }

    public void ExitGame() {  
        Application.Quit();
    }
}
