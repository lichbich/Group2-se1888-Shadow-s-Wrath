using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{

    [Header("UI")]
    public GameObject mainMenuPanel;   // Kéo MainMenuPanel vào đây
    public GameObject levelPanel;      // Kéo LevelPanel vào đây
    public GameObject settingsPanel;   // Kéo SettingsPanel vào đây
    public GameObject aboutUsPanel; // Kéo AboutUsPanel vào đây

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelPanel != null) levelPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutUsPanel != null) aboutUsPanel.SetActive(false);
    }


    // Khi nhấn Play
    public void Play()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelPanel != null) levelPanel.SetActive(true);
    }

    // Khi nhấn Back trong LevelPanel
    public void BackToMenu()
    {
        if (levelPanel != null) levelPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // Khi nhấn Settings trong MainMenuPanel
    public void Settings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // Khi nhấn Back trong SettingsPanel
    public void BackFromSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // Khi nhấn nút About Us
    public void AboutUs()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (aboutUsPanel != null) aboutUsPanel.SetActive(true);
    }

    // Khi nhấn nút Back trong About Us panel
    public void BackFromAboutUs()
    {
        if (aboutUsPanel != null) aboutUsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }


    // Load Level bằng index
    public void LoadLevelByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
