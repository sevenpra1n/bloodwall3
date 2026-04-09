using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button newGameButton;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    [SerializeField] private GameObject confirmNewGamePanel;
    [SerializeField] private Button confirmNewGameYesButton;
    [SerializeField] private Button confirmNewGameNoButton;

    private void Start()
    {
        settingsPanel.SetActive(false);

        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);

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

        // Кнопка новой игры
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (confirmNewGameYesButton != null)
            confirmNewGameYesButton.onClick.AddListener(StartNewGame);

        if (confirmNewGameNoButton != null)
            confirmNewGameNoButton.onClick.AddListener(CancelNewGame);
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

    private void OnNewGameClicked()
    {
        if (confirmNewGamePanel != null)
        {
            confirmNewGamePanel.SetActive(true);
        }
        else
        {
            // No confirmation panel wired up – delete data directly
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CancelNewGame()
    {
        if (confirmNewGamePanel != null)
            confirmNewGamePanel.SetActive(false);
    }
}