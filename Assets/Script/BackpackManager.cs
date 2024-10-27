using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackpackManager : MonoBehaviour
{
    [SerializeField]private GameObject _bagPanel;
    public static BackpackManager Instance;
    private List<Item> _itemList = new List<Item>();  
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        RefreshSlots();
    }
    public void Additem(Item item) 
    {
        Debug.Log("AddItem");
        // 从背包中添加物品
        _itemList.Add(item);
        // Debug.Log(_itemList);
        RefreshSlots();
    }

    public void UseItem(Item item)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        player.GetComponent<PlayerItemInteraction>().UseItem(item.gameObject);
        _itemList.Remove(item);
        RefreshSlots();
    }

    public void RemoveItem(Item item)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        _itemList.Remove(item);
        RefreshSlots();
        player.GetComponent<PlayerItemInteraction>().DropItem(item.gameObject);
    }
    private void RefreshSlots()
    {
        Debug.Log(_itemList.Count);
        Transform slots = _bagPanel.transform.Find("ItemsPanel/Scroll View/Viewport/Slots");
        for (int i = 0; i < slots.childCount; i++)
        {
            if(i < _itemList.Count)
            {
                slots.GetChild(i).GetChild(0).GetComponent<Image>().sprite = _itemList[i].ItemData.ItemIcon;
                slots.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = _itemList[i].ItemData.ItemName;
                slots.GetChild(i).GetComponent<SlotMenuTrigger>().SetItem(_itemList[i]);
            }
            else
            {
                slots.GetChild(i).GetChild(0).GetComponent<Image>().sprite = null;
            }
        }
    }
}
