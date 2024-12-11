using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 数据类，用于记录合成路径
/// </summary>
[CreateAssetMenu(fileName = "new CraftWay", menuName = "ItemSystem/Craft way")]
public class CraftWayData : ScriptableObject
{
    /// <summary>
    /// 合成消耗的物品
    /// </summary>
    public List<ItemData> CostItems;
    /// <summary>
    /// 合成的催化剂物品，不会被消耗
    /// </summary>
    public List<ItemData> CatalystItems;
    /// <summary>
    /// 合成的产物
    /// </summary>
    public ItemData ProductItem;
}
