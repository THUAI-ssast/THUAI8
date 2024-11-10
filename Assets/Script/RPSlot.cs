using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RPSlot : MonoBehaviour, IPointerClickHandler
{
    private Item _slotItem;
    [SerializeField] private GameObject _rp;
    public void SetItem(Item item)
    {
        _slotItem = item;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _slotItem != null)
        {
            BackpackManager.Instance.AddItem(_slotItem);
            //_rp.GetComponent<ResourcePointController>().RemoveItemFromResourcePoint(_slotItem);
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            player.GetComponent<PlayerItemInteraction>().PickUpItemFromRP(_rp, _slotItem.gameObject);
        }
    }
}
