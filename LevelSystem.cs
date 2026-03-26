using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelSystem : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Text expText;
    [SerializeField] private Image[] levelIcons = new Image[10];
    [SerializeField] private Image[] xpBarTextures = new Image[11];

    // ← УВЕЛИЧЕНЫ В 3 РАЗА
    private int[] expRequiredForLevel = new int[11]
    {
        0,      // Level 1: 0
        300,    // Level 2: 300
        750,    // Level 3: 750
        1200,   // Level 4: 1200
        1800,   // Level 5: 1800
        2550,   // Level 6: 2550
        3450,   // Level 7: 3450
        4500,   // Level 8: 4500
        6000,   // Level 9: 6000
        7950,   // Level 10: 7950
        999999  // Level 11: Max
    };

    private int currentLevel = 1;
    private int currentExp = 0;
    private int displayedExp = 0;

    private void Start()
    {
        LoadProgress();
        CalculateLevel();
        UpdateUI();
        StartCoroutine(LevelIconLevitate());
    }

    private void CalculateLevel()
    {
        currentLevel = 1;

        for (int i = 10; i > 1; i--)
        {
            if (currentExp >= expRequiredForLevel[i - 1])
            {
                currentLevel = i;
                break;
            }
        }

        Debug.Log("Пересчитан уровень: " + currentLevel + " (опыт: " + currentExp + ")");

        int expPrevious = (currentLevel > 1) ? expRequiredForLevel[currentLevel - 1] : 0;
        displayedExp = currentExp - expPrevious;
    }

    private IEnumerator LevelIconLevitate()
    {
        Vector2[] originalPositions = new Vector2[levelIcons.Length];

        for (int i = 0; i < levelIcons.Length; i++)
        {
            if (levelIcons[i] != null)
            {
                RectTransform rect = levelIcons[i].GetComponent<RectTransform>();
                if (rect != null)
                    originalPositions[i] = rect.anchoredPosition;
            }
        }

        while (true)
        {
            float time = 0f;
            float duration = 2f;
            float levitateAmount = 5f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = time / duration;

                float yOffset = Mathf.Sin(progress * Mathf.PI * 2) * levitateAmount;

                for (int i = 0; i < levelIcons.Length; i++)
                {
                    if (levelIcons[i] != null)
                    {
                        RectTransform rect = levelIcons[i].GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = originalPositions[i] + new Vector2(0, yOffset);
                        }
                    }
                }

                yield return null;
            }
        }
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log("Добавлено опыта: " + amount + ". Всего: " + currentExp);

        while (currentLevel < 10 && currentExp >= expRequiredForLevel[currentLevel])
        {
            LevelUp();
        }

        SaveProgress();
    }

    public void ShowXpGainAnimation(int amount)
    {
        Debug.Log("🎯 Показываю анимацию XP: +" + amount);

        currentExp += amount;

        StartCoroutine(AnimateXpBar(amount));

        while (currentLevel < 10 && currentExp >= expRequiredForLevel[currentLevel])
        {
            LevelUp();
        }

        SaveProgress();
    }

    private void LevelUp()
    {
        currentLevel++;
        Debug.Log("⭐ УРОВЕНЬ ПОВЫШЕН! Новый уровень: " + currentLevel);

        UpdateLevelIcon();

        if (levelText != null)
            levelText.text = currentLevel.ToString();

        StartCoroutine(LevelUpAnimation());
    }

    private void UpdateLevelIcon()
    {
        foreach (Image icon in levelIcons)
        {
            if (icon != null)
                icon.enabled = false;
        }

        if (currentLevel > 0 && currentLevel <= levelIcons.Length)
        {
            if (levelIcons[currentLevel - 1] != null)
            {
                levelIcons[currentLevel - 1].enabled = true;
                Debug.Log("Иконка уровня " + currentLevel + " включена");
            }
        }
    }

    private IEnumerator LevelUpAnimation()
    {
        Debug.Log("✨ Запускаю анимацию повышения уровня...");

        Image levelIcon = null;
        if (currentLevel > 0 && currentLevel <= levelIcons.Length)
        {
            levelIcon = levelIcons[currentLevel - 1];
        }

        if (levelIcon != null)
        {
            RectTransform iconRect = levelIcon.GetComponent<RectTransform>();
            Vector3 originalScale = iconRect.localScale;
            Vector3 originalPos = iconRect.localPosition;

            float duration = 1.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                float jumpHeight = Mathf.Sin(progress * Mathf.PI * 3) * 30f;
                iconRect.localPosition = originalPos + new Vector3(0, jumpHeight, 0);

                float pulseScale = 1f + Mathf.Sin(progress * Mathf.PI * 3) * 0.3f;
                iconRect.localScale = originalScale * pulseScale;

                Color glowColor = Color.Lerp(
                    Color.white,
                    new Color(1f, 0.9f, 0f, 1f),
                    Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 3))
                );
                levelIcon.color = glowColor;

                yield return null;
            }

            iconRect.localPosition = originalPos;
            iconRect.localScale = originalScale;
            levelIcon.color = Color.white;
        }

        yield return StartCoroutine(ResetXpBarForNewLevel());
    }

    private IEnumerator ResetXpBarForNewLevel()
    {
        Debug.Log("🔄 Обновляю XP бар для нового уровня...");

        CalculateLevel();
        UpdateUI();

        yield return null;

        Debug.Log("✅ XP бар обновлен для уровня " + currentLevel);
    }

    private IEnumerator AnimateXpBar(int expGained)
    {
        int expNeeded = expRequiredForLevel[currentLevel];
        int expPrevious = (currentLevel > 1) ? expRequiredForLevel[currentLevel - 1] : 0;

        int startExp = displayedExp;
        int endExp = currentExp - expPrevious;
        if (endExp > expNeeded - expPrevious)
            endExp = expNeeded - expPrevious;

        float animationDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            displayedExp = Mathf.RoundToInt(Mathf.Lerp(startExp, endExp, progress));

            int expForCurrentLevel = expNeeded - expPrevious;
            float fillPercent = (float)displayedExp / expForCurrentLevel;
            fillPercent = Mathf.Clamp01(fillPercent);
            UpdateXpBar(fillPercent);

            UpdateExpText();

            yield return null;
        }

        displayedExp = endExp;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = currentLevel.ToString();

        UpdateLevelIcon();

        int expNeeded = expRequiredForLevel[currentLevel];
        int expPrevious = (currentLevel > 1) ? expRequiredForLevel[currentLevel - 1] : 0;

        int expInCurrentLevel = currentExp - expPrevious;
        int expForCurrentLevel = expNeeded - expPrevious;

        float fillPercent = (float)expInCurrentLevel / expForCurrentLevel;
        fillPercent = Mathf.Clamp01(fillPercent);

        UpdateXpBar(fillPercent);
        UpdateExpText();

        Debug.Log("📊 Уровень: " + currentLevel +
                 " | Всего опыта: " + currentExp +
                 " | В этом уровне: " + expInCurrentLevel + "/" + expForCurrentLevel +
                 " | Заполнено: " + (fillPercent * 100).ToString("F1") + "%");
    }

    private void UpdateExpText()
    {
        if (expText == null) return;

        int expNeeded = expRequiredForLevel[currentLevel];
        int expPrevious = (currentLevel > 1) ? expRequiredForLevel[currentLevel - 1] : 0;

        int expInCurrentLevel = displayedExp;
        int expForCurrentLevel = expNeeded - expPrevious;

        expText.text = expInCurrentLevel + " / " + expForCurrentLevel;
    }

    private void UpdateXpBar(float fillPercent)
    {
        foreach (Image texture in xpBarTextures)
        {
            if (texture != null)
                texture.enabled = false;
        }

        int textureIndex = Mathf.RoundToInt(fillPercent * 10);
        textureIndex = Mathf.Clamp(textureIndex, 0, 10);

        if (xpBarTextures[textureIndex] != null)
        {
            xpBarTextures[textureIndex].enabled = true;
        }

        Debug.Log("XP Bar Index: " + textureIndex + "/10 (" + (fillPercent * 100).ToString("F1") + "%)");
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("PlayerLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerExp", currentExp);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentExp = PlayerPrefs.GetInt("PlayerExp", 0);

        Debug.Log("Загружен прогресс: Уровень " + currentLevel + ", Опыт " + currentExp);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetCurrentExp()
    {
        return currentExp;
    }
}