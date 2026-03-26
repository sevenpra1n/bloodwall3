using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopContent;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Button closeShopButton;
    [SerializeField] private Text coinsDisplay;
    [SerializeField] private Text slotTypeText; // ← НОВОЕ

    // ← КНОПКИ ФИЛЬТРА
    [SerializeField] private Button weaponFilterButton;
    [SerializeField] private Button headFilterButton;
    [SerializeField] private Button bodyFilterButton;
    [SerializeField] private Button legsFilterButton;

    [SerializeField] private EquipmentUIManager uiManager;

    private int playerCoins = 0;
    private CanvasGroup shopCanvasGroup;
    private AudioSource audioSource;
    private RectTransform shopRectTransform;

    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;

    private bool isAnimating = false;
    private EquipmentType currentFilter = EquipmentType.Weapon; // ← ТЕКУЩИЙ ФИЛЬТР

    private void Start()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopPanel не привязан в Inspector!");
            return;
        }

        shopCanvasGroup = shopPanel.GetComponent<CanvasGroup>();
        if (shopCanvasGroup == null)
            shopCanvasGroup = shopPanel.AddComponent<CanvasGroup>();

        shopRectTransform = shopPanel.GetComponent<RectTransform>();

        shopCanvasGroup.alpha = 0;
        shopPanel.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (closeShopButton != null)
            closeShopButton.onClick.AddListener(CloseShop);

        playerCoins = PlayerPrefs.GetInt("PlayerCoins", 0);

        // ← ПРИВЯЗЫВАЕМ КНОПКИ ФИЛЬТРА
        if (weaponFilterButton != null)
            weaponFilterButton.onClick.AddListener(() => FilterByType(EquipmentType.Weapon));
        if (headFilterButton != null)
            headFilterButton.onClick.AddListener(() => FilterByType(EquipmentType.Head));
        if (bodyFilterButton != null)
            bodyFilterButton.onClick.AddListener(() => FilterByType(EquipmentType.Body));
        if (legsFilterButton != null)
            legsFilterButton.onClick.AddListener(() => FilterByType(EquipmentType.Legs));
    }

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopPanel не привязан!");
            return;
        }

        if (shopPanel.activeSelf || isAnimating)
            return;

        playerCoins = PlayerPrefs.GetInt("PlayerCoins", 0);

        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        shopPanel.SetActive(true);
        currentFilter = EquipmentType.Weapon; // ← СТАРТУЕМ С ОРУЖИЯ

        RefreshShopItems();
        StartCoroutine(AnimateShopOpen());
        UpdateCoinDisplay();
    }

    // ← НОВЫЙ МЕТОД: ФИЛЬТРАЦИЯ ПО ТИПУ
    private void FilterByType(EquipmentType type)
    {
        currentFilter = type;
        RefreshShopItems();
    }

    // ← НОВЫЙ МЕТОД: ОБНОВЛЕНИЕ СПИСКА С ФИЛЬТРОМ
    private void RefreshShopItems()
    {
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        if (uiManager != null)
        {
            Equipment[] allEquipments = uiManager.GetAvailableEquipments();

            if (allEquipments != null)
            {
                int count = 0;
                foreach (Equipment eq in allEquipments)
                {
                    if (eq == null) continue;

                    // ← ФИЛЬТРУЕМ ПО ТИПУ
                    if (eq.type != currentFilter) continue;

                    // ← НЕ ПОКАЗЫВАЕМ УЖЕ КУПЛЕННЫЕ ПРЕДМЕТЫ
                    if (eq.isPurchased) continue;

                    Debug.Log("Магазин: показываю " + eq.equipmentName + " (цена: " + eq.price + ")");

                    GameObject itemObj = Instantiate(shopItemPrefab, shopContent);
                    ShopItemButton itemButton = itemObj.GetComponent<ShopItemButton>();

                    if (itemButton != null)
                    {
                        itemButton.Initialize(eq, this);
                    }
                    count++;
                }
                Debug.Log("Магазин: показано " + count + " предметов для " + currentFilter);
            }
        }

        // ← ОБНОВЛЯЕМ ТЕКСТ СЛОТА
        UpdateSlotTypeText();
    }

    // ← НОВЫЙ МЕТОД: ОБНОВЛЕНИЕ ТЕКСТА СЛОТА
    private void UpdateSlotTypeText()
    {
        if (slotTypeText == null) return;

        string slotName = "";
        switch (currentFilter)
        {
            case EquipmentType.Weapon:
                slotName = "Оружие";
                break;
            case EquipmentType.Head:
                slotName = "Голова";
                break;
            case EquipmentType.Body:
                slotName = "Тело";
                break;
            case EquipmentType.Legs:
                slotName = "Ноги";
                break;
        }

        slotTypeText.text = slotName;
        Debug.Log("Фильтр изменён на: " + slotName);
    }

    public void BuyItem(Equipment equipment, ShopItemButton itemButton)
    {
        if (equipment == null) return;

        playerCoins = PlayerPrefs.GetInt("PlayerCoins", 0);

        if (playerCoins >= equipment.price)
        {
            Debug.Log("ПОКУПКА: " + equipment.equipmentName);

            // ← ИСПОЛЬЗУЕМ EquipmentUIManager ДЛЯ ОБНОВЛЕНИЯ
            if (uiManager != null)
            {
                uiManager.SpendCoins(equipment.price);
            }
            else
            {
                playerCoins -= equipment.price;
                PlayerPrefs.SetInt("PlayerCoins", playerCoins);
                PlayerPrefs.Save();
            }

            equipment.isPurchased = true;
            SaveEquipmentPurchase(equipment);

            PlaySound(purchaseSound);

            if (itemButton != null)
            {
                Image itemImage = itemButton.GetComponent<Image>();
                if (itemImage != null)
                {
                    StartCoroutine(AnimatePurchase(itemImage, itemButton.gameObject));
                }
            }

            // ← ОБНОВЛЯЕМ ОБА ТЕКСТА
            UpdateCoinDisplay();
            if (uiManager != null)
                uiManager.UpdateCoinsUI();

            if (uiManager != null)
            {
                uiManager.RefreshEquipmentList();
            }

            Debug.Log("Предмет куплен: " + equipment.equipmentName);
        }
        else
        {
            PlaySound(errorSound);
            StartCoroutine(ShakePanel());
        }
    }

    // ← НОВЫЙ МЕТОД: СОХРАНЕНИЕ ПОКУПОК
    private void SaveEquipmentPurchase(Equipment equipment)
    {
        string key = "Purchased_" + equipment.equipmentName;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log("💾 Сохранена покупка: " + equipment.equipmentName);
    }

    // ← НОВЫЙ МЕТОД: ЗАГРУЗКА ПОКУПОК
    public void LoadEquipmentPurchases(Equipment[] allEquipments)
    {
        if (allEquipments == null) return;

        foreach (Equipment eq in allEquipments)
        {
            if (eq == null) continue;

            string key = "Purchased_" + eq.equipmentName;
            if (PlayerPrefs.HasKey(key))
            {
                eq.isPurchased = true;
                Debug.Log("✅ Загружена покупка: " + eq.equipmentName);
            }
        }
    }

    private IEnumerator AnimatePurchase(Image itemImage, GameObject itemButton)
    {
        if (itemImage == null || itemButton == null) yield break;

        RectTransform itemRect = itemImage.GetComponent<RectTransform>();
        if (itemRect == null) yield break;

        Vector3 originalPos = itemRect.localPosition;
        Vector3 originalScale = itemRect.localScale;
        CanvasGroup itemCanvasGroup = itemImage.GetComponent<CanvasGroup>();

        if (itemCanvasGroup == null)
            itemCanvasGroup = itemImage.gameObject.AddComponent<CanvasGroup>();

        Color originalColor = itemImage.color;
        Color greenColor = new Color(0.2f, 1f, 0.3f, 1f);
        Color whiteColor = new Color(1f, 1f, 1f, 1f);

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float bounceWaves = Mathf.Sin(progress * Mathf.PI * 6) * (1 - progress);
            float jumpHeight = 120f * bounceWaves;
            itemRect.localPosition = originalPos + new Vector3(0, jumpHeight, 0);

            float scaleWave = 1f + (Mathf.Sin(progress * Mathf.PI * 6) * 0.3f * (1 - progress));
            itemRect.localScale = originalScale * scaleWave;

            float colorFlash = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 8));
            Color currentColor = Color.Lerp(whiteColor, greenColor, colorFlash);
            itemImage.color = currentColor;

            itemCanvasGroup.alpha = 1f - (progress * progress);

            yield return null;
        }

        itemRect.localPosition = originalPos;
        itemRect.localScale = originalScale;
        itemCanvasGroup.alpha = 0;
        itemImage.color = originalColor;

        Destroy(itemButton);
    }

    public void CloseShop()
    {
        if (!isAnimating)
            StartCoroutine(AnimateShopClose());
    }

    private IEnumerator AnimateShopOpen()
    {
        isAnimating = true;

        if (shopPanel == null || shopRectTransform == null)
        {
            isAnimating = false;
            yield break;
        }

        shopRectTransform.localScale = new Vector3(0.8f, 0.8f, 1);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (shopCanvasGroup != null)
                shopCanvasGroup.alpha = progress;

            if (shopRectTransform != null)
                shopRectTransform.localScale = Vector3.Lerp(new Vector3(0.8f, 0.8f, 1), Vector3.one, progress);

            yield return null;
        }

        if (shopCanvasGroup != null)
            shopCanvasGroup.alpha = 1;

        if (shopRectTransform != null)
            shopRectTransform.localScale = Vector3.one;

        isAnimating = false;
    }

    private IEnumerator AnimateShopClose()
    {
        isAnimating = true;

        if (shopPanel == null || shopRectTransform == null)
        {
            isAnimating = false;
            yield break;
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (shopCanvasGroup != null)
                shopCanvasGroup.alpha = 1 - progress;

            if (shopRectTransform != null)
                shopRectTransform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.8f, 0.8f, 1), progress);

            yield return null;
        }

        if (shopCanvasGroup != null)
            shopCanvasGroup.alpha = 0;

        if (shopPanel != null)
            shopPanel.SetActive(false);

        isAnimating = false;
    }

    private IEnumerator ShakePanel()
    {
        if (shopPanel == null) yield break;

        RectTransform rect = shopPanel.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 originalPos = rect.localPosition;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float shake = Random.Range(-10f, 10f);
            rect.localPosition = originalPos + new Vector3(shake, 0, 0);
            yield return null;
        }

        rect.localPosition = originalPos;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, 0.7f);
    }

    public void UpdateCoinDisplay()
    {
        playerCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        if (coinsDisplay != null)
            coinsDisplay.text = "Монеты: " + playerCoins.ToString();
    }
}