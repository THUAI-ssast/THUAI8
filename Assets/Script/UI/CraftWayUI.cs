using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftWayUI : MonoBehaviour
{
    private static GameObject _craftWayItemImage;
    private static GameObject _catalystItemUI;
    private static Sprite _craftDefault;
    private static Sprite _craftSatisfied;
    /// <summary>
    /// 所有的CraftWayUI
    /// </summary>
    private static List<CraftWayUI> _uiList = new List<CraftWayUI>();
    public static void ClearItemList()
    {
        _uiList.Clear();
    }

    /// <summary>
    /// 当前选中的合成路线数据，点击应用合成时会被deploy
    /// </summary>
    public static CraftWayData SelectedCraftWay { get; private set; }

        /// <summary>
    /// 对于所有的CraftWayUI，更新其已满足的物品个数并据此从大到小排序
    /// </summary>
    public static void UpdateSatisfiedAll()
    {
        foreach (CraftWayUI craftWayUI in _uiList)
            craftWayUI.UpdateSatisfied();
        _uiList.Sort((u1, u2) => u1._satisfiedItemCount.CompareTo(u2._satisfiedItemCount));
        foreach (CraftWayUI craftWayUI in _uiList)
            craftWayUI.transform.SetAsFirstSibling();
    }

    public CraftWayData CraftWayData
    {
        get => _craftWayData;
        set
        {
            _craftWayData = value;
            updateDisplay();
        }
    }

    private CraftWayData _craftWayData;
    private List<GameObject> _itemList;


    private Image _backgroundImage;
    private Transform _equalsIcon;
    private Transform _targetIcon;
    /// <summary>
    /// 排序使用的属性，全部满足则为100+满足物品个数，否则为满足物品个数
    /// </summary>
    private int _satisfiedItemCount;

    private void Awake()
    {
        _uiList.Add(this);
        if (_craftWayItemImage == null || _catalystItemUI == null)
        {
            _craftWayItemImage = Resources.Load<GameObject>("UI/CraftItemImage");
            _catalystItemUI = Resources.Load<GameObject>("UI/CatalystItemUI");
            _craftDefault = Resources.Load<Sprite>("UI/Sprite/CraftDefault");
            _craftSatisfied = Resources.Load<Sprite>("UI/Sprite/CraftSatisfied");
        }

        _equalsIcon = transform.Find("EqualsTo");
        _targetIcon = transform.Find("Target");
        _backgroundImage = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(() => SelectedCraftWay = this.CraftWayData);
    }
    /// <summary>
    /// 更新此合成路径的所需物品Icon显示
    /// </summary>
    private void updateDisplay()
    {
        if (_craftWayData == null)
            return;
        foreach (ItemData costItem in _craftWayData.CostItems)
        {
            var obj = Instantiate(_craftWayItemImage, transform);
            obj.GetComponent<Image>().sprite = costItem.ItemIcon;
            obj.GetComponentInChildren<TMP_Text>().text = costItem.ItemName;
        }

        foreach (ItemData catalystItem in _craftWayData.CatalystItems)
        {
            var obj = Instantiate(_catalystItemUI, transform);
            obj.transform.GetChild(0).GetComponent<Image>().sprite = catalystItem.ItemIcon;
            obj.GetComponentInChildren<TMP_Text>().text = catalystItem.ItemName;
        }

        _equalsIcon.SetAsLastSibling();
        _targetIcon.GetChild(0).GetComponent<Image>().sprite = _craftWayData.ProductItem.ItemIcon;
        _targetIcon.GetChild(1).GetComponent<TMP_Text>().text = _craftWayData.ProductItem.ItemName;
        _targetIcon.SetAsLastSibling();
    }
    /// <summary>
    /// 更新此ui的满足状态，并据此更改显示状态（背景图）
    /// </summary>
    public void UpdateSatisfied()
    {
        _backgroundImage.sprite = (_satisfiedItemCount = BackpackManager.Instance.IsCraftSatisfied(_craftWayData)) >=100
            ? _craftSatisfied
            : _craftDefault;
    }

}