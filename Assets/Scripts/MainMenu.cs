using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public Button aboutButton;
    public Button exitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Kích hoạt các nút theo yêu cầu
        playButton.onClick.AddListener(OnPlayClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Nút About tạm thời bị vô hiệu hóa
        if (aboutButton != null)
        {
            aboutButton.interactable = false;
            aboutButton.GetComponentInChildren<Text>().text = "About (Coming Soon)";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnPlayClicked()
    {
        // Gọi SceneManagement để load Level 1
        if (SceneManagement.instance != null)
        {
            SceneManagement.instance.LoadLevel("Level1");
        }
        else
        {
            // Nếu chưa có SceneManagement trong scene (ví dụ bạn test riêng menu)
            SceneManager.LoadScene("Level1");
        }
    }

    void OnExitClicked()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

        // Nếu đang chạy trong Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
