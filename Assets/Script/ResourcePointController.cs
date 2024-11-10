using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ResourcePointController : NetworkBehaviour
{
    [SerializeField]private SerializableDictionary.Scripts.SerializableDictionary<ItemData, float> _serializedProbilityDictionary;
    public GameObject ResourceUIPanelPrefab;
    private GameObject _resourceUIPanelInstance;
    private Tilemap _furnitureTilemap;
    private GameObject _player;
    private readonly List<Item> _itemList = new List<Item>();
    private float _epsilon = 0.05f;

    private float _requiredActionPoint = 2;
    public float RequiredActionPoint { get => _requiredActionPoint; }

    void Start()
    {
        _resourceUIPanelInstance = transform.GetChild(0).GetChild(0).gameObject;
        _resourceUIPanelInstance.SetActive(false);
        _furnitureTilemap = transform.parent.GetComponent<Tilemap>();
        if(isServer)
        {
            StartCoroutine(initItems_debug());
        }
    }

    void Update()
    {
        _player = GameObject.FindWithTag("LocalPlayer");

        if (Input.GetKeyDown(KeyCode.Mouse1) && _player != null && !UIManager.Instance.IsUIActivating)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _furnitureTilemap.WorldToCell(mousePos);
            Vector3 realPosition = _furnitureTilemap.CellToWorld(targetCellPos);
            bool isClickingThis = realPosition + new Vector3(0.5f, 0.5f, 0) == transform.position;

            if (isClickingThis)
            {
                Vector3 playerPosBias = _player.transform.position;
                playerPosBias.x -= 0.5f;
                playerPosBias.y -= 0.5f;

                bool isAdjacentInX = Mathf.Abs(targetCellPos.x - playerPosBias.x) <= 1.05f &&
                                Mathf.Abs(targetCellPos.y - playerPosBias.y) <= _epsilon;

                bool isAdjacentInY = Mathf.Abs(targetCellPos.y - playerPosBias.y) <= 1.05f &&
                                    Mathf.Abs(targetCellPos.x - playerPosBias.x) <= _epsilon;

                if (isAdjacentInX || isAdjacentInY)
                {
                    ToggleResourcePointUI();
                }
            }
        }
    }

    private IEnumerator initItems_debug()
    {
        yield return new WaitForSeconds(1);
        foreach (var match in _serializedProbilityDictionary.Dictionary)
        {
            float x = Random.Range(0f, 1f);
            if (x <= match.Value)
            {
                CreateItem("ScriptableObject/Items/" + match.Key.name);
            }
        }
    }

    private void CreateItem(string itemdata_pth)
    {
        ItemOwner owner = ItemOwner.World;
        Vector3 position = Vector3.zero;
        Item.Create(itemdata_pth, owner, null, gameObject);
    }

    private void ToggleResourcePointUI()
    {
        if (_resourceUIPanelInstance == null)
        {
            _resourceUIPanelInstance = Instantiate(ResourceUIPanelPrefab);
            _resourceUIPanelInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
            _resourceUIPanelInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        }

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
    /// 服务器调用，向同步物品列表中添加物品
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
    /// 服务器调用，删除同步物品列表中的物品
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
}
