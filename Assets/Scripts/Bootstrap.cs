using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void Awake()
    {
         Scene systemScene = SceneManager.GetActiveScene();

        // Kiểm tra xem hiện tại chỉ có duy nhất scene System đang mở
        if (SceneManager.sceneCount == 1 && systemScene.name == "System")
        {
            Debug.Log("🟢 Only System scene active → Loading MainMenu additively...");
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadSceneAsync("MenuScene", LoadSceneMode.Additive).completed += (op) =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("MenuScene"));
            };
        }
        else
        {
            Debug.Log("🟡 System scene already active, keeping persistent objects only");
        }
    }
}
