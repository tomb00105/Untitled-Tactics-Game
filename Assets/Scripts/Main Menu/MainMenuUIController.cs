using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUIController : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject menuPanel;
    public GameObject settingsPanel;
    public GameObject exitButton;

    public void TestLevelButton()
    {
        StartCoroutine(LoadTestLevel());
    }

    IEnumerator LoadTestLevel()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Test Level", LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void SettingsButton()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void SettingsMenuBackButton()
    {
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}