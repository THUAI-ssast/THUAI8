using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemSystem/Item/ArmorItemData")]
public class ArmorItemData : ItemData
{
    public int Durability;

    public PlayerHealth.BodyPosition EquipBodyPosition;

    public SerializableDictionary<WeaponItemData.DamageType, float> DamageTypeDictionary =
        new SerializableDictionary<WeaponItemData.DamageType, float>();

}
