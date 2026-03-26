using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Achievement Panel")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private Button closeAchievementButton;

    [Header("Achievement 1: Deal Damage")]
    [SerializeField] private Image damageAchievImage;
    [SerializeField] private Sprite[] damageAchievSprites; // 6 sprites: index = claimed levels (0-5)
    [SerializeField] private Text damageAchievTitle;
    [SerializeField] private Text damageProgressText;
    [SerializeField] private Button damageClaimButton;
    [SerializeField] private Button damageUnavailableButton;

    [Header("Achievement 2: Spend Coins")]
    [SerializeField] private Image spendAchievImage;
    [SerializeField] private Sprite[] spendAchievSprites; // 6 sprites: index = claimed levels (0-5)
    [SerializeField] private Text spendAchievTitle;
    [SerializeField] private Text spendProgressText;
    [SerializeField] private Button spendClaimButton;
    [SerializeField] private Button spendUnavailableButton;

    [Header("Achievement 3: Rarity Items")]
    [SerializeField] private Image rarityAchievImage;
    [SerializeField] private Sprite[] rarityAchievSprites; // 6 sprites: index = claimed levels (0-5)
    [SerializeField] private Text rarityAchievTitle;
    [SerializeField] private Text rarityProgressText;
    [SerializeField] private Button rarityClaimButton;
    [SerializeField] private Button rarityUnavailableButton;

    [Header("Achievement 4: Dodge / Enemy Miss")]
    [SerializeField] private Image missAchievImage;
    [SerializeField] private Sprite[] missAchievSprites; // 6 sprites: index = claimed levels (0-5)
    [SerializeField] private Text missAchievTitle;
    [SerializeField] private Text missProgressText;
    [SerializeField] private Button missClaimButton;
    [SerializeField] private Button missUnavailableButton;

    private static readonly string[] RarityNames = { "Обычный", "Редкий", "Эпический", "Легендарный", "Мифический" };

    private CanvasGroup panelCanvasGroup;
    private RectTransform panelRectTransform;
    private bool isAnimating = false;

    private AchievementSystem achievementSystem;
    private EquipmentUIManager equipmentUIManager;

    private void Start()
    {
        achievementSystem = FindObjectOfType<AchievementSystem>();
        if (achievementSystem == null)
            Debug.LogError("AchievementSystem не найден в сцене! Добавьте его на GameObject.");
        else
            Debug.Log("[AchievementUIManager] Start(): achievementSystem инициализирован OK — " + achievementSystem.gameObject.name);

        equipmentUIManager = FindObjectOfType<EquipmentUIManager>();

        if (achievementPanel != null)
        {
            panelCanvasGroup = achievementPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = achievementPanel.AddComponent<CanvasGroup>();

            panelRectTransform = achievementPanel.GetComponent<RectTransform>();
            panelCanvasGroup.alpha = 0f;
            achievementPanel.SetActive(false);
        }

        if (closeAchievementButton != null)
            closeAchievementButton.onClick.AddListener(CloseAchievements);

        if (damageClaimButton != null)
            damageClaimButton.onClick.AddListener(ClaimDamageReward);

        if (spendClaimButton != null)
            spendClaimButton.onClick.AddListener(ClaimSpendReward);

        if (rarityClaimButton != null)
            rarityClaimButton.onClick.AddListener(ClaimRarityReward);

        if (missClaimButton != null)
            missClaimButton.onClick.AddListener(ClaimMissReward);
    }

    // ─── Open / Close ─────────────────────────────────────────────────────────

    public void OpenAchievements()
    {
        Debug.Log("[AchievementUIManager] OpenAchievements() вызван. achievementPanel: " + (achievementPanel == null ? "NULL" : "OK") + ", activeSelf: " + (achievementPanel != null && achievementPanel.activeSelf) + ", isAnimating: " + isAnimating);
        if (achievementPanel == null || achievementPanel.activeSelf || isAnimating)
            return;

        achievementPanel.SetActive(true);
        RefreshUI();
        StartCoroutine(AnimateOpen());
    }

    public void CloseAchievements()
    {
        if (!isAnimating)
            StartCoroutine(AnimateClose());
    }

    // ─── UI Refresh ───────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        Debug.Log("[AchievementUIManager] RefreshUI() вызван. achievementSystem: " + (achievementSystem == null ? "NULL" : "OK"));
        if (achievementSystem == null) return;
        UpdateDamageAchievement();
        UpdateSpendAchievement();
        UpdateRarityAchievement();
        UpdateMissAchievement();
    }

    private void UpdateDamageAchievement()
    {
        int level = achievementSystem.GetDamageLevel();
        float totalDamage = achievementSystem.GetTotalDamage();

        Debug.Log("[AchievementUIManager] UpdateDamageAchievement(): level=" + level + ", totalDamage=" + totalDamage);

        // Sprite
        if (damageAchievImage != null && damageAchievSprites != null && damageAchievSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(level, 0, damageAchievSprites.Length - 1);
            if (damageAchievSprites[spriteIndex] != null)
            {
                damageAchievImage.sprite = damageAchievSprites[spriteIndex];
                Debug.Log("[AchievementUIManager] damageAchievImage.sprite обновлён, spriteIndex=" + spriteIndex);
            }
            else
                Debug.LogWarning("[AchievementUIManager] damageAchievSprites[" + spriteIndex + "] == null, спрайт не назначен.");
        }
        else
            Debug.LogWarning("[AchievementUIManager] damageAchievImage или damageAchievSprites не назначены. damageAchievImage=" + (damageAchievImage == null ? "NULL" : "OK") + ", damageAchievSprites=" + (damageAchievSprites == null ? "NULL" : "длина " + damageAchievSprites.Length));

        // Progress text
        if (damageProgressText != null)
        {
            if (level >= AchievementSystem.DamageTargets.Length)
                damageProgressText.text = "Выполнено!";
            else
                damageProgressText.text = ((int)totalDamage).ToString() + " / " + AchievementSystem.DamageTargets[level].ToString();
            Debug.Log("[AchievementUIManager] damageProgressText.text = \"" + damageProgressText.text + "\"");
        }
        else
            Debug.LogWarning("[AchievementUIManager] damageProgressText не назначен.");

        // Buttons
        bool allDone = level >= AchievementSystem.DamageTargets.Length;
        bool claimable = achievementSystem.IsDamageClaimable();

        Debug.Log("[AchievementUIManager] Buttons: allDone=" + allDone + ", claimable=" + claimable + ", damageClaimButton=" + (damageClaimButton == null ? "NULL" : "OK") + ", damageUnavailableButton=" + (damageUnavailableButton == null ? "NULL" : "OK"));

        if (damageClaimButton != null)
            damageClaimButton.gameObject.SetActive(claimable && !allDone);
        if (damageUnavailableButton != null)
            damageUnavailableButton.gameObject.SetActive(!claimable && !allDone);
    }

    private void UpdateSpendAchievement()
    {
        int level = achievementSystem.GetSpendLevel();
        int totalSpent = achievementSystem.GetTotalSpent();

        // Sprite
        if (spendAchievImage != null && spendAchievSprites != null && spendAchievSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(level, 0, spendAchievSprites.Length - 1);
            if (spendAchievSprites[spriteIndex] != null)
                spendAchievImage.sprite = spendAchievSprites[spriteIndex];
        }

        // Progress text
        if (spendProgressText != null)
        {
            if (level >= AchievementSystem.SpendTargets.Length)
                spendProgressText.text = "Выполнено!";
            else
                spendProgressText.text = totalSpent.ToString() + " / " + AchievementSystem.SpendTargets[level].ToString();
        }

        // Buttons
        bool allDone = level >= AchievementSystem.SpendTargets.Length;
        bool claimable = achievementSystem.IsSpendClaimable();

        if (spendClaimButton != null)
            spendClaimButton.gameObject.SetActive(claimable && !allDone);
        if (spendUnavailableButton != null)
            spendUnavailableButton.gameObject.SetActive(!claimable && !allDone);
    }

    private void UpdateRarityAchievement()
    {
        int level = achievementSystem.GetRarityLevel();
        int progress = achievementSystem.GetRarityProgress();

        // Sprite
        if (rarityAchievImage != null && rarityAchievSprites != null && rarityAchievSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(level, 0, rarityAchievSprites.Length - 1);
            if (rarityAchievSprites[spriteIndex] != null)
                rarityAchievImage.sprite = rarityAchievSprites[spriteIndex];
        }

        // Progress text: show next rarity to unlock, or "Выполнено!"
        if (rarityProgressText != null)
        {
            if (level >= AchievementSystem.RarityRewards.Length)
            {
                rarityProgressText.text = "Выполнено!";
            }
            else
            {
                string nextRarity = (level < RarityNames.Length) ? RarityNames[level] : "";
                rarityProgressText.text = (progress > level ? "✓" : "") + nextRarity;
            }
        }

        // Buttons
        bool allDone = level >= AchievementSystem.RarityRewards.Length;
        bool claimable = achievementSystem.IsRarityClaimable();

        if (rarityClaimButton != null)
            rarityClaimButton.gameObject.SetActive(claimable && !allDone);
        if (rarityUnavailableButton != null)
            rarityUnavailableButton.gameObject.SetActive(!claimable && !allDone);
    }

    private void UpdateMissAchievement()
    {
        int level = achievementSystem.GetMissLevel();
        int totalMisses = achievementSystem.GetTotalMisses();

        // Sprite
        if (missAchievImage != null && missAchievSprites != null && missAchievSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(level, 0, missAchievSprites.Length - 1);
            if (missAchievSprites[spriteIndex] != null)
                missAchievImage.sprite = missAchievSprites[spriteIndex];
        }

        // Progress text
        if (missProgressText != null)
        {
            if (level >= AchievementSystem.MissTargets.Length)
                missProgressText.text = "Выполнено!";
            else
                missProgressText.text = totalMisses.ToString() + " / " + AchievementSystem.MissTargets[level].ToString();
        }

        // Buttons
        bool allDone = level >= AchievementSystem.MissTargets.Length;
        bool claimable = achievementSystem.IsMissClaimable();

        if (missClaimButton != null)
            missClaimButton.gameObject.SetActive(claimable && !allDone);
        if (missUnavailableButton != null)
            missUnavailableButton.gameObject.SetActive(!claimable && !allDone);
    }

    // ─── Claim Handlers ───────────────────────────────────────────────────────

    private void ClaimDamageReward()
    {
        if (achievementSystem == null) return;

        int oldLevel = achievementSystem.GetDamageLevel();
        achievementSystem.ClaimDamageReward();
        int newLevel = achievementSystem.GetDamageLevel();

        if (newLevel != oldLevel && damageAchievImage != null
            && damageAchievSprites != null && newLevel < damageAchievSprites.Length)
        {
            StartCoroutine(AnimateTextureChange(damageAchievImage, damageAchievSprites[newLevel]));
        }

        RefreshUI();

        if (equipmentUIManager != null)
            equipmentUIManager.UpdateCoinsUI();
    }

    private void ClaimSpendReward()
    {
        if (achievementSystem == null) return;

        int oldLevel = achievementSystem.GetSpendLevel();
        achievementSystem.ClaimSpendReward();
        int newLevel = achievementSystem.GetSpendLevel();

        if (newLevel != oldLevel && spendAchievImage != null
            && spendAchievSprites != null && newLevel < spendAchievSprites.Length)
        {
            StartCoroutine(AnimateTextureChange(spendAchievImage, spendAchievSprites[newLevel]));
        }

        RefreshUI();
    }

    private void ClaimRarityReward()
    {
        if (achievementSystem == null) return;

        int oldLevel = achievementSystem.GetRarityLevel();
        achievementSystem.ClaimRarityReward();
        int newLevel = achievementSystem.GetRarityLevel();

        if (newLevel != oldLevel && rarityAchievImage != null
            && rarityAchievSprites != null && newLevel < rarityAchievSprites.Length)
        {
            StartCoroutine(AnimateTextureChange(rarityAchievImage, rarityAchievSprites[newLevel]));
        }

        RefreshUI();

        if (equipmentUIManager != null)
            equipmentUIManager.UpdateCoinsUI();
    }

    private void ClaimMissReward()
    {
        if (achievementSystem == null) return;

        int oldLevel = achievementSystem.GetMissLevel();
        achievementSystem.ClaimMissReward();
        int newLevel = achievementSystem.GetMissLevel();

        if (newLevel != oldLevel && missAchievImage != null
            && missAchievSprites != null && newLevel < missAchievSprites.Length)
        {
            StartCoroutine(AnimateTextureChange(missAchievImage, missAchievSprites[newLevel]));
        }

        RefreshUI();

        if (equipmentUIManager != null)
            equipmentUIManager.UpdateCoinsUI();
    }

    // ─── Animations ───────────────────────────────────────────────────────────

    private IEnumerator AnimateTextureChange(Image image, Sprite newSprite)
    {
        if (image == null) yield break;

        float duration = 0.3f;
        float elapsed = 0f;

        // Fade out
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = image.color;
            c.a = 1f - (elapsed / duration);
            image.color = c;
            yield return null;
        }

        if (newSprite != null)
            image.sprite = newSprite;

        // Fade in + scale up
        elapsed = 0f;
        image.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            Color c = image.color;
            c.a = progress;
            image.color = c;
            image.transform.localScale = Vector3.Lerp(new Vector3(0.8f, 0.8f, 1f), Vector3.one, progress);
            yield return null;
        }

        Color finalColor = image.color;
        finalColor.a = 1f;
        image.color = finalColor;
        image.transform.localScale = Vector3.one;
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;

        if (panelRectTransform != null)
            panelRectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = progress;

            if (panelRectTransform != null)
                panelRectTransform.localScale = Vector3.Lerp(new Vector3(0.8f, 0.8f, 1f), Vector3.one, progress);

            yield return null;
        }

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;
        if (panelRectTransform != null)
            panelRectTransform.localScale = Vector3.one;

        isAnimating = false;
    }

    private IEnumerator AnimateClose()
    {
        isAnimating = true;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = 1f - progress;

            if (panelRectTransform != null)
                panelRectTransform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.8f, 0.8f, 1f), progress);

            yield return null;
        }

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;
        if (achievementPanel != null)
            achievementPanel.SetActive(false);

        isAnimating = false;
    }
}