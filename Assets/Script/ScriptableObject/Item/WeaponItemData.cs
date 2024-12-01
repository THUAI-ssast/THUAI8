using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerHealth;


[CreateAssetMenu(menuName = "ItemSystem/Item/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public enum DamageType
    {
        Blunt,
        Puncture,
        Slash,
        Burn
    }
    public static Dictionary<DamageType, string> DamageTypeToChinese = new Dictionary<DamageType, string>()
    {
        { DamageType.Blunt, "¶ÛÆ÷" },
        { DamageType.Puncture, "´©´Ì" },
        { DamageType.Slash, "»Ó¿³" },
        { DamageType.Burn, "×ÆÉÕ" }
    };


    public int Durability;

    public float AttakAPCost;
    public float BasicDamage;

    public SerializableDictionary<PlayerHealth.BodyPosition, float> BodyDamageDictionary =
        new SerializableDictionary<PlayerHealth.BodyPosition, float>();

    public DamageType AttackDamageType;
}
