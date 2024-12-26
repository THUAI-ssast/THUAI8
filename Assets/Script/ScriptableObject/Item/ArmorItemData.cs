using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 护甲数据类
/// </summary>
[CreateAssetMenu(menuName = "ItemSystem/Item/ArmorItemData")]
public class ArmorItemData : ItemData
{
    /// <summary>
    /// 初始耐久度(承伤值)
    /// </summary>
    public int Durability;
    /// <summary>
    /// 可装备部位
    /// </summary>
    public PlayerHealth.BodyPosition EquipBodyPosition;
    /// <summary>
    /// 减伤字典，每种伤害类型对应不同减伤比例
    /// </summary>
    public SerializableDictionary<WeaponItemData.DamageType, float> DamageTypeDictionary =
        new SerializableDictionary<WeaponItemData.DamageType, float>();

}
