using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] string mainScene;
    public void StartGame()
    {
        SceneManager.LoadScene(mainScene);
    }
}
