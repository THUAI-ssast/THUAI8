using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourcePointController : MonoBehaviour
{
    public GameObject ResourceUIPanelPrefab;

    private GameObject _resourceUIPanelInstance;
    private Tilemap _furnitureTilemap;
    private GameObject _player;
    private List<Item> _itemList;
    private Dictionary<string, float> _itemProbabilityDictionary;
    private float _epsilon = 0.05f;

    void Start()
    {
        _furnitureTilemap = transform.parent.GetComponent<Tilemap>();
        _itemList = new List<Item>();
        InitializeItemProbability();
        StartCoroutine(initItems_debug());
        Debug.Log(_itemList.Count);
    }

    void Update()
    {
        Debug.Log(_itemList.Count);
        _player = GameObject.FindWithTag("LocalPlayer");

        if (Input.GetKeyDown(KeyCode.Mouse1) && _player != null)
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
        if (GameObject.FindWithTag("LocalPlayer") != null)
        {
            CreateItem("ScriptableObject/Items/Ä¾°å");
            CreateItem("ScriptableObject/Items/´¸Ê¯");
            CreateItem("ScriptableObject/Items/Ä¾°ô");
            CreateItem("ScriptableObject/Items/½ðÊôÆÆÆ¬");
        }
    }

    private void CreateItem(string itemdata_pth)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        ItemOwner owner = ItemOwner.World;
        Vector3 position = Vector3.zero;
        Item.Create(itemdata_pth, owner, player, gameObject);
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

    private void InitializeItemProbability()
    {
        _itemProbabilityDictionary = new Dictionary<string, float>();

        _itemProbabilityDictionary.Add("ScriptableObject/Items/µ¶Æ¬", 0.2f);
        _itemProbabilityDictionary.Add("ScriptableObject/Items/Ä¾°å", 0.5f);
        _itemProbabilityDictionary.Add("ScriptableObject/Items/½ðÊôÆÆÆ¬", 0.3f);
    }

    public void AddItemToResourcePoint(Item item)
    {
        if (!_itemList.Contains(item))
        {
            _itemList.Add(item);
        }
    }
}
