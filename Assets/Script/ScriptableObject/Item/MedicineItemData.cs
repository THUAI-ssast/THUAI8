using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 药品数据类
/// </summary>
[CreateAssetMenu(menuName = "ItemSystem/Item/MedicineItemData")]
public class MedicineItemData : ItemData
{
    /// <summary>
    /// 初始耐久度(可使用次数)
    /// </summary>
    public int Durability;
    /// <summary>
    /// 使用在对应部位能回复的血量
    /// </summary>
    public SerializableDictionary<PlayerHealth.BodyPosition, float> BodyHealDictionary =
        new SerializableDictionary<PlayerHealth.BodyPosition, float>();
}
