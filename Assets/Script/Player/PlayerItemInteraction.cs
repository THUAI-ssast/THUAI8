using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/// <summary>
/// 玩家与物品的交互类，挂载在player的GameObject上。执行[Command]方法。
/// </summary>
public class PlayerItemInteraction : NetworkBehaviour
{
    /// <summary>
    /// 服务器处理玩家拾取物品到背包事件。
    /// </summary>
    /// <param name="currentObj">拾取的物品的GameObject</param>
    [Command]
    public void PickUpItem(GameObject currentObj)
    {
        Item item = currentObj.GetComponent<Item>();
        ItemStatusChange(currentObj, false, ItemOwner.PlayerBackpack, Vector3.zero, gameObject.GetComponent<NetworkIdentity>().netId);
    }
    /// <summary>
    /// 服务器处理玩家丢弃物品到世界事件。
    /// </summary>
    /// <param name="currentObj">丢弃的物品的GameObject</param>
    [Command]
    public void DropItem(GameObject currentObj)
    {
        Item item = currentObj.GetComponent<Item>();
        ItemStatusChange(currentObj, true, ItemOwner.World, gameObject.GetComponent<NetworkIdentity>().transform.position, 0);        
    }
    /// <summary>
    /// 服务器处理玩家使用物品事件。
    /// </summary>
    /// <param name="currentObj">使用的物品的GameObject</param>
    [Command]
    public void UseItem(GameObject currentObj)
    {
        Item item = currentObj.GetComponent<Item>();
        ItemStatusChange(currentObj, false, ItemOwner.PlayerSuit, Vector3.zero, 0);
        item.ItemData.UseItem();
    } 
    /// <summary>
    /// 由服务器执行，更新物品状态。调用[ClientRpc]方法。
    /// </summary>
    /// <param name="obj">物品GameObject</param>
    /// <param name="isDisplay">是否在世界中显示</param>
    /// <param name="owner">物品拥有者</param>
    /// <param name="position">若物品拥有者为世界，则为玩家位置；否则为0向量</param>
    /// <param name="playerId">若物品拥有者为玩家，则为玩家ID；否则为0</param>
    private void ItemStatusChange(GameObject obj, bool isDisplay, ItemOwner owner, Vector3 position, uint playerId)
    {
        obj.GetComponent<Item>().ItemLocationUpdate(owner, playerId);
        ObjectWorldDisplay(obj, isDisplay, position);
    }
    /// <summary>
    /// 由客户端执行，更新物品在世界中的显示状态。
    /// </summary>
    /// <param name="obj">物品GameObject</param>
    /// <param name="isDisplay">是否在世界中显示</param>
    /// <param name="position">若物品拥有者为世界，则为玩家位置；否则为0向量</param>
    [ClientRpc]
    private void ObjectWorldDisplay(GameObject obj, bool isDisplay, Vector3 position)
    {
        Debug.Log("ObjectWorldDisplay");
        obj.transform.position = position;
        obj.GetComponent<SpriteRenderer>().enabled = isDisplay;
    }
}
