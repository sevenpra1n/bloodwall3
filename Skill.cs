using UnityEngine;

[System.Serializable]
public class Skill
{
    public string skillName;
    public int manaCost;
    public string description;
    public SkillEffect effect;

    public enum SkillEffect
    {
        Fireball,      // 20 урона
        Shield,        // 50% урона, 1 ход
        Heal,          // 30 HP
        PoisonDagger   // 10 урона + 5 урона/ход на 2 хода
    }

    public Skill(string name, int cost, SkillEffect effectType)
    {
        skillName = name;
        manaCost = cost;
        effect = effectType;

        switch (effectType)
        {
            case SkillEffect.Fireball:
                description = "Огненный шар: 20 урона (30 маны)";
                break;
            case SkillEffect.Shield:
                description = "Щит: -50% урона (25 маны)";
                break;
            case SkillEffect.Heal:
                description = "Лечение: +30 HP (40 маны)";
                break;
            case SkillEffect.PoisonDagger:
                description = "Яд: 10 + 5/ход x2 (20 маны)";
                break;
        }
    }
}