using UnityEngine;

public enum CharacterType { Knight, Archer }
public enum AttackType { Melee, Ranged }

[CreateAssetMenu(fileName = "CharacterData", menuName = "Character/CharacterData")]
[System.Serializable]
public class CharacterData : ScriptableObject
{
    public CharacterType type;
    public string characterName;
    public string description;
    public Sprite icon;
    public int price;       // 0 for Knight, 5000 for Archer
    public bool isUnlocked; // Knight always true; Archer requires purchase
    public float hpMultiplier;  // Knight: 1.0f, Archer: 0.8f
    public float dodgeChance;   // Knight: 0.1f (10%), Archer: 0.2f (20%)
    [SerializeField] public AttackType attackType;
    public Sprite[] idleSprites;
    public Sprite[] attackSprites;
    [SerializeField] public Sprite skillIcon;
    [SerializeField] public Sprite[] skillSpritesNormal;    // ← Обычное состояние
    [SerializeField] public Sprite[] skillSpritesHover;     // ← При наведении
    [SerializeField] public Sprite[] skillSpritesPressed;   // ← При нажатии
    [SerializeField] public Sprite[] skillSpritesCooldown;  // ← На кулдауне
    [SerializeField] public Sprite skillSpriteNoMana;       // ← Нет маны
}