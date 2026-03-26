using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private Image fadePanel;
    [SerializeField] private Text loadingText;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SettingsManager settingsManager;

    [SerializeField] private AudioClip buttonClickSound;

    private float audioFadeDuration = 3f;
    private float panelFadeDuration = 1f;
    private float loadingDuration = 4f;
    private bool isTransitioning = false;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);
    }

    public void OnNewGame()
    {
        PlayButtonSound();
        if (!isTransitioning)
        {
            DisableAllButtons();

            // ← ПОЛНЫЙ СБРОС ВСЕХ ДАННЫХ
            ResetAllGameData();

            StartCoroutine(TransitionToScene("EquipmentScene"));
        }
    }

    public void OnContinue()
    {
        PlayButtonSound();
        if (!isTransitioning)
        {
            DisableAllButtons();
            StartCoroutine(TransitionToScene("EquipmentScene"));
        }
    }

    public void OnSettings()
    {
        PlayButtonSound();
        settingsManager.OpenSettings();
    }

    public void OnExit()
    {
        PlayButtonSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound, 0.08f);
        }
    }

    private void DisableAllButtons()
    {
        newGameButton.interactable = false;
        continueButton.interactable = false;
        settingsButton.interactable = false;
        exitButton.interactable = false;
    }

    // ← НОВЫЙ МЕТОД: ПОЛНЫЙ СБРОС ИГРЫ
    private void ResetAllGameData()
    {
        Debug.Log("🔄 ПОЛНЫЙ СБРОС ИГРЫ!");

        // ← ОЧИЩАЕМ ВСЕ СТАТИСТИКИ
        PlayerPrefs.DeleteKey("PlayerCoins");
        PlayerPrefs.DeleteKey("PlayerExp");
        PlayerPrefs.DeleteKey("PlayerLevel");
        PlayerPrefs.DeleteKey("PlayerPower");
        PlayerPrefs.DeleteKey("CurrentStage");

        // ← ОЧИЩАЕМ СНАРЯЖЕНИЕ
        PlayerPrefs.DeleteKey("EquippedWeapon");
        PlayerPrefs.DeleteKey("EquippedHead");
        PlayerPrefs.DeleteKey("EquippedBody");
        PlayerPrefs.DeleteKey("EquippedLegs");

        // ← ОЧИЩАЕМ ДАННЫЕ О БОЯХ
        PlayerPrefs.DeleteKey("JustFinishedBattle");
        PlayerPrefs.DeleteKey("LastBattleExpReward");

        // ← ОЧИЩАЕМ ВСЕ ПОКУПКИ (ВСЕ Purchased_* КЛЮЧИ)
        ResetAllPurchases();

        // ← СБРАСЫВАЕМ ДОСТИЖЕНИЯ
        AchievementSystem.ResetAchievements();

        // ← УСТАНАВЛИВАЕМ СТАРТОВЫЕ ЗНАЧЕНИЯ
        PlayerPrefs.SetInt("PlayerCoins", 0);         // 0 монет в начале
        PlayerPrefs.SetInt("PlayerExp", 0);           // 0 опыта
        PlayerPrefs.SetInt("PlayerLevel", 1);         // Уровень 1
        PlayerPrefs.SetInt("PlayerPower", 10);        // Базовая мощь 10
        PlayerPrefs.SetInt("CurrentStage", 0);        // Этаж 0

        PlayerPrefs.Save();

        Debug.Log("✅ Игра сброшена на стартовые значения!");
        Debug.Log("   💰 Монеты: 0");
        Debug.Log("   ⭐ Уровень: 1");
        Debug.Log("   📊 Опыт: 0");
        Debug.Log("   ⚔️ Мощь: 10");
        Debug.Log("   🎁 Все покупки удалены");
    }

    // ← НОВЫЙ МЕТОД: УДАЛЕНИЕ ВСЕХ ПОКУПОК
    private void ResetAllPurchases()
    {
        // ← СПИСОК ВСЕХ ПРЕДМЕТОВ КОТОРЫЕ МОГЛИ БЫТЬ КУПЛЕНЫ
        string[] purchasableItems = new string[]
        {
        // Оружие
        "Золотой клинок",
        "Алмазный клинок",
        "Незеритовая кирка??",
        "Незеритовый меч!",
        // Голова
        "Голова скелета",
        "Голова иссушителя..",
        "ДУЭЙН ДЖОНСОН?",
        // Тело
        "Элитры!",
        // Ноги
        "Rick Owens",
        "Ботинки клоуна (разраба)",
        };

        foreach (string itemName in purchasableItems)
        {
            string key = "Purchased_" + itemName;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                Debug.Log("❌ Удалена покупка: " + itemName);
            }
        }
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
        }

        if (audioManager != null)
        {
            StartCoroutine(audioManager.FadeOutAudio(audioFadeDuration));
        }

        yield return StartCoroutine(FadeIn(panelFadeDuration));

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(loadingDuration);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadePanel == null)
            yield break;

        float elapsed = 0f;
        Color startColor = fadePanel.color;
        startColor.a = 0;
        fadePanel.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            Color newColor = fadePanel.color;
            newColor.a = Mathf.Lerp(0, 1, progress);
            fadePanel.color = newColor;
            yield return null;
        }

        Color finalColor = fadePanel.color;
        finalColor.a = 1;
        fadePanel.color = finalColor;
    }
}