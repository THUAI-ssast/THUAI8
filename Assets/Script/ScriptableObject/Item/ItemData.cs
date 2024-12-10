using System;
using UnityEngine;
/// <summary>
/// 物品数据类，表示一类物品的数据，以ScriptableObject形式存储在Assets中。
/// </summary>
[CreateAssetMenu(menuName = "ItemSystem/Item/ItemData")]

[Serializable]
public class ItemData : ScriptableObject
{
    /// <summary>
    /// 物品类型枚举类
    /// </summary>
    public enum ItemTypeEnum
    {
        Consumable, // 消耗品
        Equipment, // 装备
        Weapon, // 武器
        Material, // 材料
        Suit // 套装
    }
    /// <summary>
    /// 物品名称
    /// </summary>
    public string ItemName;
    /// <summary>
    /// 物品图标
    /// </summary>
    public Sprite ItemIcon;
    /// <summary>
    /// 物品描述
    /// </summary>
    public string ItemDesc;
    /// <summary>
    /// 物品类型
    /// </summary>
    public ItemTypeEnum ItemType;
    /// <summary>
    /// 物品价值
    /// </summary>
    public int ItemValue;
    /// <summary>
    /// 物品使用方法
    /// </summary>
    public virtual void UseItem()
    {
        // 物品使用方法
    }
}
