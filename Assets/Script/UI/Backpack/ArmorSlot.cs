using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmorSlot : MonoBehaviour, IPointerClickHandler
{
    private Item _armorItem = null;

    private Image _displayImage;
    private TMP_Text _armorName;
    private TMP_Text _armorDurability;

    void Start()
    {
        _displayImage = transform.Find("Image").GetComponent<Image>();
        _armorName = transform.Find("Name").GetComponent<TMP_Text>();
        _armorDurability = transform.Find("Durability").GetComponent<TMP_Text>();
        _menuPrefab = Resources.Load<GameObject>("UI/OperationMenuArmor");
        UpdateDisplay();
    }

    public Item SetItem(Item item)
    {
        Item oldItem = _armorItem;
        _armorItem = item;
        UpdateDisplay();
        return oldItem;
    }

    public Item GetItem()
    {
        return _armorItem;
    }

    public void UpdateDisplay()
    {
        if (_armorItem == null)
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
            _armorDurability.text = $"{_armorItem.CurrentDurability}/{armorData.Durability}";
        }
    }

    private GameObject _menuPrefab;
    private GameObject _menuObject;

    /// <summary>
    /// 监听鼠标右键点击事件。生成全局唯一菜单、加入菜单按钮点击事件。
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
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