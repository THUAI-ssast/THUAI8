using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 背包管理类。管理背包中的一切行为，包括对物品丢弃、使用、添加等。
/// </summary>
public class BackpackManager : MonoBehaviour
{
    /// <summary>
    /// 定位背包UI
    /// </summary>
    [SerializeField]private GameObject _bagPanel;
    /// <summary>
    /// 单例模式
    /// </summary>
    public static BackpackManager Instance;
    /// <summary>
    /// 背包中现有所有物品的列表，增删需更改列表。
    /// </summary>
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
    /// <summary>
    /// 初始化背包
    /// </summary>
    void Start()
    {
        RefreshSlots();
    }
    /// <summary>
    /// 创建物品并添加到背包中
    /// </summary>
    /// <param name="itemdata_pth">Resources中要创建的物品的信息的路径</param>
    private void CreateItem(string itemdata_pth)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        ItemOwner owner = ItemOwner.PlayerBackpack;
        Vector3 position = Vector3.zero;
        Item.Create(itemdata_pth, owner, player);
    }
    /// <summary>
    /// 销毁物品
    /// </summary>
    /// <param name="item">要销毁的物品</param>
    private void DestroyItem(Item item)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        RemoveItem(item);
        Item.Destroy(item, player);
    }
    /// <summary>
    /// 向背包中添加物品
    /// </summary>
    /// <param name="item">要添加的物品</param>
    public void AddItem(Item item) 
    {
        // 从背包中添加物品
        _itemList.Add(item);
        RefreshSlots();
    }
    /// <summary>
    /// 从背包中移除物品
    /// </summary>
    /// <param name="item">要移除的物品</param>
    private void RemoveItem(Item item)
    {
        _itemList.Remove(item);
        RefreshSlots();
    }
    /// <summary>
    /// 使用背包中的物品
    /// </summary>
    /// <param name="item">要使用的物品</param>
    public void UseItem(Item item)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        player.GetComponent<PlayerItemInteraction>().UseItem(item.gameObject);
        _itemList.Remove(item);
        RefreshSlots();
    }
    /// <summary>
    /// 丢弃背包中的物品到世界
    /// </summary>
    /// <param name="item">要丢弃的物品</param>
    public void DropItem(Item item)
    {
        RemoveItem(item);
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        player.GetComponent<PlayerItemInteraction>().DropItem(item.gameObject);
    }
    /// <summary>
    /// 刷新背包中的物品槽，更新物品显示状态，包括图标、名称等。
    /// </summary>
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
