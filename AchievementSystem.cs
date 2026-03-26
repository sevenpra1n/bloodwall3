using UnityEngine;

public class AchievementSystem : MonoBehaviour
{
    // Achievement 1: Deal Damage
    public static readonly int[] DamageTargets = { 500, 3500, 8500, 15000, 35000 };
    public static readonly int[] DamageRewards = { 30, 75, 250, 500, 1250 }; // coins

    // Achievement 2: Spend Coins
    public static readonly int[] SpendTargets = { 300, 2000, 10000, 25000, 120000 };
    // Level 5 (index 4) reward: exclusive sword added to inventory

    // Achievement 3: Collect Rarity Items (Common → Mythical)
    public static readonly int[] RarityRewards = { 0, 50, 200, 500, 2000 }; // coins per level

    // Achievement 4: Dodge / enemy miss
    public static readonly int[] MissTargets = { 10, 40, 120, 250, 500 };
    public static readonly int[] MissRewards = { 50, 80, 220, 450, 1000 };

    // PlayerPrefs keys
    private const string TotalDamageKey = "Achiev_TotalDamage";
    private const string TotalSpentKey = "Achiev_TotalSpent";
    private const string DamageLevelKey = "Achiev_DamageLevel";
    private const string SpendLevelKey = "Achiev_SpendLevel";
    private const string RarityProgressKey = "Achiev_RarityProgress";
    private const string RarityLevelKey = "Achiev_RarityLevel";
    private const string TotalMissesKey = "Achiev_TotalMisses";
    private const string MissLevelKey = "Achiev_MissLevel";

    private float totalDamageDealt;
    private int totalCoinsSpent;
    private int damageAchievLevel; // number of levels already claimed (0-5)
    private int spendAchievLevel;  // number of levels already claimed (0-5)

    // rarityProgress: highest rarity index obtained (0=none, 1=Common, 2=Rare, 3=Epic, 4=Legendary, 5=Mythical)
    private int rarityProgress;
    private int rarityAchievLevel; // number of rarity levels already claimed (0-5)

    private int totalMisses;
    private int missAchievLevel;   // number of miss levels already claimed (0-5)

    private static AchievementSystem instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void LoadData()
    {
        totalDamageDealt = PlayerPrefs.GetFloat(TotalDamageKey, 0f);
        totalCoinsSpent = PlayerPrefs.GetInt(TotalSpentKey, 0);
        damageAchievLevel = PlayerPrefs.GetInt(DamageLevelKey, 0);
        spendAchievLevel = PlayerPrefs.GetInt(SpendLevelKey, 0);
        rarityProgress = PlayerPrefs.GetInt(RarityProgressKey, 0);
        rarityAchievLevel = PlayerPrefs.GetInt(RarityLevelKey, 0);
        totalMisses = PlayerPrefs.GetInt(TotalMissesKey, 0);
        missAchievLevel = PlayerPrefs.GetInt(MissLevelKey, 0);
    }

    public void SaveData()
    {
        PlayerPrefs.SetFloat(TotalDamageKey, totalDamageDealt);
        PlayerPrefs.SetInt(TotalSpentKey, totalCoinsSpent);
        PlayerPrefs.SetInt(DamageLevelKey, damageAchievLevel);
        PlayerPrefs.SetInt(SpendLevelKey, spendAchievLevel);
        PlayerPrefs.SetInt(RarityProgressKey, rarityProgress);
        PlayerPrefs.SetInt(RarityLevelKey, rarityAchievLevel);
        PlayerPrefs.SetInt(TotalMissesKey, totalMisses);
        PlayerPrefs.SetInt(MissLevelKey, missAchievLevel);
        PlayerPrefs.Save();
    }

    public static void ResetAchievements()
    {
        PlayerPrefs.DeleteKey(TotalDamageKey);
        PlayerPrefs.DeleteKey(TotalSpentKey);
        PlayerPrefs.DeleteKey(DamageLevelKey);
        PlayerPrefs.DeleteKey(SpendLevelKey);
        PlayerPrefs.DeleteKey(RarityProgressKey);
        PlayerPrefs.DeleteKey(RarityLevelKey);
        PlayerPrefs.DeleteKey(TotalMissesKey);
        PlayerPrefs.DeleteKey(MissLevelKey);
        PlayerPrefs.DeleteKey("HasExclusiveSword");
        PlayerPrefs.Save();
        Debug.Log("🏆 Достижения сброшены!");
    }

    // ─── Damage achievement ───────────────────────────────────────────────────

    public void AddDamage(float damage)
    {
        if (damage <= 0f) return;
        totalDamageDealt += damage;
        // Update in-memory PlayerPrefs without flushing to disk on every hit
        PlayerPrefs.SetFloat(TotalDamageKey, totalDamageDealt);
    }

    public float GetTotalDamage() => totalDamageDealt;
    public int GetDamageLevel() => damageAchievLevel;

    public int GetDamageTarget(int level)
    {
        return (level >= 0 && level < DamageTargets.Length) ? DamageTargets[level] : 0;
    }

    public int GetDamageReward(int level)
    {
        return (level >= 0 && level < DamageRewards.Length) ? DamageRewards[level] : 0;
    }

    public bool IsDamageClaimable()
    {
        if (damageAchievLevel >= DamageTargets.Length) return false;
        return totalDamageDealt >= DamageTargets[damageAchievLevel];
    }

    public void ClaimDamageReward()
    {
        if (!IsDamageClaimable()) return;
        int reward = DamageRewards[damageAchievLevel];
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        PlayerPrefs.SetInt("PlayerCoins", coins + reward);
        damageAchievLevel++;
        SaveData();
        Debug.Log("🏆 Получена награда за урон: +" + reward + " монет");
    }

    // ─── Spend-coins achievement ──────────────────────────────────────────────

    public void AddSpentCoins(int amount)
    {
        if (amount <= 0) return;
        totalCoinsSpent += amount;
        // Update in-memory PlayerPrefs without flushing to disk on every purchase
        PlayerPrefs.SetInt(TotalSpentKey, totalCoinsSpent);
    }

    public int GetTotalSpent() => totalCoinsSpent;
    public int GetSpendLevel() => spendAchievLevel;

    public int GetSpendTarget(int level)
    {
        return (level >= 0 && level < SpendTargets.Length) ? SpendTargets[level] : 0;
    }

    public bool IsSpendClaimable()
    {
        if (spendAchievLevel >= SpendTargets.Length) return false;
        return totalCoinsSpent >= SpendTargets[spendAchievLevel];
    }

    public void ClaimSpendReward()
    {
        if (!IsSpendClaimable()) return;
        if (spendAchievLevel == 4)
        {
            // Level 5: exclusive sword
            PlayerPrefs.SetInt("HasExclusiveSword", 1);
            PlayerPrefs.Save();
            Debug.Log("🏆 Получен эксклюзивный меч!");
        }
        spendAchievLevel++;
        SaveData();
        Debug.Log("🏆 Получена награда за траты (уровень " + spendAchievLevel + ")");
    }

    // ─── Rarity Items achievement ─────────────────────────────────────────────

    public void AddRarityItem(Rarity rarity)
    {
        // Convert rarity enum to 1-based index (Common=1 .. Mythical=5)
        int rarityIndex = (int)rarity + 1;
        if (rarityIndex > rarityProgress)
        {
            rarityProgress = rarityIndex;
            PlayerPrefs.SetInt(RarityProgressKey, rarityProgress);
            Debug.Log("🏆 Новая редкость получена: " + rarity + " (прогресс: " + rarityProgress + ")");
        }
    }

    public int GetRarityProgress() => rarityProgress;
    public int GetRarityLevel() => rarityAchievLevel;

    public int GetRarityReward(int level)
    {
        return (level >= 0 && level < RarityRewards.Length) ? RarityRewards[level] : 0;
    }

    public bool IsRarityClaimable()
    {
        if (rarityAchievLevel >= RarityRewards.Length) return false;
        return rarityProgress > rarityAchievLevel;
    }

    public void ClaimRarityReward()
    {
        if (!IsRarityClaimable()) return;
        int reward = RarityRewards[rarityAchievLevel];
        if (reward > 0)
        {
            int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
            PlayerPrefs.SetInt("PlayerCoins", coins + reward);
        }
        rarityAchievLevel++;
        SaveData();
        Debug.Log("🏆 Получена награда за редкость: +" + reward + " монет (уровень " + rarityAchievLevel + ")");
    }

    // ─── Dodge / Miss achievement ─────────────────────────────────────────────

    public void AddMiss()
    {
        totalMisses++;
        PlayerPrefs.SetInt(TotalMissesKey, totalMisses);
        Debug.Log("🏆 Уворот! Всего уворотов: " + totalMisses);
    }

    public int GetTotalMisses() => totalMisses;
    public int GetMissLevel() => missAchievLevel;

    public int GetMissTarget(int level)
    {
        return (level >= 0 && level < MissTargets.Length) ? MissTargets[level] : 0;
    }

    public int GetMissReward(int level)
    {
        return (level >= 0 && level < MissRewards.Length) ? MissRewards[level] : 0;
    }

    public bool IsMissClaimable()
    {
        if (missAchievLevel >= MissTargets.Length) return false;
        return totalMisses >= MissTargets[missAchievLevel];
    }

    public void ClaimMissReward()
    {
        if (!IsMissClaimable()) return;
        int reward = MissRewards[missAchievLevel];
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        PlayerPrefs.SetInt("PlayerCoins", coins + reward);
        missAchievLevel++;
        SaveData();
        Debug.Log("🏆 Получена награда за уворот: +" + reward + " монет (уровень " + missAchievLevel + ")");
    }
}