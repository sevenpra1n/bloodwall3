using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Equipment equipment;
    [SerializeField] private Image equipmentIcon;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text tooltipName;
    [SerializeField] private Text tooltipDescription;
    [SerializeField] private Text tooltipPower;
    [SerializeField] private Button equipButton;

    private EquipmentUIManager uiManager;
    private TooltipAnimator tooltipAnimator;

    public void Initialize(Equipment eq, EquipmentUIManager manager)
    {
        equipment = eq;
        uiManager = manager;

        if (equipment == null)
        {
            Debug.LogError("Equipment is NULL!");
            return;
        }

        if (equipButton != null)
        {
            equipButton.onClick.AddListener(EquipThis);
        }
        else
        {
            Debug.LogWarning("equipButton not assigned!");
        }

        // Получаем компонент TooltipAnimator
        if (tooltipPanel != null)
        {
            tooltipAnimator = tooltipPanel.GetComponent<TooltipAnimator>();
            if (tooltipAnimator == null)
            {
                tooltipAnimator = tooltipPanel.AddComponent<TooltipAnimator>();
            }
        }
    }

    public void SetEquipmentIcon(Sprite icon)
    {
        if (equipmentIcon == null)
        {
            Debug.LogError("equipmentIcon Image component is NULL! Check EquipmentButtonPrefab structure!");
            return;
        }

        if (icon == null)
        {
            Debug.LogError("Icon sprite is NULL!");
            equipmentIcon.color = Color.red;
            return;
        }

        equipmentIcon.sprite = icon;
        equipmentIcon.color = Color.white;

        Debug.Log("Icon set successfully: " + icon.name);
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

            if (tooltipPower != null)
                tooltipPower.text = "Мощь: +" + equipment.power;

            // Запускаем анимацию с цветом редкости
            if (tooltipAnimator != null)
            {
                tooltipAnimator.ShowTooltip(equipment.rarity);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            if (tooltipAnimator != null)
            {
                tooltipAnimator.HideTooltip();
            }
            else
            {
                tooltipPanel.SetActive(false);
            }
        }
    }

    private void EquipThis()
    {
        if (uiManager != null)
        {
            uiManager.EquipItemDirect(equipment);
        }
    }
}