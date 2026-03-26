using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumePercent;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip panelCloseSound;

    private CanvasGroup canvasGroup;
    private float panelAnimDuration = 0.5f;
    private bool isSettingsOpen = false;

    private AudioSource audioSource;

    private void Start()
    {
        canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = settingsPanel.AddComponent<CanvasGroup>();
        }

        // Создаём AudioSource если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        canvasGroup.alpha = 0f;
        settingsPanel.SetActive(false);

        backButton.onClick.AddListener(CloseSettings);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        LoadSettings();
    }

    public void OpenSettings()
    {
        if (!isSettingsOpen)
        {
            isSettingsOpen = true;
            settingsPanel.SetActive(true);
            canvasGroup.alpha = 0f;
            Debug.Log("Открываю Settings...");
            StartCoroutine(AnimatePanel(true));
        }
    }

    public void CloseSettings()
    {
        if (isSettingsOpen)
        {
            isSettingsOpen = false;
            PlayCloseSound();
            Debug.Log("Закрываю Settings...");
            StartCoroutine(AnimatePanel(false));
        }
    }

    private IEnumerator AnimatePanel(bool show)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        while (elapsed < panelAnimDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / panelAnimDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (!show)
        {
            settingsPanel.SetActive(false);
        }

        Debug.Log("Анимация завершена. Active: " + settingsPanel.activeSelf);
    }

    private void PlayCloseSound()
    {
        if (panelCloseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(panelCloseSound, 0.15f);
        }
    }

    private void OnVolumeChanged(float value)
    {
        volumePercent.text = ((int)value).ToString() + "%";

        // ← ГЛАВНОЕ ИЗМЕНЕНИЕ: ИСПОЛЬЗУЕМ AudioListener ДЛЯ ГЛОБАЛЬНОЙ ГРОМКОСТИ
        AudioListener.volume = value / 100f;

        if (audioManager != null)
        {
            audioManager.SetVolume(value / 100f);
        }

        SaveSettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("Volume", 100f);
        volumeSlider.value = savedVolume;

        // ← ЗАГРУЖАЕМ ГРОМКОСТЬ ПРИ СТАРТЕ
        AudioListener.volume = savedVolume / 100f;
    }
}