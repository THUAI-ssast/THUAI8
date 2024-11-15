using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ItemSystem/Item/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public enum DamageType
    {
        Puncture,
        Blunt,
        Burn
    }


    public int Durability;

    public float AttakAPCost;
    public float BasicDamage;

    public SerializableDictionary<PlayerHealth.BodyPosition, float> BodyDamageDictionary =
        new SerializableDictionary<PlayerHealth.BodyPosition, float>();

    public DamageType AttackDamageType;
}
