using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class EquipmentUIManager : MonoBehaviour
{
    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text expText;
    [SerializeField] private Text powerText;
    [SerializeField] private Button fightButton;

    [SerializeField] private Image weaponSlot;
    [SerializeField] private Image headSlot;
    [SerializeField] private Image bodySlot;
    [SerializeField] private Image legsSlot;

    [SerializeField] private Button weaponSlotButton;
    [SerializeField] private Button headSlotButton;
    [SerializeField] private Button bodySlotButton;
    [SerializeField] private Button legsSlotButton;

    [SerializeField] private GameObject equipmentListPanel;
    [SerializeField] private Transform equipmentListContent;
    [SerializeField] private GameObject equipmentButtonPrefab;
    [SerializeField] private Button closeEquipmentListButton;

    [SerializeField] private Equipment[] availableEquipments;

    [SerializeField] private AudioClip commonEquipSound;
    [SerializeField] private AudioClip rareEquipSound;
    [SerializeField] private AudioClip epicEquipSound;
    [SerializeField] private AudioClip legendaryEquipSound;
    [SerializeField] private AudioClip mythicalEquipSound;
    [SerializeField] private AudioClip equipmentMenuMusic;

    [SerializeField] private AudioClip stage1RageSound;
    [SerializeField] private Image stage1RageImage;
    [SerializeField] private Text stage1RageText;
    [SerializeField] private AudioClip stage2RageSound;
    [SerializeField] private Image stage2RageImage;
    [SerializeField] private Text stage2RageText;
    [SerializeField] private AudioClip stage3RageSound;
    [SerializeField] private Image stage3RageImage;
    [SerializeField] private Text stage3RageText;

    [SerializeField] private Image fadePanel;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private AchievementUIManager achievementUIManager;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private AudioClip[] stageRageSounds = new AudioClip[5];
    [SerializeField] private Image[] stageRageImages = new Image[5];
    [SerializeField] private Text[] stageRageTexts = new Text[5];

    [SerializeField] private Button characterSelectButton;       // "Выбрать персонажа" button
    [SerializeField] private Image characterDisplayImage;
[SerializeField] private GameObject characterSelectionPanel;
    [SerializeField] private KnightAnimator knightAnimator;      // menu character animator

    private int coins = 0;
    private int exp = 0;
    private EquipmentType currentSelectedSlot;
    private List<GameObject> instantiatedButtons = new List<GameObject>();

    private Sprite defaultWeaponSprite;
    private Sprite defaultHeadSprite;
    private Sprite defaultBodySprite;
    private Sprite defaultLegsSprite;

    private AudioSource audioSource;
    private AudioSource musicSource;

    private int currentStage = 0;
    private AchievementSystem achievementSystem;

    private void Start()
    {
        defaultWeaponSprite = weaponSlot.sprite;
        defaultHeadSprite = headSlot.sprite;
        defaultBodySprite = bodySlot.sprite;
        defaultLegsSprite = legsSlot.sprite;
        LoadEquipmentPurchases();

        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        exp = PlayerPrefs.GetInt("PlayerExp", 0);
        currentStage = PlayerPrefs.GetInt("CurrentStage", 0);

        Debug.Log("Загружены монеты: " + coins + ", опыт: " + exp + ", этаж: " + currentStage);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        if (equipmentMenuMusic != null)
        {
            musicSource.clip = equipmentMenuMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.Play();
        }

        if (openShopButton != null)
            openShopButton.onClick.AddListener(() => shopManager.OpenShop());

        if (achievementsButton != null && achievementUIManager != null)
            achievementsButton.onClick.AddListener(() => achievementUIManager.OpenAchievements());

        if (characterSelectButton != null && characterSelectionPanel != null)
            characterSelectButton.onClick.AddListener(() => characterSelectionPanel.gameObject.SetActive(true));

        if (characterSelectionPanel != null)
            characterSelectionPanel.gameObject.SetActive(false);

        CharacterDetailPanel detailPanel = characterSelectionPanel.GetComponentInChildren<CharacterDetailPanel>();
        if (detailPanel != null)
            detailPanel.gameObject.SetActive(false);

        PurchaseConfirmPanel purchasePanel = characterSelectionPanel.GetComponentInChildren<PurchaseConfirmPanel>();
        if (purchasePanel != null)
            purchasePanel.gameObject.SetActive(false);

        achievementSystem = FindObjectOfType<AchievementSystem>();

        // Refresh CharacterManager data and apply current character sprites to menu animator
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.RefreshData();
            if (knightAnimator != null)
            {
                CharacterData charData = CharacterManager.Instance.GetCurrentCharacter();
                if (charData != null && charData.idleSprites != null && charData.idleSprites.Length > 0)
                    knightAnimator.SetIdleSprites(charData.idleSprites);
            }
        }

        if (fadePanel == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject fadePanelObj = new GameObject("FadePanel");
                fadePanelObj.transform.SetParent(canvas.transform, false);
                fadePanelObj.transform.SetAsLastSibling();
                fadePanel = fadePanelObj.AddComponent<Image>();
                fadePanel.color = Color.clear;
                fadePanel.raycastTarget = false;
                RectTransform rectTransform = fadePanelObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }

        fadePanel.color = Color.clear;
        fadePanel.raycastTarget = false;

        UpdateCoinsUI();
        UpdateStatsUI();

        weaponSlotButton.onClick.AddListener(() => ShowEquipmentList(EquipmentType.Weapon));
        headSlotButton.onClick.AddListener(() => ShowEquipmentList(EquipmentType.Head));
        bodySlotButton.onClick.AddListener(() => ShowEquipmentList(EquipmentType.Body));
        legsSlotButton.onClick.AddListener(() => ShowEquipmentList(EquipmentType.Legs));

        fightButton.onClick.AddListener(GoToBattle);
        closeEquipmentListButton.onClick.AddListener(CloseEquipmentList);

        LoadEquipmentData();
        UpdateEquipmentUI();
        equipmentListPanel.SetActive(false);

        StartCoroutine(TriggerXpAnimationIfNeeded());
    }

    private IEnumerator TriggerXpAnimationIfNeeded()
    {
        yield return new WaitForSeconds(0.5f);

        if (PlayerPrefs.HasKey("JustFinishedBattle"))
        {
            int expReward = PlayerPrefs.GetInt("LastBattleExpReward", 0);

            Debug.Log("🎯 Запускаю анимацию XP: +" + expReward);

            LevelSystem levelSystem = FindObjectOfType<LevelSystem>();
            if (levelSystem != null && expReward > 0)
            {
                levelSystem.ShowXpGainAnimation(expReward);
            }

            PlayerPrefs.DeleteKey("JustFinishedBattle");
            PlayerPrefs.DeleteKey("LastBattleExpReward");
            PlayerPrefs.Save();
        }
    }

    private void UpdateEquipmentUI()
    {
        Equipment weapon = playerEquipment.GetEquipped(EquipmentType.Weapon);
        Equipment head = playerEquipment.GetEquipped(EquipmentType.Head);
        Equipment body = playerEquipment.GetEquipped(EquipmentType.Body);
        Equipment legs = playerEquipment.GetEquipped(EquipmentType.Legs);

        if (weapon != null)
        {
            weaponSlot.sprite = weapon.icon;
            weaponSlot.color = Color.white;
            ApplyRarityGlow(weaponSlot, weapon.rarity);
        }
        else
        {
            weaponSlot.sprite = defaultWeaponSprite;
            weaponSlot.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            RemoveRarityGlow(weaponSlot);
        }

        if (head != null)
        {
            headSlot.sprite = head.icon;
            headSlot.color = Color.white;
            ApplyRarityGlow(headSlot, head.rarity);
        }
        else
        {
            headSlot.sprite = defaultHeadSprite;
            headSlot.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            RemoveRarityGlow(headSlot);
        }

        if (body != null)
        {
            bodySlot.sprite = body.icon;
            bodySlot.color = Color.white;
            ApplyRarityGlow(bodySlot, body.rarity);
        }
        else
        {
            bodySlot.sprite = defaultBodySprite;
            bodySlot.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            RemoveRarityGlow(bodySlot);
        }

        if (legs != null)
        {
            legsSlot.sprite = legs.icon;
            legsSlot.color = Color.white;
            ApplyRarityGlow(legsSlot, legs.rarity);
        }
        else
        {
            legsSlot.sprite = defaultLegsSprite;
            legsSlot.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            RemoveRarityGlow(legsSlot);
        }

        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        int newPower = playerEquipment.GetTotalPower();
        int oldPower = int.Parse(powerText.text);

        if (newPower != oldPower)
        {
            // ← ЗАПУСКАЕМ АНИМАЦИЮ МОЩИ
            StartCoroutine(AnimatePowerChange(oldPower, newPower));
        }
    }

    private void ApplyRarityGlow(Image slot, Rarity rarity)
    {
        RarityGlowEffect glow = slot.gameObject.GetComponent<RarityGlowEffect>();
        if (glow == null)
            glow = slot.gameObject.AddComponent<RarityGlowEffect>();
        glow.Initialize(rarity);
    }

    private void RemoveRarityGlow(Image slot)
    {
        RarityGlowEffect glow = slot.gameObject.GetComponent<RarityGlowEffect>();
        if (glow != null)
            Destroy(glow);
    }

    public void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coins = PlayerPrefs.GetInt("PlayerCoins", 0);
            coinsText.text = coins.ToString();
            Debug.Log("💰 Монеты обновлены: " + coins);
        }
    }

    public void SpendCoins(int amount)
    {
        int oldCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        coins = oldCoins - amount;
        if (coins < 0) coins = 0;

        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();

        // Track for spend-coins achievement
        if (achievementSystem != null)
            achievementSystem.AddSpentCoins(amount);

        StartCoroutine(AnimateCoinChange(oldCoins, coins));
        Debug.Log("💸 Потрачено " + amount + " монет. Осталось: " + coins);
    }

    public void AddCoins(int amount)
    {
        int oldCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        coins = oldCoins + amount;

        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();

        StartCoroutine(AnimateCoinChange(oldCoins, coins));
        Debug.Log("💰 Добавлено " + amount + " монет. Всего: " + coins);
    }

    private IEnumerator AnimatePowerChange(int oldPower, int newPower)
    {
        yield return StartCoroutine(AnimateNumberText(powerText, oldPower, newPower));
    }

    private IEnumerator AnimateCoinChange(int oldCoins, int newCoins)
    {
        if (coinsText == null) yield break;
        yield return StartCoroutine(AnimateNumberText(coinsText, oldCoins, newCoins));
    }

    private IEnumerator AnimateNumberText(Text textComponent, int oldValue, int newValue)
    {
        if (textComponent == null) yield break;

        float duration = 0.8f;
        float elapsed = 0f;

        bool isIncrease = newValue > oldValue;
        Color targetColor = isIncrease ? Color.green : Color.red;
        Color originalColor = textComponent.color;

        textComponent.color = targetColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            int current = Mathf.RoundToInt(Mathf.Lerp(oldValue, newValue, progress));
            textComponent.text = current.ToString();

            yield return null;
        }

        textComponent.text = newValue.ToString();
        textComponent.color = originalColor;
    }

    private void ShowEquipmentList(EquipmentType type)
    {
        currentSelectedSlot = type;
        equipmentListPanel.SetActive(true);

        foreach (GameObject button in instantiatedButtons)
        {
            Destroy(button);
        }
        instantiatedButtons.Clear();

        foreach (Equipment equipment in availableEquipments)
        {
            if (equipment == null) continue;

            if (!equipment.isPurchased && equipment.price > 0) continue;

            if (equipment.type == type)
            {
                GameObject buttonObject = Instantiate(equipmentButtonPrefab, equipmentListContent);
                EquipmentItemButton itemButton = buttonObject.GetComponent<EquipmentItemButton>();

                if (itemButton != null)
                {
                    itemButton.Initialize(equipment, this);
                    itemButton.SetEquipmentIcon(equipment.icon);
                }

                instantiatedButtons.Add(buttonObject);
            }
        }
    }

    public void RefreshEquipmentList()
    {
        if (equipmentListPanel.activeSelf)
        {
            ShowEquipmentList(currentSelectedSlot);
        }
    }

    public Equipment[] GetAvailableEquipments()
    {
        return availableEquipments;
    }

    public void EquipItemDirect(Equipment equipment)
    {
        playerEquipment.EquipItem(equipment);
        StartCoroutine(PlayEquipAnimation(equipment));
        UpdateEquipmentUI();
        CloseEquipmentList();

        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        UpdateCoinsUI();

        // Track rarity achievement
        if (achievementSystem != null)
            achievementSystem.AddRarityItem(equipment.rarity);

        // ← СОХРАНЯЕМ СНАРЯЖЕНИЕ
        SaveEquipmentData();
        PlayerPrefs.Save();
    }

    private IEnumerator PlayEquipAnimation(Equipment equipment)
    {
        Image targetSlot = null;

        switch (equipment.type)
        {
            case EquipmentType.Weapon:
                targetSlot = weaponSlot;
                break;
            case EquipmentType.Head:
                targetSlot = headSlot;
                break;
            case EquipmentType.Body:
                targetSlot = bodySlot;
                break;
            case EquipmentType.Legs:
                targetSlot = legsSlot;
                break;
        }

        if (targetSlot == null)
            yield break;

        PlayEquipSound(equipment.rarity);

        yield return StartCoroutine(AnimatePlacementSmooth(targetSlot, equipment.rarity));
    }

    private IEnumerator AnimatePlacementSmooth(Image targetSlot, Rarity rarity)
    {
        const float duration = 0.35f;
        Color rarityColor;
        switch (rarity)
        {
            case Rarity.Rare:      rarityColor = new Color(1f, 0.6f, 0f);   break;
            case Rarity.Epic:      rarityColor = new Color(0.8f, 0.2f, 1f); break;
            case Rarity.Legendary: rarityColor = new Color(1f, 0.84f, 0f);  break;
            case Rarity.Mythical:  rarityColor = new Color(1f, 0.2f, 0.2f); break;
            default:               rarityColor = Color.white;                break;
        }

        Color originalColor = targetSlot.color;
        float elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            targetSlot.color = Color.Lerp(originalColor, rarityColor, elapsed / (duration / 2f));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            targetSlot.color = Color.Lerp(rarityColor, originalColor, elapsed / (duration / 2f));
            yield return null;
        }

        targetSlot.color = originalColor;
    }

    private void PlayEquipSound(Rarity rarity)
    {
        AudioClip soundToPlay = null;

        switch (rarity)
        {
            case Rarity.Common:
                soundToPlay = commonEquipSound;
                break;
            case Rarity.Rare:
                soundToPlay = rareEquipSound;
                break;
            case Rarity.Epic:
                soundToPlay = epicEquipSound;
                break;
            case Rarity.Legendary:
                soundToPlay = legendaryEquipSound;
                break;
            case Rarity.Mythical:
                soundToPlay = mythicalEquipSound;
                break;
        }

        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay, 0.8f);
        }
    }

    private IEnumerator FlashSlotColor(Image slot, Color rarityColor, float duration)
    {
        Color originalColor = slot.color;
        float elapsed = 0f;

        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration / 2);
            slot.color = Color.Lerp(originalColor, rarityColor, progress);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (duration / 2);
            slot.color = Color.Lerp(rarityColor, originalColor, progress);
            yield return null;
        }

        slot.color = originalColor;
    }

    public void CloseEquipmentList()
    {
        equipmentListPanel.SetActive(false);
    }

    // ✅ НОВЫЙ КОД - ТОЛЬКО СОХРАНЕНИЕ

    private void GoToBattle()
    {
        int totalPower = playerEquipment.GetTotalPower();
        PlayerPrefs.SetInt("PlayerPower", totalPower);

        SaveEquipmentData();
        SavePlayerStats();
        PlayerPrefs.Save();

        Debug.Log("💾 Данные сохран��ны, показываю выбор уровней...");

        // ← ПРОСТО ПОКАЗЫВАЕМ ПАНЕЛЬ УРОВНЕЙ, БЕЗ ЗАПУСКА ИГРЫ!
        // TransitionToBattle() будет вызван из StageButton.cs
    }

    // ← НОВЫЙ МЕТОД: ЗАПУСК БИТВЫ С АНИМАЦИЕЙ
    public void StartBattleTransition()
    {
        StartCoroutine(TransitionToBattle());
    }

    // ← НОВЫЙ МЕТОД: СОХРАНЕНИЕ СТАТИСТИКИ ИГРОКА
    private void SavePlayerStats()
    {
        int currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        int currentExp = PlayerPrefs.GetInt("PlayerExp", 0);
        int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        int currentStage = PlayerPrefs.GetInt("CurrentStage", 0);

        Debug.Log("📊 Сохраняю статистику:");
        Debug.Log("   Level: " + currentLevel);
        Debug.Log("   Exp: " + currentExp);
        Debug.Log("   Coins: " + currentCoins);
        Debug.Log("   Stage: " + currentStage);

        PlayerPrefs.SetInt("PlayerLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerExp", currentExp);
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.SetInt("CurrentStage", currentStage);
    }

    private IEnumerator TransitionToBattle()
    {
        float fadeDuration = 0.6f;
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, progress);
            yield return null;
        }

        musicSource.volume = 0;
        musicSource.Stop();

        yield return StartCoroutine(FadeToBlack(0.6f));
        yield return StartCoroutine(ShowStageImage(9f));

        SceneManager.LoadScene("BattleScene");
    }

    private IEnumerator FadeToBlack(float duration)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.raycastTarget = true;

        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 1);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            fadePanel.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        fadePanel.color = endColor;
    }

    private IEnumerator ShowStageImage(float duration)
    {
        Image stageImage = null;
        Text stageText = null;
        AudioClip stageSound = null;

        currentStage = PlayerPrefs.GetInt("CurrentStage", 0);

        // ← ИСПОЛЬЗУЕМ МАССИВЫ ВМЕСТО SWITCH
        if (currentStage >= 0 && currentStage < stageRageImages.Length)
        {
            stageImage = stageRageImages[currentStage];
            stageText = stageRageTexts[currentStage];
            stageSound = stageRageSounds[currentStage];
        }

        if (stageImage == null)
        {
            Debug.LogError("Stage image не назначена для этажа " + (currentStage + 1));
            yield break;
        }

        Color imageColor = stageImage.color;
        imageColor.a = 0;
        stageImage.color = imageColor;
        stageImage.raycastTarget = false;

        if (!stageImage.gameObject.activeInHierarchy)
        {
            stageImage.gameObject.SetActive(true);
        }

        if (stageText != null && !stageText.gameObject.activeInHierarchy)
        {
            stageText.gameObject.SetActive(true);
        }

        if (stageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stageSound, 1f);
        }

        float elapsed = 0f;
        float scaleUpDuration = 0.5f;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleUpDuration;

            imageColor = stageImage.color;
            imageColor.a = Mathf.Pow(progress, 2);
            stageImage.color = imageColor;

            float scale = 0.5f + (progress * 0.5f);
            stageImage.transform.localScale = new Vector3(scale, scale, 1);

            yield return null;
        }

        imageColor = stageImage.color;
        imageColor.a = 1;
        stageImage.color = imageColor;
        stageImage.transform.localScale = Vector3.one;

        if (stageText != null)
        {
            Color textColor = stageText.color;
            textColor.a = 1;
            stageText.color = textColor;
        }

        float pulseElapsed = 0f;
        float pulseDuration = 0.5f;

        while (pulseElapsed < pulseDuration)
        {
            pulseElapsed += Time.deltaTime;
            float pulseProgress = pulseElapsed / pulseDuration;

            float pulseScale = 1 + Mathf.Sin(pulseProgress * Mathf.PI * 2) * 0.1f;
            stageImage.transform.localScale = new Vector3(pulseScale, pulseScale, 1);

            yield return null;
        }

        stageImage.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(duration - scaleUpDuration - pulseDuration);

        elapsed = 0f;
        float fadeOutDuration = 1f;
        Color textColor2 = Color.white;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;

            imageColor = stageImage.color;
            imageColor.a = 1 - progress;
            stageImage.color = imageColor;

            if (stageText != null)
            {
                textColor2 = stageText.color;
                textColor2.a = 1 - progress;
                stageText.color = textColor2;
            }

            stageImage.transform.localScale = new Vector3(1 - (progress * 0.3f), 1 - (progress * 0.3f), 1);

            yield return null;
        }

        imageColor = stageImage.color;
        imageColor.a = 0;
        stageImage.color = imageColor;

        if (stageText != null)
        {
            stageText.gameObject.SetActive(false);
        }
        stageImage.gameObject.SetActive(false);
    }

    private void SaveEquipmentData()
    {
        Equipment weapon = playerEquipment.GetEquipped(EquipmentType.Weapon);
        Equipment head = playerEquipment.GetEquipped(EquipmentType.Head);
        Equipment body = playerEquipment.GetEquipped(EquipmentType.Body);
        Equipment legs = playerEquipment.GetEquipped(EquipmentType.Legs);

        PlayerPrefs.SetString("EquippedWeapon", weapon != null ? weapon.equipmentName : "");
        PlayerPrefs.SetString("EquippedHead", head != null ? head.equipmentName : "");
        PlayerPrefs.SetString("EquippedBody", body != null ? body.equipmentName : "");
        PlayerPrefs.SetString("EquippedLegs", legs != null ? legs.equipmentName : "");

        Debug.Log("💾 Снаряжение сохранено!");
    }

    private void LoadEquipmentPurchases()
    {
        if (availableEquipments == null) return;

        foreach (Equipment eq in availableEquipments)
        {
            if (eq == null) continue;

            string key = "Purchased_" + eq.equipmentName;
            if (PlayerPrefs.HasKey(key))
            {
                eq.isPurchased = true;
                Debug.Log("✅ Загружена ��окупка: " + eq.equipmentName);
            }
        }

        Debug.Log("💾 Все покупки загружены!");
    }

    private void LoadEquipmentData()
    {
        string weaponName = PlayerPrefs.GetString("EquippedWeapon", "");
        string headName = PlayerPrefs.GetString("EquippedHead", "");
        string bodyName = PlayerPrefs.GetString("EquippedBody", "");
        string legsName = PlayerPrefs.GetString("EquippedLegs", "");

        if (!string.IsNullOrEmpty(weaponName))
        {
            Equipment weapon = System.Array.Find(availableEquipments, eq => eq.equipmentName == weaponName);
            if (weapon != null)
            {
                weapon.isPurchased = true;
                playerEquipment.EquipItem(weapon);
            }
        }

        if (!string.IsNullOrEmpty(headName))
        {
            Equipment head = System.Array.Find(availableEquipments, eq => eq.equipmentName == headName);
            if (head != null)
            {
                head.isPurchased = true;
                playerEquipment.EquipItem(head);
            }
        }

        if (!string.IsNullOrEmpty(bodyName))
        {
            Equipment body = System.Array.Find(availableEquipments, eq => eq.equipmentName == bodyName);
            if (body != null)
            {
                body.isPurchased = true;
                playerEquipment.EquipItem(body);
            }
        }

        if (!string.IsNullOrEmpty(legsName))
        {
            Equipment legs = System.Array.Find(availableEquipments, eq => eq.equipmentName == legsName);
            if (legs != null)
            {
                legs.isPurchased = true;
                playerEquipment.EquipItem(legs);
            }
        }

        Debug.Log("💾 Снаряжение загружено!");
    }
}