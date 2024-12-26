using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI行为类，点击资源点内物品槽拾取物品
/// </summary>
public class RPSlot : MonoBehaviour, IPointerClickHandler
{
    private Item _slotItem;
    [SerializeField] private GameObject _rp;
    public void SetItem(Item item)
    {
        _slotItem = item;
    }
    public Item GetItem()
    {
        return _slotItem;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _slotItem != null)
        {
            if(BackpackManager.Instance.IsBackpackFull)
            {
                UIManager.Instance.DisplayHoverStatusPanel("你的背包已满!");
                return;
            }
            BackpackManager.Instance.AddItem(_slotItem);
            //_rp.GetComponent<ResourcePointController>().RemoveItemFromResourcePoint(_slotItem);
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            player.GetComponent<PlayerItemInteraction>().PickUpItemFromRP(_rp, _slotItem.gameObject);
            _slotItem.ItemLocation.Owner = ItemOwner.PlayerBackpack;
        }
    }
}
