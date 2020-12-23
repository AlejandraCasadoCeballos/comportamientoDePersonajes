using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string menuScene = "MainMenu";
    public const string configScene = "Configuration";
    public const string exitScene = "ExitGame";
    public const string victoryScene = "Victory"; 
    public const string gameScene = "GameScene";
    public const string usernameScene = "Username";
    public const string rankingScene = "Ranking";
    public const string controlsScene = "Controls";
    public const string levelsScene = "Levels";


    private static string previousScene = "";

    private static string requestedScene = "";

    public static  void LoadMainMenuScene()
    {
        LoadScene(menuScene);
    }
    public static void LoadConfigScene()
    {
        LoadScene(configScene);
    }
    public static void LoadExitGame()
    {
        LoadScene(exitScene);
    }
    public static void LoadUsernameScene()
    {
        LoadScene(usernameScene);
    }
    public static void LoadVictoryScene()
    {
        LoadScene(victoryScene);
    }
    public static void LoadLevelsScene()
    {
        LoadScene(levelsScene);
    }
    public static void LoadGameScene()
    {
        LoadScene(gameScene);
    }
    public static void LoadControlsScene()
    {
        LoadScene(controlsScene);
    }
    public static void LoadRankingScene()
    {
        LoadScene(rankingScene);
    }

    public static void LoadPreviousScene()
    {
        if (!previousScene.Equals(""))
        {
            LoadScene(previousScene);
        } else
        {
            LoadMainMenuScene();
            Debug.Log("There was no previous scene so main menu was loaded");
        }
    }

    public static void LoadRequestedScene()
    {
        SceneManager.LoadScene(requestedScene, LoadSceneMode.Single);
    }

    public static string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    private static void LoadScene(string name)
    {
        previousScene = GetCurrentScene();
        /*if(UITransition.instance != null)
        {
            requestedScene = name;
            UITransition.instance.FadeOut();
        } else
        {*/
            SceneManager.LoadScene(name, LoadSceneMode.Single);
        //}
    }

    public void ExitGame()
    {
        Debug.Log("Exit game");
        Application.Quit();
    }
}
