using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text tooltipName;
    [SerializeField] private Text tooltipDescription;
    [SerializeField] private Text tooltipPrice;
    [SerializeField] private Text tooltipPower;

    private Equipment equipment;
    private ShopManager shopManager;
    private TooltipAnimator tooltipAnimator;
    private Button button;

    public void Initialize(Equipment eq, ShopManager manager)
    {
        equipment = eq;
        shopManager = manager;

        // Показываем иконку
        if (itemIcon != null && equipment.icon != null)
        {
            itemIcon.sprite = equipment.icon;
        }

        // Кнопка для клика
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnItemClick());
        }

        // Tooltip анимация
        if (tooltipPanel != null)
        {
            tooltipAnimator = tooltipPanel.GetComponent<TooltipAnimator>();
            if (tooltipAnimator == null)
                tooltipAnimator = tooltipPanel.AddComponent<TooltipAnimator>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);

            if (tooltipName != null)
                tooltipName.text = equipment.equipmentName;

            if (tooltipDescription != null)
                tooltipDescription.text = equipment.description;

            if (tooltipPrice != null)
                tooltipPrice.text = equipment.price.ToString() + " монет";

            if (tooltipPower != null)
                tooltipPower.text = "Мощь: +" + equipment.power;

            if (tooltipAnimator != null)
                tooltipAnimator.ShowTooltip(equipment.rarity);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            if (tooltipAnimator != null)
                tooltipAnimator.HideTooltip();
            else
                tooltipPanel.SetActive(false);
        }
    }

    private void OnItemClick()
    {
        if (shopManager != null)
        {
            // ПЕРЕДАЁМ itemButton чтобы удалить его после покупки
            shopManager.BuyItem(equipment, this);
        }
    }
}