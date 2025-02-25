using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;
using Mirror.BouncyCastle.Asn1.X509;
using TMPro;
/// <summary>
/// 玩家行为类，管理玩家与物品的交互，多执行[Command]方法。
/// </summary>
public class PlayerItemInteraction : NetworkBehaviour
{
    /// <summary>
    /// 随机玩家，用于处理Item行为需要发command但不属于特定玩家的事件
    /// </summary>
    public static PlayerItemInteraction RandomPlayer;
    private void Awake()
    {
        RandomPlayer = this;
    }
    /// <summary>
    /// 服务器处理玩家拾取物品到背包事件。
    /// </summary>
    /// <param name="currentObj">拾取的物品的GameObject</param>
    [Command]
    public void PickUpItem(GameObject currentObj)
    {
        ItemStatusChange(currentObj, false, ItemOwner.PlayerBackpack, Vector3.zero, gameObject.GetComponent<NetworkIdentity>().netId);
    }
    /// <summary>
    /// 服务器处理玩家丢弃物品到世界事件。
    /// </summary>
    /// <param name="currentObj">丢弃的物品的GameObject</param>
    [Command]
    public void DropItem(GameObject currentObj)
    {
        ItemStatusChange(currentObj, true, ItemOwner.World, gameObject.GetComponent<NetworkIdentity>().transform.position, 0);        
    }
    /// <summary>
    /// 服务器处理玩家创建物品事件。
    /// </summary>
    /// <param name="itemData_pth">Resources中要创建的物品信息路径</param>
    /// <param name="owner">物品拥有者</param>
    /// <param name="player">发出请求的玩家</param>
    [Command]
    public void CreateItem(string itemData_pth, ItemOwner owner, GameObject player)
    {
        GameObject instance = Instantiate(Resources.Load<GameObject>("ScriptableObject/Items/General_Item"), new Vector3(100, 100, 0), Quaternion.identity);
        NetworkServer.Spawn(instance);
        NetworkIdentity playerIdentity = player.GetComponent<NetworkIdentity>();
        instance.GetComponent<Item>().ItemData = Resources.Load<ItemData>(itemData_pth);
        RpcInitInstanceOnClients(instance, itemData_pth, owner, playerIdentity.netId,null);
        TargetNotifyItemCreatedInBackpack(playerIdentity.connectionToClient, instance);
    }
    /// <summary>
    /// 只应该被server调用
    /// </summary>
    /// <param name="itemData_pth"></param>
    /// <param name="owner"></param>
    /// <param name="resourcePoint"></param>
    public void CreateItemForClient(string itemData_pth, ItemOwner owner, GameObject resourcePoint)
    {
        GameObject instance = Instantiate(Resources.Load<GameObject>("ScriptableObject/Items/General_Item"), new Vector3(100, 100, 0), Quaternion.identity);
        NetworkServer.Spawn(instance);
        instance.GetComponent<Item>().ItemData = Resources.Load<ItemData>(itemData_pth);
        RpcInitInstanceOnClients(instance, itemData_pth, owner, 0, resourcePoint);
    }
    /// <summary>
    /// 由服务器调用，令每个客户端初始化Item状态和GameObject状态
    /// </summary>
    /// <param name="instance">Item的GameObject</param>
    /// <param name="itemData_pth">Resources中要创建的物品的信息的路径</param>
    /// <param name="owner">物品拥有者</param>
    /// <param name="playerId">玩家id</param>
    [ClientRpc]
    public void RpcInitInstanceOnClients(GameObject instance, string itemData_pth, ItemOwner owner, uint playerId,GameObject resourcePoint)
    {
        ItemData itemData = Resources.Load<ItemData>(itemData_pth);
        var spriteRenderer = instance.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        if(itemData == null)
        {
            Debug.Log(itemData_pth);
        }
        spriteRenderer.sprite = itemData.ItemIcon;
        var item = instance.GetComponent<Item>();
        item.Initialize(itemData, owner, playerId);
        if (resourcePoint)
            resourcePoint.GetComponent<ResourcePointController>().AddItemToResourcePoint(item);
    }
    /// <summary>
    /// 服务器处理玩家销毁物品事件。
    /// </summary>
    /// <param name="currentObj">要销毁的GameObject</param>
    public void DestroyItem(GameObject currentObj)
    {
        if (isServer) 
            NetworkServer.Destroy(currentObj);
        else
            CmdDestroyItem(currentObj);
    }
    [Command]
    public void CmdDestroyItem(GameObject currentObj)
    {
        NetworkServer.Destroy(currentObj);
    }

    /// <summary>
    /// 减少耐久度的统一接口，若耐久度为0则销毁。
    /// 请通过对应物品拥有者的playerInteractive.DecreaseDurability()使用
    /// </summary>
    /// <param name="itemObject">需要减少耐久度的物体</param>
    public void DecreaseDurability(GameObject itemObject,float damage = 1)
    {
        if (isServer)
        {
            RpcDecreaseItemDurability(itemObject, damage);
        }
        else
        {
            CmdDecreaseDurability(itemObject, damage);
        }
    }
    [Command]
    public void CmdDecreaseDurability(GameObject itemObject, float damage)
    {
        RpcDecreaseItemDurability(itemObject,damage);
    }
    /// <summary>
    /// DecreaseDurability的附属同步函数
    /// </summary>
    /// <param name="itemObject"></param>
    [ClientRpc]
    private void RpcDecreaseItemDurability(GameObject itemObject, float damage )
    {
        var item = itemObject.GetComponent<Item>();
        if (item!=null&&item.CurrentDurability!=-1)
        {
            item.DecreaseDurability(damage);
            if (item.ItemLocation.Owner==ItemOwner.PlayerBackpack)
            {
                BackpackManager.Instance.RefreshSlots();
                BackpackManager.Instance.RefreshArmorDisplay();
            }
        }
    }

    /// <summary>
    /// 服务器处理玩家使用物品事件。
    /// </summary>
    /// <param name="currentObj">使用的物品的GameObject</param>
    [Command]
    public void UseItem(GameObject currentObj)
    {
        Item item = currentObj.GetComponent<Item>();
    }
    [Command]
    public void PickUpItemFromRP(GameObject rp, GameObject instance)
    {
        rp.GetComponent<ResourcePointController>().RemoveItemFromResourcePoint(instance.GetComponent<Item>());
    }
    /// <summary>
    /// 由客户端执行，更新物品状态。
    /// </summary>
    /// <param name="obj">物品GameObject</param>
    /// <param name="isDisplay">是否在世界中显示</param>
    /// <param name="owner">物品拥有者</param>
    /// <param name="position">若物品拥有者为世界，则为玩家位置；否则为0向量</param>
    /// <param name="playerId">若物品拥有者为玩家，则为玩家ID；否则为0</param>
    [ClientRpc]
    private void ItemStatusChange(GameObject obj, bool isDisplay, ItemOwner owner, Vector3 position, uint playerId)
    {
        obj.GetComponent<Item>().ItemLocationUpdate(owner, playerId);
        ObjectWorldDisplay(obj, isDisplay, position);
    }
    /// <summary>
    /// 更新物品在世界中的显示状态。
    /// </summary>
    /// <param name="obj">物品GameObject</param>
    /// <param name="isDisplay">是否在世界中显示</param>
    /// <param name="position">若物品拥有者为世界，则为玩家位置；否则为0向量</param>\
    private void ObjectWorldDisplay(GameObject obj, bool isDisplay, Vector3 position)
    {
        obj.transform.position = position;
        obj.GetComponent<SpriteRenderer>().enabled = isDisplay;
    }

    [TargetRpc]
    private void TargetNotifyItemCreatedInBackpack(NetworkConnectionToClient player, GameObject instance)
    {
        BackpackManager.Instance.AddItem(instance.GetComponent<Item>());
    }
}
