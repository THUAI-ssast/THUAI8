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
    [SerializeField] public ItemData ItemData;
    /// <summary>
    /// 物品的拥有者信息
    /// </summary>
    [SerializeField] private ItemOwnerInfo _itemlocation;
    /// <summary>
    /// 物品的网络ID
    /// </summary>
    private int _itemID; // TODO: NetworkIdentity
    /// <summary>
    /// 更新物品的拥有者信息
    /// </summary>
    /// <param name="owner">物品拥有者</param>
    /// <param name="playerId">若物品拥有者为玩家，则为玩家ID；否则为0</param>
    public void ItemLocationUpdate(ItemOwner owner, uint playerId)
    {
        _itemlocation.Owner = owner;
        _itemlocation.PlayerId = playerId;
    }
    /// <summary>
    /// 右键点击物品拾取物品到背包
    /// </summary>
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1))
        {
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            player.GetComponent<PlayerItemInteraction>().PickUpItem(gameObject);
            BackpackManager.Instance.AddItem(gameObject.GetComponent<Item>());
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
