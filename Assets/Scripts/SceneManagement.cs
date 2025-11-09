using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement instance;

    [Header("Loading UI")]
    public GameObject loadingScreen;
    public Slider progressBar;
    public Text progressText;
    public Text levelLabel;

    void Start()
    {
        if (instance == this)
        DontDestroyOnLoad(gameObject);
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    // Gọi để load scene theo tên
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadLevelAsync(sceneName));
    }

    // Gọi để load scene kế tiếp
    public void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        // Nếu hết level thì load lại level đầu tiên hoặc hiện màn hình chiến thắng
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Lấy tên scene kế tiếp
            string nextScenePath = SceneUtility.GetScenePathByBuildIndex(nextIndex);
            string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(nextScenePath);
            StartCoroutine(LoadLevelAsync(nextSceneName));
        }
        else
        {
            Debug.Log("All levels completed!");
             // Ẩn UI loading nếu đang bật
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
            // Bạn có thể gọi hàm hiển thị màn hình thắng tại đây
        }
    }

 private IEnumerator LoadLevelAsync(string scenePath)
{
    // Luôn bật Loading Screen trước
    if (loadingScreen != null)
        loadingScreen.SetActive(true);

    // Sau đó mới ẩn các UI khác
    HideAllUIExceptLoading();

    // Lấy tên scene (loại bỏ .unity)
    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
    if (levelLabel != null)
        levelLabel.text = "Loading " + sceneName + "...";

    // Bắt đầu load scene bất đồng bộ
    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
    operation.allowSceneActivation = false;

    float displayedProgress = 0f;

    while (!operation.isDone)
    {
        // Unity chỉ tăng đến 0.9 khi chưa kích hoạt scene
        float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

        // Làm mượt tiến trình (chạy đều từ 0 → 1)
        displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * 3f);

        // Cập nhật UI
        if (progressBar != null)
            progressBar.value = displayedProgress;

        if (progressText != null)
            progressText.text = Mathf.RoundToInt(displayedProgress * 100f) + "%";

        // Khi gần xong (>=0.9) → đợi 1 chút rồi cho load scene
        if (displayedProgress >= 0.995f)
        {
            if (progressBar != null) progressBar.value = 1f;
            if (progressText != null) progressText.text = "100%";

            // Delay nhỏ để người chơi thấy loading đầy
            yield return new WaitForSeconds(0.5f);
            operation.allowSceneActivation = true;
        }

        yield return null;
    }

    // 5️⃣ Tắt loading screen khi hoàn tất
    if (loadingScreen != null)
        loadingScreen.SetActive(false);
}

private void HideAllUIExceptLoading()
{
    // Lấy scene hiện tại
    var currentScene = SceneManager.GetActiveScene();

#if UNITY_2023_1_OR_NEWER
    Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
#else
    Canvas[] allCanvas = FindObjectsOfType<Canvas>(true);
#endif

    foreach (Canvas canvas in allCanvas)
    {
        if (canvas == null) continue;

        // Nếu canvas không thuộc scene hiện tại, bỏ qua (ví dụ canvas global của hệ thống)
        if (canvas.gameObject.scene != currentScene) continue;

        // Duyệt tất cả direct children của canvas (thường là các panel, menu, hud, v.v.)
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child == null) continue;

            GameObject go = child.gameObject;

            // Bỏ qua nếu chính là loadingScreen hoặc là descendant của loadingScreen
            if (loadingScreen != null)
            {
                if (go == loadingScreen || go.transform.IsChildOf(loadingScreen.transform))
                {
                    // giữ lại
                    continue;
                }
            }

            // Tắt child UI (panel) này
            go.SetActive(false);
        }
    }
}



}
