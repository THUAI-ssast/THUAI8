using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "ItemSystem/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemTypeEnum
    {
        Consumable,
        Equipment,
        Weapon,
        Material,
        Suit
    }
    public string ItemName;
    public Sprite ItemIcon;
    public string ItemDesc;
    public ItemTypeEnum ItemType;
    public int ItemValue;
    public virtual void UseItem()
    {
        // 物品使用方法
    }
}
