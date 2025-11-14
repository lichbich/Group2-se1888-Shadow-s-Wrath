using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAsync : MonoBehaviour
{
    // Tên của scene Main Menu
    public string sceneToLoad = "FinalMainMenu";

    void Start()
    {
        // Bắt đầu tải scene Main Menu ở chế độ nền
        StartCoroutine(LoadSceneInBackground());
    }

    IEnumerator LoadSceneInBackground()
    {
        // Bắt đầu hoạt động tải scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Chờ cho đến khi scene được tải xong
        while (!operation.isDone)
        {
            // (Bạn có thể cập nhật thanh tiến trình ở đây nếu muốn)
            // float progress = Mathf.Clamp01(operation.progress / 0.9f);
            // Debug.Log(progress);

            yield return null; // Chờ đến frame tiếp theo
        }
    }
}