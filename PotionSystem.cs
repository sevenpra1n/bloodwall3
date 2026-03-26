using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PotionSystem : MonoBehaviour
{
    [SerializeField] private int maxHealthPotions = 3;
    [SerializeField] private int maxManaPotions = 3;

    private int currentHealthPotions;
    private int currentManaPotions;

    [SerializeField] private Button healthPotionButton;
    [SerializeField] private Button manaPotionButton;

    [SerializeField] private Sprite[] healthPotionSprites;
    [SerializeField] private Sprite[] manaPotionSprites;

    [SerializeField] private Sprite[] healthPotionHoverSprites;
    [SerializeField] private Sprite[] manaPotionHoverSprites;

    [SerializeField] private Sprite[] healthPotionPressedSprites;
    [SerializeField] private Sprite[] manaPotionPressedSprites;

    [SerializeField] private PlayerController player;

    [SerializeField] private Image[] playerHPBars = new Image[9];
    [SerializeField] private Image[] playerManaBars = new Image[9];

    [SerializeField] private AudioClip potionSound;

    private bool healthIsPressed = false;
    private bool manaIsPressed = false;
    private bool healthIsHovered = false;
    private bool manaIsHovered = false;

    private bool isAnimatingHP = false;
    private bool isAnimatingMana = false;

    private AudioSource audioSource;

    private void Start()
    {
        currentHealthPotions = maxHealthPotions;
        currentManaPotions = maxManaPotions;

        EventTrigger healthTrigger = healthPotionButton.gameObject.GetComponent<EventTrigger>();
        if (healthTrigger == null)
            healthTrigger = healthPotionButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger manaTrigger = manaPotionButton.gameObject.GetComponent<EventTrigger>();
        if (manaTrigger == null)
            manaTrigger = manaPotionButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry healthEnter = new EventTrigger.Entry();
        healthEnter.eventID = EventTriggerType.PointerEnter;
        healthEnter.callback.AddListener((data) => OnHealthEnter());
        healthTrigger.triggers.Add(healthEnter);

        EventTrigger.Entry healthExit = new EventTrigger.Entry();
        healthExit.eventID = EventTriggerType.PointerExit;
        healthExit.callback.AddListener((data) => OnHealthExit());
        healthTrigger.triggers.Add(healthExit);

        EventTrigger.Entry healthDown = new EventTrigger.Entry();
        healthDown.eventID = EventTriggerType.PointerDown;
        healthDown.callback.AddListener((data) => OnHealthDown());
        healthTrigger.triggers.Add(healthDown);

        EventTrigger.Entry healthUp = new EventTrigger.Entry();
        healthUp.eventID = EventTriggerType.PointerUp;
        healthUp.callback.AddListener((data) => OnHealthUp());
        healthTrigger.triggers.Add(healthUp);

        EventTrigger.Entry manaEnter = new EventTrigger.Entry();
        manaEnter.eventID = EventTriggerType.PointerEnter;
        manaEnter.callback.AddListener((data) => OnManaEnter());
        manaTrigger.triggers.Add(manaEnter);

        EventTrigger.Entry manaExit = new EventTrigger.Entry();
        manaExit.eventID = EventTriggerType.PointerExit;
        manaExit.callback.AddListener((data) => OnManaExit());
        manaTrigger.triggers.Add(manaExit);

        EventTrigger.Entry manaDown = new EventTrigger.Entry();
        manaDown.eventID = EventTriggerType.PointerDown;
        manaDown.callback.AddListener((data) => OnManaDown());
        manaTrigger.triggers.Add(manaDown);

        EventTrigger.Entry manaUp = new EventTrigger.Entry();
        manaUp.eventID = EventTriggerType.PointerUp;
        manaUp.callback.AddListener((data) => OnManaUp());
        manaTrigger.triggers.Add(manaUp);

        healthPotionButton.onClick.AddListener(UseHealthPotion);
        manaPotionButton.onClick.AddListener(UseManaPotion);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        UpdatePotionUI();
    }

    private void OnHealthEnter()
    {
        healthIsHovered = true;
        UpdateHealthImage();
    }

    private void OnHealthExit()
    {
        healthIsHovered = false;
        healthIsPressed = false;
        UpdateHealthImage();
    }

    private void OnHealthDown()
    {
        healthIsPressed = true;
        UpdateHealthImage();
        StartCoroutine(ShakeButton(healthPotionButton.GetComponent<RectTransform>()));
    }

    private void OnHealthUp()
    {
        healthIsPressed = false;
        UpdateHealthImage();
    }

    private void UpdateHealthImage()
    {
        Image healthImage = healthPotionButton.GetComponent<Image>();
        if (healthImage == null || currentHealthPotions < 0)
            return;

        if (healthIsPressed && currentHealthPotions > 0)
        {
            healthImage.sprite = healthPotionPressedSprites[currentHealthPotions];
        }
        else if (healthIsHovered && currentHealthPotions > 0)
        {
            healthImage.sprite = healthPotionHoverSprites[currentHealthPotions];
        }
        else
        {
            healthImage.sprite = healthPotionSprites[currentHealthPotions];
        }
    }

    private void OnManaEnter()
    {
        manaIsHovered = true;
        UpdateManaImage();
    }

    private void OnManaExit()
    {
        manaIsHovered = false;
        manaIsPressed = false;
        UpdateManaImage();
    }

    private void OnManaDown()
    {
        manaIsPressed = true;
        UpdateManaImage();
        StartCoroutine(ShakeButton(manaPotionButton.GetComponent<RectTransform>()));
    }

    private void OnManaUp()
    {
        manaIsPressed = false;
        UpdateManaImage();
    }

    private void UpdateManaImage()
    {
        Image manaImage = manaPotionButton.GetComponent<Image>();
        if (manaImage == null || currentManaPotions < 0)
            return;

        if (manaIsPressed && currentManaPotions > 0)
        {
            manaImage.sprite = manaPotionPressedSprites[currentManaPotions];
        }
        else if (manaIsHovered && currentManaPotions > 0)
        {
            manaImage.sprite = manaPotionHoverSprites[currentManaPotions];
        }
        else
        {
            manaImage.sprite = manaPotionSprites[currentManaPotions];
        }
    }

    public void UseHealthPotion()
    {
        if (currentHealthPotions <= 0)
            return;

        if (isAnimatingHP)
            return;

        PlayPotionSound();

        float healAmount = player.GetPower() * 0.3f;
        player.Heal(healAmount);

        StartCoroutine(AnimateHPBars(Color.green));

        currentHealthPotions--;
        UpdatePotionUI();
    }

    public void UseManaPotion()
    {
        if (currentManaPotions <= 0)
            return;

        if (isAnimatingMana)
            return;

        PlayPotionSound();

        player.RestoreMana(50);

        StartCoroutine(AnimateManaBars(new Color(0.8f, 0.2f, 1f)));

        currentManaPotions--;
        UpdatePotionUI();
    }

    private void PlayPotionSound()
    {
        if (potionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(potionSound, 0.7f);
        }
    }

    private IEnumerator ShakeButton(RectTransform buttonRect)
    {
        if (buttonRect == null)
            yield break;

        Vector3 originalPos = buttonRect.localPosition;
        float duration = 0.3f;
        float elapsed = 0f;
        float shakeAmount = 5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float shake = (1 - progress) * shakeAmount;
            buttonRect.localPosition = originalPos + new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0
            );

            yield return null;
        }

        buttonRect.localPosition = originalPos;
    }

    private IEnumerator AnimateHPBars(Color glowColor)
    {
        isAnimatingHP = true;

        if (playerHPBars == null || playerHPBars.Length == 0)
        {
            isAnimatingHP = false;
            yield break;
        }

        Vector3[] originalScales = new Vector3[playerHPBars.Length];
        Color[] originalColors = new Color[playerHPBars.Length];
        RectTransform[] rects = new RectTransform[playerHPBars.Length];

        for (int i = 0; i < playerHPBars.Length; i++)
        {
            if (playerHPBars[i] != null)
            {
                rects[i] = playerHPBars[i].GetComponent<RectTransform>();
                if (rects[i] != null)
                {
                    originalScales[i] = rects[i].localScale;
                    originalColors[i] = playerHPBars[i].color;
                }
            }
        }

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float scaleProgress = Mathf.Sin(progress * Mathf.PI) * 0.3f + 1f;
            float colorProgress = Mathf.Sin(progress * Mathf.PI);

            for (int i = 0; i < playerHPBars.Length; i++)
            {
                if (playerHPBars[i] != null && rects[i] != null)
                {
                    rects[i].localScale = originalScales[i] * scaleProgress;
                    playerHPBars[i].color = Color.Lerp(originalColors[i], glowColor, colorProgress);
                }
            }

            yield return null;
        }

        for (int i = 0; i < playerHPBars.Length; i++)
        {
            if (playerHPBars[i] != null && rects[i] != null)
            {
                rects[i].localScale = originalScales[i];
                playerHPBars[i].color = originalColors[i];
            }
        }

        isAnimatingHP = false;
    }

    private IEnumerator AnimateManaBars(Color glowColor)
    {
        isAnimatingMana = true;

        if (playerManaBars == null || playerManaBars.Length == 0)
        {
            isAnimatingMana = false;
            yield break;
        }

        Vector3[] originalScales = new Vector3[playerManaBars.Length];
        Color[] originalColors = new Color[playerManaBars.Length];
        RectTransform[] rects = new RectTransform[playerManaBars.Length];

        for (int i = 0; i < playerManaBars.Length; i++)
        {
            if (playerManaBars[i] != null)
            {
                rects[i] = playerManaBars[i].GetComponent<RectTransform>();
                if (rects[i] != null)
                {
                    originalScales[i] = rects[i].localScale;
                    originalColors[i] = playerManaBars[i].color;
                }
            }
        }

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float scaleProgress = Mathf.Sin(progress * Mathf.PI) * 0.3f + 1f;
            float colorProgress = Mathf.Sin(progress * Mathf.PI);

            for (int i = 0; i < playerManaBars.Length; i++)
            {
                if (playerManaBars[i] != null && rects[i] != null)
                {
                    rects[i].localScale = originalScales[i] * scaleProgress;
                    playerManaBars[i].color = Color.Lerp(originalColors[i], glowColor, colorProgress);
                }
            }

            yield return null;
        }

        for (int i = 0; i < playerManaBars.Length; i++)
        {
            if (playerManaBars[i] != null && rects[i] != null)
            {
                rects[i].localScale = originalScales[i];
                playerManaBars[i].color = originalColors[i];
            }
        }

        isAnimatingMana = false;
    }

    private void UpdatePotionUI()
    {
        if (healthPotionButton != null && healthPotionSprites.Length > currentHealthPotions)
        {
            Image healthImage = healthPotionButton.GetComponent<Image>();
            if (healthImage != null)
            {
                healthImage.sprite = healthPotionSprites[currentHealthPotions];
            }
        }

        if (manaPotionButton != null && manaPotionSprites.Length > currentManaPotions)
        {
            Image manaImage = manaPotionButton.GetComponent<Image>();
            if (manaImage != null)
            {
                manaImage.sprite = manaPotionSprites[currentManaPotions];
            }
        }

        healthPotionButton.interactable = currentHealthPotions > 0;
        manaPotionButton.interactable = currentManaPotions > 0;
    }

    public void DisableButtons()
    {
        healthPotionButton.interactable = false;
        manaPotionButton.interactable = false;

        Image healthImage = healthPotionButton.GetComponent<Image>();
        if (healthImage != null)
            healthImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        Image manaImage = manaPotionButton.GetComponent<Image>();
        if (manaImage != null)
            manaImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
    }

    public void EnableButtons()
    {
        healthPotionButton.interactable = currentHealthPotions > 0;
        manaPotionButton.interactable = currentManaPotions > 0;

        Image healthImage = healthPotionButton.GetComponent<Image>();
        if (healthImage != null)
            healthImage.color = Color.white;

        Image manaImage = manaPotionButton.GetComponent<Image>();
        if (manaImage != null)
            manaImage.color = Color.white;
    }

    public int GetHealthPotions()
    {
        return currentHealthPotions;
    }

    public int GetManaPotions()
    {
        return currentManaPotions;
    }
}