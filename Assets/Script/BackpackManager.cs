using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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
    /// 向背包中添加物品
    /// </summary>
    /// <param name="item">要添加的物品</param>
    public void AddItem(Item item) 
    {
        _itemList.Add(item);
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
    public void RemoveItem(Item item)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        _itemList.Remove(item);
        RefreshSlots();
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
}
