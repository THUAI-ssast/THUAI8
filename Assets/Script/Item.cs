using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 物品类，表示一个物品，挂载在物品的GameObject上。
/// </summary>
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
    [SerializeField] private float _pickUpDistance = 1;
    /// <summary> 
    /// 物品的网络ID
    /// </summary>
    private uint _itemID;
    void Awake()
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
            PlayerItemInteraction.RamdomPlayer.CreateItemForClient(itemData_pth, owner,place);
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
    private bool CanBePickedUp()
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
    private void OnMouseOver()
    {  
        if(Input.GetMouseButtonDown(1) && CanBePickedUp())
        {
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            Item item = gameObject.GetComponent<Item>();
            player.GetComponent<PlayerItemInteraction>().PickUpItem(item.gameObject);
            BackpackManager.Instance.AddItem(item);
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
