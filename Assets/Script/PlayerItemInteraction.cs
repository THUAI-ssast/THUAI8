using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerItemInteraction : NetworkBehaviour
{
    // void Start()
    // {
    //     ItemManager.Instance.CmdSpawnItem();
    // }

    [Command]
    public void PickUpItem(GameObject currentObj)
    {
        Debug.Log("Player PickUpItem");
        // 将物品添加到背包中
        Item item = currentObj.GetComponent<Item>();
        // WorldDisplayItems.Remove(item);
        ItemStatusChange(currentObj, false, ItemOwner.PlayerBackpack, Vector3.zero, gameObject.GetComponent<NetworkIdentity>().netId);
    }
    [Command]
    public void DropItem(GameObject currentObj)
    {
        // 将物品放置到世界中
        Item item = currentObj.GetComponent<Item>();
        // WorldDisplayItems.Add(item);
        ItemStatusChange(currentObj, true, ItemOwner.World, gameObject.GetComponent<NetworkIdentity>().transform.position, 0);        
    }
    [Command]
    public void UseItem(GameObject currentObj)
    {
        Item item = currentObj.GetComponent<Item>();
        ItemStatusChange(currentObj, false, ItemOwner.PlayerSuit, Vector3.zero, 0);
        item.ItemData.UseItem();
    } 
    private void ItemStatusChange(GameObject obj, bool isDisplay, ItemOwner owner, Vector3 position, uint playerId)
    {
        obj.GetComponent<Item>().ItemLocationUpdate(owner, playerId);
        ObjectWorldDisplay(obj, isDisplay, position);
    }
    [ClientRpc]
    private void ObjectWorldDisplay(GameObject obj, bool isDisplay, Vector3 position)
    {
        Debug.Log("ObjectWorldDisplay");
        obj.transform.position = position;
        obj.GetComponent<SpriteRenderer>().enabled = isDisplay;
    }
}
