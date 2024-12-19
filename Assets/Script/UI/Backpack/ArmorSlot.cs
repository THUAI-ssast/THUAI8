using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI显示类，展示单个护甲的槽位UI
/// </summary>
public class ArmorSlot : MonoBehaviour, IPointerClickHandler
{
    private Item _armorItem = null;

    private Image _displayImage;
    private TMP_Text _armorName;
    private TMP_Text _armorDurability;

    void Awake()
    {
        _displayImage = transform.Find("Image").GetComponent<Image>();
        _armorName = transform.Find("Name").GetComponent<TMP_Text>();
        _armorDurability = transform.Find("Durability").GetComponent<TMP_Text>();
        _menuPrefab = Resources.Load<GameObject>("UI/OperationMenuArmor");
        UpdateDisplay();
    }
    /// <summary>
    /// 取下原护甲，将新护甲放置在槽位上
    /// </summary>
    /// <param name="item">要展示的新护甲</param>
    /// <returns>原来展示的护甲</returns>
    public Item SetItem(Item item)
    {
        Item oldItem = _armorItem;
        _armorItem = item;
        UpdateDisplay();
        return oldItem;
    }
    /// <summary>
    /// 获得当前正在展示的护甲
    /// </summary>
    /// <returns>当前正在展示的护甲</returns>
    public Item GetItem()
    {
        return _armorItem;
    }
    /// <summary>
    /// 刷新护甲的展示
    /// </summary>
    public void UpdateDisplay()
    {
        if (_armorItem == null && _displayImage && _armorName && _armorDurability)
        {
            _displayImage.enabled = false;
            _armorName.text = "";
            _armorDurability.text = "";
        }
        else if (_armorItem.ItemData is ArmorItemData armorData)
        {
            _displayImage.enabled = true;
            _displayImage.sprite = armorData.ItemIcon;
            _armorName.text = armorData.ItemName;
            _armorDurability.text = $"{_armorItem.CurrentDurability}";
            Debug.Log(_armorItem == null);
            Debug.Log(_armorItem.CurrentDurability); // here
        }
    }

    private GameObject _menuPrefab;
    private GameObject _menuObject;

    /// <summary>
    /// 处理右键点击唤起卸下菜单
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // _battlePanel 为空
        if (UIManager.Instance.BattlePanel.activeSelf)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right && _armorItem != null &&
            _armorItem.ItemData is ArmorItemData armorData)
        {
            if (_menuObject != null)
            {
                Destroy(_menuObject);
                return;
            }

            if (UIManager.Instance.ExistingOperationMenu != null)
            {
                Destroy(UIManager.Instance.ExistingOperationMenu);
            }

            _menuObject = Instantiate(_menuPrefab, transform, false);
            _menuObject.transform.position = transform.position;
            UIManager.Instance.ExistingOperationMenu = _menuObject;

            var layout = _menuObject.transform.GetChild(0);
            layout.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject player = GameObject.FindWithTag("LocalPlayer");
                if (player == null)
                    return;
                player.GetComponent<PlayerHealth>().UnEquipArmor(armorData.EquipBodyPosition);
                Destroy(_menuObject);
            });
        }
    }
}