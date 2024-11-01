using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftWayUI : MonoBehaviour
{ 
    private GameObject _craftWayItemImage;
    private GameObject _catalystItemUI;

    public CraftWayData CraftWayData
    {
        get => _craftWayData;
        set
        {
            _craftWayData = value;
            upDateDisplay();
        }
    }
    private CraftWayData _craftWayData;
    private List<GameObject> _itemList;

    private Transform _equalsIcon;
    private Transform _targetIcon;

    private void Awake()
    {
        _craftWayItemImage = Resources.Load<GameObject>("UI/CraftItemImage");
        _catalystItemUI = Resources.Load<GameObject>("UI/CatalystItemUI");
        _equalsIcon = transform.Find("EqualsTo");
        _targetIcon = transform.Find("Target");
    }

    private void upDateDisplay()
    {
        if (_craftWayData==null)
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
}
