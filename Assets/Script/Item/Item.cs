using System;
using Mirror;
using UnityEngine;
using static PlayerHealth;

/// <summary>
/// 物品类，表示一个物品，挂载在物品的GameObject上
/// </summary>
[Serializable]
public class Item : NetworkBehaviour
{
    /// <summary>
    /// 一类物品的数据
    /// </summary>
    public ItemData ItemData;
    /// <summary>
    /// 物品的拥有者信息
    /// </summary>
    public ItemOwnerInfo ItemLocation;
    /// <summary>
    /// 物品的拾取距离
    /// </summary>
    [SerializeField] protected float _pickUpDistance = 1.5f;

    /// <summary>
    /// 物体耐久度，若无耐久度则为-1，有耐久度的物体耐久度归零会损坏
    /// </summary>
    public float CurrentDurability { get; set; } = -1;
    public float MaxDurability { get; private set; } = -1;
    /// <summary> 
    /// 物品的网络ID
    /// </summary>
    protected uint _itemID;
    protected void Awake()
    {
        _itemID = gameObject.GetComponent<NetworkIdentity>().netId;
        if(ItemLocation == null)
        {
            ItemLocation = new ItemOwnerInfo();
        }
    }
    /// <summary>
    /// 创建物品实例
    /// </summary>
    /// <param name="itemData_pth">Resources中要创建的物品的信息的路径</param>
    /// <param name="owner">物品拥有者</param>
    /// <param name="player">若不为null，则为发出请求的玩家；否则从服务器调用</param>
    /// <param name="place">物品创建到哪里。若为null，则为玩家背包；否则从服务器调用，为资源点的引用</param>
    /// <returns></returns>
    public static void Create(string itemData_pth, ItemOwner owner, GameObject player, GameObject place)
    {
        if (player == null)
        {
            PlayerItemInteraction.RandomPlayer.CreateItemForClient(itemData_pth, owner,place);
        }
        else
        {
            player.GetComponent<PlayerItemInteraction>().CreateItem(itemData_pth, owner, player);
        }
    }
    /// <summary>
    /// 初始化Item
    /// </summary>
    /// <param name="itemData">物品信息</param>
    /// <param name="owner">物品拥有者</param>
    /// <param name="playerId">玩家id</param>
    public void Initialize(ItemData itemData, ItemOwner owner, uint playerId)
    {
        ItemData = itemData;
        ItemLocation.Owner = owner;
        ItemLocation.PlayerId = playerId;
        if (itemData is ArmorItemData armorItemData)
        {
            MaxDurability = armorItemData.Durability;
            ItemData.ItemDesc = $"防具：装备部位:{BodyToChinese[armorItemData.EquipBodyPosition]}\n";
            foreach (WeaponItemData.DamageType damageType in Enum.GetValues(typeof(WeaponItemData.DamageType)))
            {
                if (armorItemData.DamageTypeDictionary.ContainsKey(damageType))
                {
                    float damageMult = armorItemData.DamageTypeDictionary.Get(damageType);
                    ItemData.ItemDesc += $"{WeaponItemData.DamageTypeToChinese[damageType]}：({damageMult})倍\n";
                }
            }
        }
        else if (itemData is WeaponItemData weaponItemData)
        {
            MaxDurability = weaponItemData.Durability;
            ItemData.ItemDesc = $"武器：AP消耗({weaponItemData.AttakAPCost})\n" +
                                $"({WeaponItemData.DamageTypeToChinese[weaponItemData.AttackDamageType]})伤害" +
                                $"({weaponItemData.BasicDamage})HP\n";
            foreach (BodyPosition bodyPosition in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
            {
                if (weaponItemData.BodyDamageDictionary.ContainsKey(bodyPosition))
                {
                    float damageMult = weaponItemData.BodyDamageDictionary.Get(bodyPosition);
                    ItemData.ItemDesc += $"{BodyToChinese[bodyPosition]}：({damageMult}倍)\n";
                }
                else
                {
                    ItemData.ItemDesc += $"{BodyToChinese[bodyPosition]}：{1}倍\n";
                }
            }
        }
        else if (itemData is MedicineItemData medicineItemData)
        {
            MaxDurability = medicineItemData.Durability;
            ItemData.ItemDesc = $"药品："+ (itemData.ItemName == "止痛药" || itemData.ItemName == "肾上腺素" ? "全身回复\n" : "选择部位回复\n");
            foreach (BodyPosition bodyPosition in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
            {
                if (medicineItemData.BodyHealDictionary.ContainsKey(bodyPosition))
                {
                    float healHP = medicineItemData.BodyHealDictionary.Get(bodyPosition);
                    ItemData.ItemDesc += $"用于{BodyToChinese[bodyPosition]}：({healHP})HP\n";
                }
            }
        }
        CurrentDurability = MaxDurability;
    }

    /// <summary>
    /// 最终执行耐久度改变的函数，若想调用请通过对应物品拥有者的playerInteractive.DecreaseDurability();
    /// </summary>
    /// <param name="count">减少的耐久度，默认为1</param>
    public void DecreaseDurability(float count = 1)
    {
        if (MaxDurability == -1)
            return;
        CurrentDurability = (float)Math.Round(CurrentDurability - count,1);
        if (CurrentDurability<=0)
        {
            Item.Destroy(this,PlayerItemInteraction.RandomPlayer.gameObject);
        }
    }
    /// <summary>
    /// 销毁物品
    /// </summary>
    /// <param name="item">要销毁的物品</param>
    /// <param name="player">发出请求的玩家</param>
    public static void Destroy(Item item, GameObject player)
    {
        player.GetComponent<PlayerItemInteraction>().DestroyItem(item.gameObject);
    }
    
    /// <summary>
    /// 更新物品的拥有者信息
    /// </summary>
    /// <param name="owner">物品拥有者</param>
    /// <param name="playerId">若物品拥有者为玩家，则为玩家ID；否则为0</param>
    public void ItemLocationUpdate(ItemOwner owner, uint playerId)
    {
        ItemLocation.Owner = owner;
        ItemLocation.PlayerId = playerId;
    }
    /// <summary>
    /// 判断物品是否可以被拾取
    /// </summary>
    /// <returns>如果物品可以被拾取，返回true；否则返回false</returns>
    protected bool CanBePickedUp()
    {
        if(ItemLocation.Owner != ItemOwner.World) return false;
        if(UIManager.Instance.IsUIActivating == true) return false;
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        if (Vector3.Distance(gameObject.transform.position, player.transform.position) > _pickUpDistance) return false;
        return true;
    }
    /// <summary>
    /// 右键点击物品拾取物品到背包
    /// </summary>
    protected void OnMouseOver()
    {  
        if(Input.GetMouseButtonDown(1) && CanBePickedUp())
        {
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            Item item = gameObject.GetComponent<Item>();
            player.GetComponent<PlayerItemInteraction>().PickUpItem(item.gameObject);
            BackpackManager.Instance.AddItem(item);
        }
    }

    private void OnDestroy()
    {
        if (BackpackManager.Instance!=null)
        {
            BackpackManager.Instance.RemoveItem(this);
        }
        
    }
}
/// <summary>
/// 物品拥有者枚举类
/// </summary>
public enum ItemOwner
{
    PlayerBackpack,
    World,
    PlayerSuit,
    Other
}
/// <summary>
/// 物品拥有者信息类
/// </summary>
[System.Serializable]
public class ItemOwnerInfo
{
    /// <summary>
    /// 物品拥有者
    /// </summary>
    [SerializeField]private ItemOwner _owner;    
    /// <summary>
    /// 物品拥有者为玩家时，玩家的ID
    /// </summary>
    private uint _playerId;
    /// <summary>
    /// 物品拥有者的设置和获取
    /// </summary>
    public ItemOwner Owner
    {
        get => _owner;
        set => _owner = value;
    }
    /// <summary>
    /// 玩家ID的设置和获取
    /// </summary>
    public uint PlayerId
    {
        get => _playerId;
        set
        {
            if (Owner == ItemOwner.PlayerSuit || Owner == ItemOwner.PlayerBackpack)
            {   
                _playerId = value;
            }
            else
            {
                _playerId = 0;
            }
        }
    }

}
