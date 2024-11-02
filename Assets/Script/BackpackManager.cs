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
    public void AddItem(Item item) 
    {
        // 从背包中添加物品
        _itemList.Add(item);
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
                slots.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    /// <summary>
    /// 确定背包内是否包含合成所需要物品
    /// </summary>
    /// <param name="craftWay">检测的目标合成路径</param>
    /// <returns>若为-1则全部满足，否则返回满足物品个数</returns>
    public int IsCraftSatisfied(CraftWayData craftWay)
    {
        int count = 0;
        List<Item> testItemList = new List<Item>(_itemList);
        foreach (ItemData costItem in craftWay.CostItems)
        {
            if(!testItemList.Remove(testItemList.Find(i=>i.ItemData.ItemName==costItem.ItemName)))
                return count;
            count++;
        }
        foreach (ItemData catalystItem in craftWay.CatalystItems)
        {
            if (!testItemList.Remove(testItemList.Find(i => i.ItemData.ItemName == catalystItem.ItemName)))
                return count;
            count++;
        }
        return -1;
    }

    /// <summary>
    /// 应用合成，在背包内销毁Cost Item并增加Product Item
    /// </summary>
    /// <param name="craftWay">要应用的目标合成路径</param>
    /// <returns>若为-1则全部满足并成功应用，否则返回满足物品个数</returns>
    public int DeployCraft(CraftWayData craftWay)
    {
        int count = 0;
        List<Item> testItemList = new List<Item>(_itemList);
        foreach (ItemData costItem in craftWay.CostItems)
        {
            if (!testItemList.Remove(testItemList.FindLast(i => i.ItemData.ItemName == costItem.ItemName)))
                return count;
            count++;
        }
        foreach (ItemData catalystItem in craftWay.CatalystItems)
        {
            if (!testItemList.Remove(testItemList.FindLast(i => i.ItemData.ItemName == catalystItem.ItemName)))
                return count;
            count++;
        }
        _itemList = testItemList;
        return -1;
    }
}
