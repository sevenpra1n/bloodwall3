using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] protected float maxHP = 100f;  // ← ПРОСТО ПЕРЕМЕННАЯ ТЕПЕРЬ
    protected float currentHP;

    [SerializeField] protected Image hpBar;
    [SerializeField] protected Text hpText;
    [SerializeField] protected Image[] hpBarTextures;

    protected bool isShielded = false;
    protected float shieldDuration = 0f;

    protected virtual void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }

    public virtual void TakeDamage(float damage)
    {
        if (isShielded)
        {
            damage *= 0.5f;
            shieldDuration -= Time.deltaTime;
            if (shieldDuration <= 0)
                isShielded = false;
        }

        currentHP -= damage;
        if (currentHP < 0)
            currentHP = 0;

        UpdateHPUI();
    }

    public void ApplyShield(float duration)
    {
        isShielded = true;
        shieldDuration = duration;
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;

        UpdateHPUI();
    }

    protected void UpdateHPUI()
    {
        if (hpText != null)
            hpText.text = currentHP.ToString("F0") + "/" + maxHP.ToString("F0");

        UpdateHPBarTexture();
    }

    protected void UpdateHPBarTexture()
    {
        if (hpBarTextures == null || hpBarTextures.Length == 0)
            return;

        foreach (Image texture in hpBarTextures)
        {
            if (texture != null)
                texture.enabled = false;
        }

        float hpPercent = currentHP / maxHP;

        int textureIndex = Mathf.RoundToInt(hpPercent * 8);
        if (textureIndex > 8) textureIndex = 8;
        if (textureIndex < 0) textureIndex = 0;

        if (textureIndex < hpBarTextures.Length && hpBarTextures[textureIndex] != null)
        {
            hpBarTextures[textureIndex].enabled = true;
        }
    }

    public float GetCurrentHP()
    {
        return currentHP;
    }

    public float GetMaxHP()
    {
        return maxHP;
    }

    public bool IsAlive()
    {
        return currentHP > 0;
    }
}