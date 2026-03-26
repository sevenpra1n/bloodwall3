using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Shows detailed information about a character with an animated preview.
/// Wire up in Unity Inspector:
///   - characterImage     – the Image used to animate the character preview
///   - characterNameText  – character name label
///   - descriptionText    – description label
///   - statsText          – HP multiplier / Dodge / Skill info
///   - selectButton       – selects the character and updates the menu
///   - returnButton       – goes back to CharacterSelectionPanel
///   - knightAnimator     – reference to the menu character animator (updated on Select)
///   - frameDelay         – seconds between idle animation frames
/// </summary>
public class CharacterDetailPanel : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text statsText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private KnightAnimator knightAnimator;
    [SerializeField] private float frameDelay = 0.15f;

    private CharacterType currentType;
    private CharacterSelectionPanel selectionPanel;
    private Coroutine idleCoroutine;

    private void Start()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectClicked);

        if (returnButton != null)
            returnButton.onClick.AddListener(OnReturnClicked);
    }

    private void OnDisable()
    {
        StopIdleAnimation();
    }

    public void Show(CharacterType type, CharacterSelectionPanel parent)
    {
        currentType = type;
        selectionPanel = parent;

        CharacterData data = CharacterManager.Instance != null
            ? CharacterManager.Instance.GetCharacterData(type)
            : null;

        if (data == null) return;

        // Update text fields
        if (characterNameText != null)
            characterNameText.text = data.characterName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (statsText != null)
        {
            string skillInfo = type == CharacterType.Archer
                ? "Скилл 1: Выстрел\n(урон: 1.5x / 2x / 3x)"
                : "Скилл 1: Тяжелый удар\n(урон: 0.3x)";

            statsText.text =
                "HP: x" + data.hpMultiplier.ToString("F1") + "\n" +
                "Уворот: " + (data.dodgeChance * 100f).ToString("F0") + "%\n" +
                skillInfo;
        }

        // Start animated preview
        StopIdleAnimation();

        if (characterImage != null && data.idleSprites != null && data.idleSprites.Length > 0)
        {
            characterImage.sprite = data.idleSprites[0];
            characterImage.gameObject.SetActive(true);
            idleCoroutine = StartCoroutine(PlayIdleAnimation(data.idleSprites));
        }

        // The Select button should be disabled if the character is already selected
        if (selectButton != null && CharacterManager.Instance != null)
        {
            bool alreadySelected = CharacterManager.Instance.GetCurrentCharacterType() == type;
            selectButton.interactable = !alreadySelected;
        }
    }

    private IEnumerator PlayIdleAnimation(Sprite[] sprites)
    {
        int frame = 0;
        while (true)
        {
            if (characterImage != null && sprites != null && sprites.Length > 0)
                characterImage.sprite = sprites[frame % sprites.Length];

            yield return new WaitForSeconds(frameDelay);
            frame++;
        }
    }

    private void StopIdleAnimation()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
    }

    private void OnSelectClicked()
    {
        if (CharacterManager.Instance == null) return;

        CharacterManager.Instance.SelectCharacter(currentType);

        // Update the menu character animator to show the selected character
        if (knightAnimator != null && CharacterManager.Instance != null)
        {
            CharacterData data = CharacterManager.Instance.GetCurrentCharacter();
            if (data != null && data.idleSprites != null && data.idleSprites.Length > 0)
                knightAnimator.SetIdleSprites(data.idleSprites);
        }

        // Disable the Select button since this character is now active
        if (selectButton != null)
            selectButton.interactable = false;

        Debug.Log("✅ Персонаж выбран: " + currentType);
    }

    private void OnReturnClicked()
    {
        StopIdleAnimation();
        gameObject.SetActive(false);

        // ОТКРОЙ ПЕРВУЮ ПАНЕЛЬ НАЗАД!
        CharacterSelectionPanel selectionPanel = FindObjectOfType<CharacterSelectionPanel>();
        if (selectionPanel != null)
            selectionPanel.gameObject.SetActive(true);
    }
}