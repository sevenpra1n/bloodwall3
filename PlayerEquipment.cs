using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private Equipment weaponEquipped;
    [SerializeField] private Equipment headEquipped;
    [SerializeField] private Equipment bodyEquipped;
    [SerializeField] private Equipment legsEquipped;

    private int totalPower = 10;

    private void Start()
    {
        CalculateStats();
    }

    public void EquipItem(Equipment equipment)
    {
        if (equipment == null)
            return;

        switch (equipment.type)
        {
            case EquipmentType.Weapon:
                weaponEquipped = equipment;
                break;
            case EquipmentType.Head:
                headEquipped = equipment;
                break;
            case EquipmentType.Body:
                bodyEquipped = equipment;
                break;
            case EquipmentType.Legs:
                legsEquipped = equipment;
                break;
        }

        CalculateStats();
    }

    private void CalculateStats()
    {
        totalPower = 10;

        if (weaponEquipped != null)
            totalPower += weaponEquipped.power;

        if (headEquipped != null)
            totalPower += headEquipped.power;

        if (bodyEquipped != null)
            totalPower += bodyEquipped.power;

        if (legsEquipped != null)
            totalPower += legsEquipped.power;
    }

    public Equipment GetEquipped(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon:
                return weaponEquipped;
            case EquipmentType.Head:
                return headEquipped;
            case EquipmentType.Body:
                return bodyEquipped;
            case EquipmentType.Legs:
                return legsEquipped;
            default:
                return null;
        }
    }

    public int GetTotalPower()
    {
        return totalPower;
    }
}