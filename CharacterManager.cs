using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [SerializeField] private CharacterData knightData;
    [SerializeField] private CharacterData archerData;

    private CharacterType currentType = CharacterType.Knight;

    [SerializeField] private KnightAnimator menuKnightAnimator;  // ← ДОБАВЬ

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (menuKnightAnimator == null)
            menuKnightAnimator = FindObjectOfType<KnightAnimator>();

        LoadData();  // ← ПЕРЕМЕСТИЛ СЮДА!

        // ← ЗАГРУЗИ СПРАЙТ СРАЗУ В AWAKE!
        if (menuKnightAnimator != null && currentType == CharacterType.Knight && knightData != null && knightData.idleSprites != null && knightData.idleSprites.Length > 0)
        {
            menuKnightAnimator.SetIdleSprites(knightData.idleSprites);
        }
        else if (menuKnightAnimator != null && currentType == CharacterType.Archer && archerData != null && archerData.idleSprites != null && archerData.idleSprites.Length > 0)
        {
            menuKnightAnimator.SetIdleSprites(archerData.idleSprites);
        }
    }

    private void Start()
    {
        LoadData();

        // ← ДОБАВЬ ЭТО!
        if (menuKnightAnimator != null && knightData != null && knightData.idleSprites != null && knightData.idleSprites.Length > 0)
        {
            menuKnightAnimator.SetIdleSprites(knightData.idleSprites);
            menuKnightAnimator.characterImage.sprite = knightData.idleSprites[0];  // ← ПЕРВЫЙ КАДР!
        }
    }

    private void LoadData()
    {
        string saved = PlayerPrefs.GetString("CurrentCharacter", "Knight");
        currentType = saved == "Archer" ? CharacterType.Archer : CharacterType.Knight;

        if (knightData != null)
        {
            knightData.isUnlocked = true;
            knightData.price = 0;
            if (string.IsNullOrEmpty(knightData.characterName))
                knightData.characterName = "Рыцарь";
            if (knightData.hpMultiplier <= 0f) knightData.hpMultiplier = 1.0f;
            if (knightData.dodgeChance <= 0f) knightData.dodgeChance = 0.1f;
        }

        if (archerData != null)
        {
            archerData.isUnlocked = PlayerPrefs.GetInt("ArcherUnlocked", 0) == 1;
            archerData.price = 100;
            if (string.IsNullOrEmpty(archerData.characterName))
                archerData.characterName = "Лучник";
            if (archerData.hpMultiplier <= 0f) archerData.hpMultiplier = 0.8f;
            if (archerData.dodgeChance <= 0f) archerData.dodgeChance = 0.2f;
        }
    }

    public CharacterData GetCurrentCharacter()
    {
        CharacterData data = currentType == CharacterType.Archer ? archerData : knightData;
        return data;
    }

    public CharacterData GetCharacterData(CharacterType type)
    {
        return type == CharacterType.Archer ? archerData : knightData;
    }

    public CharacterType GetCurrentCharacterType()
    {
        return currentType;
    }

    public void SelectCharacter(CharacterType type)
    {
        if (!IsCharacterUnlocked(type))
        {
            Debug.LogWarning("Попытка выбрать заблокированного персонажа: " + type);
            return;
        }

        currentType = type;
        PlayerPrefs.SetString("CurrentCharacter", type.ToString());
        PlayerPrefs.Save();

        // ОБНОВИ АНИМАЦИЮ В МЕНЮ!
        CharacterData data = GetCurrentCharacter();
        if (menuKnightAnimator != null && data != null && data.idleSprites != null)
            menuKnightAnimator.SetIdleSprites(data.idleSprites);

        Debug.Log("✅ Выбран персонаж: " + type);
    }

    /// <summary>
    /// Attempts to purchase the character, deducting coins from PlayerPrefs.
    /// Returns true if successful.
    /// </summary>
    public bool BuyCharacter(CharacterType type)
    {
        CharacterData data = GetCharacterData(type);
        if (data == null || data.isUnlocked)
        {
            Debug.Log("Персонаж уже разблокирован: " + type);
            return false;
        }

        int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        if (currentCoins < data.price)
        {
            Debug.Log("Недостаточно монет для покупки: " + type + " (нужно " + data.price + ")");
            return false;
        }

        currentCoins -= data.price;
        data.isUnlocked = true;

        if (type == CharacterType.Archer)
            PlayerPrefs.SetInt("ArcherUnlocked", 1);

        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
        Debug.Log("💰 Куплен персонаж: " + type + ". Осталось монет: " + currentCoins);
        return true;
    }

    public bool IsCharacterUnlocked(CharacterType type)
    {
        CharacterData data = GetCharacterData(type);
        return data != null && data.isUnlocked;
    }

    /// <summary>
    /// Reload data from PlayerPrefs (call after returning to menu).
    /// </summary>
    public void RefreshData()
    {
        LoadData();
    }
}