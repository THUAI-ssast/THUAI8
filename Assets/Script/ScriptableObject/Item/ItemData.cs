using System;
using UnityEngine;
using static PlayerHealth;
/// <summary>
/// 物品数据类，表示一类物品的数据，以ScriptableObject形式存储在Assets中
/// </summary>
[CreateAssetMenu(menuName = "ItemSystem/Item/ItemData")]

[System.Serializable]
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
    public string ItemDesc
    {
        get
        {
            if (string.IsNullOrEmpty(_itemDesc))
            {
                if (this is ArmorItemData armorItemData)
                {
                    ItemDesc = $"防具：装备部位:{BodyToChinese[armorItemData.EquipBodyPosition]}\n";
                    foreach (WeaponItemData.DamageType damageType in Enum.GetValues(typeof(WeaponItemData.DamageType)))
                    {
                        if (armorItemData.DamageTypeDictionary.ContainsKey(damageType))
                        {
                            float damageMult = armorItemData.DamageTypeDictionary.Get(damageType);
                            ItemDesc += $"{WeaponItemData.DamageTypeToChinese[damageType]}：({damageMult})倍\n";
                        }
                    }
                }
                else if (this is WeaponItemData weaponItemData)
                {
                    ItemDesc = $"武器：AP消耗({weaponItemData.AttakAPCost})\n" +
                                        $"({WeaponItemData.DamageTypeToChinese[weaponItemData.AttackDamageType]})伤害" +
                                        $"({weaponItemData.BasicDamage})HP\n";
                    foreach (BodyPosition bodyPosition in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
                    {
                        if (weaponItemData.BodyDamageDictionary.ContainsKey(bodyPosition))
                        {
                            float damageMult = weaponItemData.BodyDamageDictionary.Get(bodyPosition);
                            ItemDesc += $"{BodyToChinese[bodyPosition]}：({damageMult}倍)\n";
                        }
                        else
                        {
                            ItemDesc += $"{BodyToChinese[bodyPosition]}：{1}倍\n";
                        }
                    }
                }
                else if (this is MedicineItemData medicineItemData)
                {
                    ItemDesc = $"药品：" + (ItemName == "止痛药" || ItemName == "肾上腺素" ? "全身回复\n" : "选择部位回复\n");
                    foreach (BodyPosition bodyPosition in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
                    {
                        if (medicineItemData.BodyHealDictionary.ContainsKey(bodyPosition))
                        {
                            float healHP = medicineItemData.BodyHealDictionary.Get(bodyPosition);
                            ItemDesc += $"用于{BodyToChinese[bodyPosition]}：({healHP})HP\n";
                        }
                    }
                }
            }
            else if (this is not MedicineItemData && this is not ArmorItemData && this is not WeaponItemData)
            {
                _itemDesc = _itemDesc.Replace("，", ",\n");
            }

            return _itemDesc;
        }
        set => _itemDesc = value;
    }
    public string _itemDesc;
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
