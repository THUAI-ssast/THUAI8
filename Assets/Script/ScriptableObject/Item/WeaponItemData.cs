using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerHealth;

/// <summary>
/// 武器数据类
/// </summary>
[CreateAssetMenu(menuName = "ItemSystem/Item/WeaponItemData")]
public class WeaponItemData : ItemData
{
    /// <summary>
    /// 武器伤害类型
    /// </summary>
    public enum DamageType
    {
        Blunt,
        Puncture,
        Slash,
        Burn
    }
    /// <summary>
    /// 将伤害类型转为中文string的字典
    /// </summary>
    public static Dictionary<DamageType, string> DamageTypeToChinese = new Dictionary<DamageType, string>()
    {
        { DamageType.Blunt, "钝器" },
        { DamageType.Puncture, "穿刺" },
        { DamageType.Slash, "挥砍" },
        { DamageType.Burn, "灼烧" }
    };

    /// <summary>
    /// 初始耐久度
    /// </summary>
    public int Durability;
    /// <summary>
    /// 每次攻击消耗的AP值
    /// </summary>
    public float AttakAPCost;
    /// <summary>
    /// 基础伤害
    /// </summary>
    public float BasicDamage;
    /// <summary>
    /// 攻击不同部位的伤害乘数
    /// </summary>
    public SerializableDictionary<PlayerHealth.BodyPosition, float> BodyDamageDictionary =
        new SerializableDictionary<PlayerHealth.BodyPosition, float>();
    /// <summary>
    /// 伤害类型
    /// </summary>
    public DamageType AttackDamageType;
}
