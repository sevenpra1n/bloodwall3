using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Purchase confirmation dialog.
/// Wire up in Unity Inspector:
///   - characterNameText  – shows the character's name
///   - priceText          – shows cost ("5000 монет")
///   - buyButton          – triggers purchase
///   - cancelButton       – closes panel
/// </summary>
public class PurchaseConfirmPanel : MonoBehaviour
{
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    private CharacterType pendingType;
    private CharacterSelectionPanel selectionPanel;
    private EquipmentUIManager equipmentUIManager;

    private void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void Show(CharacterType type, CharacterSelectionPanel parent, EquipmentUIManager uiManager)
    {
        pendingType = type;
        selectionPanel = parent;
        equipmentUIManager = uiManager;

        CharacterData data = CharacterManager.Instance != null
            ? CharacterManager.Instance.GetCharacterData(type)
            : null;

        if (characterNameText != null)
            characterNameText.text = data != null ? data.characterName : type.ToString();

        if (priceText != null)
        {
            int price = data != null ? data.price : 5000;
            priceText.text = price + " монет";
        }
    }

    private void OnBuyClicked()
    {
        if (CharacterManager.Instance == null) return;

        bool success = CharacterManager.Instance.BuyCharacter(pendingType);
        if (success)
        {
            // Refresh the coin display in the equipment menu
            if (equipmentUIManager != null)
                equipmentUIManager.UpdateCoinsUI();

            // Notify the selection panel to refresh its icons
            if (selectionPanel != null)
                selectionPanel.OnPurchaseComplete();

            Debug.Log("✅ Покупка успешна: " + pendingType);
        }
        else
        {
            Debug.Log("❌ Покупка не удалась: недостаточно монет или уже куплено.");
        }

        gameObject.SetActive(false);
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);

        // ОТКРОЙ ПЕРВУЮ ПАНЕЛЬ НАЗАД!
        CharacterSelectionPanel selectionPanel = FindObjectOfType<CharacterSelectionPanel>();
        if (selectionPanel != null)
            selectionPanel.gameObject.SetActive(true);
    }
}