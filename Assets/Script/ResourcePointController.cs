using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourcePointController : MonoBehaviour
{
    public GameObject ResourceUIPanelPrefab;
    public Tile ResourcePointTile; // ����Ϊ��Դ��
    private Tilemap _furnitureTilemap;
    private float _epsilon = 0.05f;
    private GameObject _player;
    private Dictionary<Vector3Int, GameObject> _resourcePointUIs = new Dictionary<Vector3Int, GameObject>(); // �洢ÿ����Դ���UI

    // Start is called before the first frame update
    void Start()
    {
        _furnitureTilemap = GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        _player = GameObject.FindWithTag("LocalPlayer");

        if (Input.GetKeyDown(KeyCode.Mouse1) && _player != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _furnitureTilemap.WorldToCell(mousePos);
            TileBase tile = _furnitureTilemap.GetTile(targetCellPos);

            if (tile == ResourcePointTile)
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
                    ToggleResourcePointUI(targetCellPos);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAllResourceUIs();
        }
    }

    private void ToggleResourcePointUI(Vector3Int resourcePointPosition)
    {
        if (_resourcePointUIs.ContainsKey(resourcePointPosition))
        {
            GameObject resourceUIPanel = _resourcePointUIs[resourcePointPosition];
            if (resourceUIPanel.activeSelf)
            {
                UIManager.Instance.RemoveActiveUI(resourceUIPanel);
                resourceUIPanel.SetActive(false);
            }
            else
            {
                UIManager.Instance.AddActiveUI(resourceUIPanel);
                resourceUIPanel.SetActive(true);
            }
        }
        else
        {
            // �����µ�UI��岢����������Դ��
            GameObject resourceUIPanel = Instantiate(ResourceUIPanelPrefab);
            resourceUIPanel.transform.SetParent(GameObject.Find("Canvas").transform, false); // ��UI�������Canvas��
            resourceUIPanel.transform.position = Camera.main.WorldToScreenPoint(_furnitureTilemap.CellToWorld(resourcePointPosition));

            UIManager.Instance.AddActiveUI(resourceUIPanel);

            // ��UI���洢���ֵ����Ա��ٴη���
            _resourcePointUIs.Add(resourcePointPosition, resourceUIPanel);
        }
    }


    public void HideAllResourceUIs()
    {
        foreach (var ui in _resourcePointUIs.Values)
        {
            ui.SetActive(false);
        }
    }
}
