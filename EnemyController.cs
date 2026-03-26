using UnityEngine;
using UnityEngine.UI;

public class EnemyController : Character
{
    [SerializeField] private float power = 150f;

    private int poisonDuration = 0;
    private float poisonDamage = 0f;

    private bool lastAttackWasCrit = false;
    private bool lastAttackWasMiss = false;

    // ← ИЗМЕНЕНО: 0 ПО УМОЛЧАНИЮ
    private float missChance = 0f;
    private float critChance = 0f;

    private void Start()
    {
        maxHP = power * 1f;
        currentHP = maxHP;
        UpdateHPUI();

        Debug.Log("Враг стартанул с мощью: " + power + " и HP: " + maxHP);
    }

    // ← УСТАНАВЛИВАЕТ ПАРАМЕТРЫ ИЗ BattleManager
    public void SetEnemyParameters(float newPower, float newMissChance, float newCritChance)
    {
        power = newPower;
        missChance = Mathf.Clamp01(newMissChance);
        critChance = Mathf.Clamp01(newCritChance);

        maxHP = power * 1f;
        currentHP = maxHP;
        UpdateHPUI();

        Debug.Log("⚙️ Параметры врага установлены:");
        Debug.Log("   💪 Мощь: " + power);
        Debug.Log("   💨 Шанс миса: " + (missChance * 100) + "%");
        Debug.Log("   ⚡ Шанс крита: " + (critChance * 100) + "%");
    }

    public void BasicAttack(Character player)
    {
        float damage = power * 0.15f;
        if (lastAttackWasCrit)
            damage *= 2;

        player.TakeDamage(damage);
    }

    public void ApplyPoison(int duration, float damage)
    {
        poisonDuration = duration;
        poisonDamage = damage;
    }

    public void UpdatePoison()
    {
        if (poisonDuration > 0)
        {
            TakeDamage(poisonDamage);
            poisonDuration--;
        }
    }

    // ← ПРОВЕРКА ПРОМАХА
    public bool CheckMiss()
    {
        lastAttackWasMiss = Random.value < missChance;
        Debug.Log("🎲 Враг проверяет мис (" + (missChance * 100) + "%): " + (lastAttackWasMiss ? "МИСС!" : "ПОПАДАНИЕ"));
        return lastAttackWasMiss;
    }

    // ← ПРОВЕРКА КРИТА
    public bool CheckCrit()
    {
        lastAttackWasCrit = Random.value < critChance;
        Debug.Log("⚡ Враг проверяет крит (" + (critChance * 100) + "%): " + (lastAttackWasCrit ? "КРИТ!" : "Обычный урон"));
        return lastAttackWasCrit;
    }

    public bool WasLastAttackCrit()
    {
        return lastAttackWasCrit;
    }

    public bool WasLastAttackMiss()
    {
        return lastAttackWasMiss;
    }

    public float GetPower()
    {
        return power;
    }

    public void SetPower(float newPower)
    {
        power = newPower;
        maxHP = power * 1f;
        currentHP = maxHP;
        UpdateHPUI();
    }
}