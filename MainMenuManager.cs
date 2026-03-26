using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    private void Start()
    {
        settingsPanel.SetActive(false);

        // Кнопка для перехода в EquipmentScene
        if (playButton != null)
        {
            playButton.onClick.AddListener(GoToEquipment);
        }

        // Кнопка для перехода в EquipmentScene (если это отдельная кнопка)
        if (equipmentButton != null)
        {
            equipmentButton.onClick.AddListener(GoToEquipment);
        }

        // Кнопка настроек
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        // Кнопка выхода
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }

        // Кнопка закрытия настроек
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }
    }

    private void GoToEquipment()
    {
        SceneManager.LoadScene("EquipmentScene");
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}