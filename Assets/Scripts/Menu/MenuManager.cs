using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    [SerializeField] private string sceneGamePlay;
    public void StartGame()
    {
        SceneManager.LoadScene(sceneGamePlay);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}
