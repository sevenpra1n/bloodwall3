using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int stageIndex = 0;
    [SerializeField] private StageData stageData;

    // ← ВРУЧНУЮ ВВОДИМОЕ ИМЯ
    [SerializeField] private string customEnemyName = "Враг";

    [SerializeField] private Sprite customEnemyIcon;

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text tooltipEnemyName;
    [SerializeField] private Text tooltipPower;
    [SerializeField] private Text tooltipHP;
    [SerializeField] private Text tooltipCoinsReward;
    [SerializeField] private Text tooltipExpReward;
    [SerializeField] private Image tooltipEnemyImage;
    [SerializeField] private Image buttonEnemyIcon;

    private Button button;
    internal Button Button => button;
    private Image stageButtonImage;
    private CanvasGroup tooltipCanvasGroup;

    private static readonly System.Collections.Generic.List<StageButton> allStageButtons =
        new System.Collections.Generic.List<StageButton>();

    private void Awake()
    {
        allStageButtons.Add(this);
    }

    private void OnDestroy()
    {
        allStageButtons.Remove(this);
    }

    private void Start()
    {
        Debug.Log("🎮 StageButton " + (stageIndex + 1) + " START");

        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        button.onClick.AddListener(StartBattle);
        button.interactable = true;

        stageButtonImage = GetComponent<Image>();
        if (stageButtonImage != null)
            stageButtonImage.raycastTarget = true;

        if (buttonEnemyIcon != null && customEnemyIcon != null)
        {
            buttonEnemyIcon.sprite = customEnemyIcon;
            Debug.Log("✅ Иконка кнопки установлена");
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            tooltipCanvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (tooltipCanvasGroup == null)
                tooltipCanvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
            tooltipCanvasGroup.alpha = 0;
        }

        Debug.Log("✅ StageButton " + (stageIndex + 1) + " готов");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("💬 Мышка на кнопке " + (stageIndex + 1));
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("👋 Мышка уходит");
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipPanel == null || stageData == null)
            return;

        tooltipPanel.SetActive(true);

        // ✅ ИМЯ ВРАГА ИЗ ПОЛЯ customEnemyName
        if (tooltipEnemyName != null)
            tooltipEnemyName.text = customEnemyName;

        // ← ТОЛЬКО ЦИФРЫ
        if (tooltipPower != null)
            tooltipPower.text = stageData.enemyData.power.ToString();

        if (tooltipHP != null)
            tooltipHP.text = (stageData.enemyData.power * 1).ToString("F0");

        if (tooltipCoinsReward != null)
            tooltipCoinsReward.text = stageData.coinsReward.ToString();

        if (tooltipExpReward != null)
            tooltipExpReward.text = stageData.expReward.ToString();

        if (tooltipEnemyImage != null && stageData.enemyData.idleSprite != null)
            tooltipEnemyImage.sprite = stageData.enemyData.idleSprite;

        StartCoroutine(AnimateTooltip(true));
    }

    private void HideTooltip()
    {
        StartCoroutine(AnimateTooltip(false));
    }

    private IEnumerator AnimateTooltip(bool show)
    {
        if (tooltipCanvasGroup == null)
            yield break;

        float duration = 0.2f;
        float elapsed = 0f;
        float startAlpha = tooltipCanvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            tooltipCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            yield return null;
        }

        tooltipCanvasGroup.alpha = endAlpha;

        if (!show)
            tooltipPanel.SetActive(false);
    }

    private void StartBattle()
    {
        Debug.Log("🎮 ЗАПУСК ЭТАЖА " + (stageIndex + 1));

        // Disable all StageButtons to prevent double-click
        foreach (StageButton sb in allStageButtons)
        {
            if (sb.Button != null)
                sb.Button.interactable = false;
        }

        PlayerPrefs.SetInt("CurrentStage", stageIndex);
        PlayerPrefs.Save();

        // ← ЗАПУСКАЕМ ПОЛНЫЙ ПЕРЕХОД С FADE И КАРТИНКОЙ
        EquipmentUIManager equipManager = FindObjectOfType<EquipmentUIManager>();
        if (equipManager != null)
        {
            equipManager.StartBattleTransition();
        }
        else
        {
            // Если не найдена EquipmentUIManager, просто загружаем сцену
            SceneManager.LoadScene("BattleScene");
        }
    }
}