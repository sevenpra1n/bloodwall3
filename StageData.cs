using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public float power = 150f;
    public Sprite idleSprite;
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public AudioClip attackSound;

    // ← НОВЫЕ ПОЛЯ ДЛЯ НАСТРОЙКИ ВРАГОВ
    [Range(0f, 100f)]
    public float missChance = 20f;  // Шанс промаха врага (%)

    [Range(0f, 100f)]
    public float critChance = 20f;  // Шанс крита врага (%)
}

[System.Serializable]
public class StageData
{
    public int stageNumber = 1;
    public EnemyData enemyData;
    public int coinsReward = 30;
    public int expReward = 50;
    public bool isBossStage = false;
}