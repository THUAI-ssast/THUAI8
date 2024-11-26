using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ResourcePointController : NetworkBehaviour
{
    [SerializeField] private SerializableDictionary.Scripts.SerializableDictionary<ItemData, float> _serializedProbilityDictionary;
    private GameObject _resourceUIPanelInstance;
    private Tilemap _furnitureTilemap;
    private GameObject _player;
    private readonly List<Item> _itemList = new List<Item>();
    private bool _useCustomInitItems = false;
    private List<Item> _customInitItems;

    private float _requiredActionPoint = 2;
    public float RequiredActionPoint { get => _requiredActionPoint; }

    void Start()
    {
        _resourceUIPanelInstance = transform.GetChild(0).GetChild(0).gameObject;
        _resourceUIPanelInstance.SetActive(false);
        _furnitureTilemap = transform.parent.GetComponent<Tilemap>();
        if (isServer)
        {
            if (_useCustomInitItems && _customInitItems != null)
            {
                StartCoroutine(AssignInitItems(_customInitItems));
            }
            else
            {
                StartCoroutine(RandomInitItems());
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && _player != null && !UIManager.Instance.IsUIActivating)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int playerPos = _player.GetComponent<PlayerMove>().TilePosition;
            Vector3Int targetCellPos = _furnitureTilemap.WorldToCell(mousePos);
            Vector3 realPosition = _furnitureTilemap.CellToWorld(targetCellPos);
            bool isClickingThis = realPosition + new Vector3(0.5f, 0.5f, 0) == transform.position;

            if (isClickingThis)
            {
                bool isAdjacentInX = Mathf.Abs(targetCellPos.x - playerPos.x) == 1 && targetCellPos.y == playerPos.y;
                bool isAdjacentInY = Mathf.Abs(targetCellPos.y - playerPos.y) == 1 && targetCellPos.x == playerPos.x;

                if (isAdjacentInX || isAdjacentInY)
                {
                    ToggleResourcePointUI();
                }
            }
        }
        else if (isClient)
        {
            _player = GameObject.FindWithTag("LocalPlayer");
        }
    }

    private IEnumerator RandomInitItems()
    {
        yield return new WaitForSeconds(1);
        foreach (var match in _serializedProbilityDictionary.Dictionary)
        {
            float x = Random.Range(0f, 1f);
            if (x <= match.Value)
            {
                string directory = "ScriptableObject/Items/";
                if (match.Key is WeaponItemData)
                {
                    directory += "Weapons/";
                }
                else if (match.Key is ArmorItemData)
                {
                    directory += "Armor/";
                }
                else if (match.Key is MedicineItemData)
                {
                    directory += "Medicines/";
                }
                CreateItem(directory + match.Key.name);
            }
        }
    }

    public IEnumerator AssignInitItems(List<Item> items)
    {
        yield return new WaitForSeconds(1);
        foreach (var item in items)
        {
            string directory = "ScriptableObject/Items/";
            if (item.ItemData is WeaponItemData)
            {
                directory += "Weapons/";
            }
            else if (item.ItemData is ArmorItemData)
            {
                directory += "Armor/";
            }
            else if (item.ItemData is MedicineItemData)
            {
                directory += "Medicines/";
            }
            CreateItem(directory + item.ItemData.ItemName);
        }
    }

    private void CreateItem(string itemdata_pth)
    {
        ItemOwner owner = ItemOwner.World;
        Item.Create(itemdata_pth, owner, null, gameObject);
    }

    private void ToggleResourcePointUI()
    {
        if (_resourceUIPanelInstance.activeSelf)
        {
            UIManager.Instance.RemoveActiveUI(_resourceUIPanelInstance);
            _resourceUIPanelInstance.SetActive(false);
        }
        else
        {
            UIManager.Instance.AddActiveUI(_resourceUIPanelInstance);
            _resourceUIPanelInstance.SetActive(true);
        }
    }

    /// <summary>
    /// ���������ã���ͬ����Ʒ�б��������Ʒ
    /// </summary>
    /// <param name="item"></param>
    public void AddItemToResourcePoint(Item item)
    {
        if (!_itemList.Contains(item))
        {
            _itemList.Add(item);
            RefreshSlots();
        }
    }
    /// <summary>
    /// ���������ã�ɾ��ͬ����Ʒ�б��е���Ʒ
    /// </summary>
    /// <param name="item"></param>
    [ClientRpc]
    public void RemoveItemFromResourcePoint(Item item)
    {
        if (_itemList.Contains(item))
        {
            _itemList.Remove(item);
            RefreshSlots();
        }
    }

    private void RefreshSlots()
    {
        Transform slots = gameObject.transform.Find("Canvas/ResourcePointPanel/Scroll View/Viewport/Slots");
        for (int i = 0; i < slots.childCount; i++)
        {
            if (i < _itemList.Count)
            {
                var image = slots.GetChild(i).GetChild(0).GetComponent<Image>();
                image.enabled = true;
                image.sprite = _itemList[i].ItemData.ItemIcon;
                slots.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = _itemList[i].ItemData.ItemName;
                slots.GetChild(i).GetComponent<RPSlot>().SetItem(_itemList[i]);
            }
            else
            {
                slots.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
                slots.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    public void InitializeWithCustomItems(List<Item> customItems)
    {
        _useCustomInitItems = true;
        _customInitItems = customItems;
    }
}
