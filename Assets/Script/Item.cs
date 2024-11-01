using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] public ItemData ItemData;
    [SerializeField] private ItemOwnerInfo _itemlocation;
    private int _itemID; // TODO: NetworkIdentity
    public void ItemLocationUpdate(ItemOwner owner, uint playerId)
    {
        _itemlocation.Owner = owner;
        _itemlocation.PlayerId = playerId;
    }
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

public enum ItemOwner
{
    PlayerBackpack,
    World,
    PlayerSuit,
    Other
}
[System.Serializable]
public class ItemOwnerInfo
{
    [SerializeField]private ItemOwner _owner;    
    // private Vector3 _position;    // 当 owner 为 World 时，表示物品的世界坐标；否则为 Vector3.zero
    private uint _playerId;        // 当 owner 为 Player 时，表示所属玩家的 ID；否则为 0
    public ItemOwner Owner
    {
        get => _owner;
        set => _owner = value;
    }

    // public Vector3 Position
    // {
    //     get => _position;
    //     set
    //     {
    //         if (Owner == ItemOwner.World)
    //         {
    //             _position = value;
    //         }
    //         else
    //         {
    //             _position = Vector3.zero;
    //         }
    //     }
    // }

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
