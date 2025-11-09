
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    public Button replayButton;
    public Button menuButton;
    public Button exitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        replayButton.onClick.AddListener(OnReplayClicked);
        menuButton.onClick.AddListener(OnMenuClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnReplayClicked()
    {
        Debug.Log("Replay clicked - reloading Level1");

        if (SceneManagement.instance != null)
        {
            SceneManagement.instance.LoadLevel("Level1");
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    void OnMenuClicked()
    {
        Debug.Log("Return to Main Menu");

        if (SceneManagement.instance != null)
        {
            SceneManagement.instance.LoadLevel("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    
    void OnExitClicked()
    {
        Debug.Log("Exit game");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
