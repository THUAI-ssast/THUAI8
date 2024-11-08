using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ResourcePointController : MonoBehaviour
{
    public GameObject ResourceUIPanel;
    private GameObject _resourceUIInstance; // 用于保存动态创建的UI实例
    private Tilemap _furnitureTilemap;
    private Vector3Int _resourcePointPosition;
    private float _epsilon = 0.05f;
    private GameObject _player;

    void Start()
    {
        _furnitureTilemap = GetComponent<Tilemap>();
    }

    void Update()
    {
        _player = GameObject.FindWithTag("LocalPlayer");
        if (Input.GetKeyDown(KeyCode.Mouse1) && _player != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _furnitureTilemap.WorldToCell(mousePos);
            TileBase tile = _furnitureTilemap.GetTile(targetCellPos);

            if (tile is ResourcePointTile resourceTile)
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
                    ShowResourceUI(resourceTile);
                }
            }
        }
    }

    private void ShowResourceUI(ResourcePointTile resourceTile)
    {
        // 如果UI实例已经存在，先销毁它
        if (_resourceUIInstance != null)
        {
            Destroy(_resourceUIInstance);
        }

        // 创建UI实例并初始化内容
        _resourceUIInstance = Instantiate(ResourceUIPanel, transform);
        // _resourceUIInstance.GetComponentInChildren<Text>().text = $"Resource: {resourceTile.resourceName}\nAmount: {resourceTile.resourceAmount}";

        // 设置UI的位置（可以是屏幕中心或鼠标附近）
        _resourceUIInstance.transform.position = Camera.main.WorldToScreenPoint(_furnitureTilemap.CellToWorld(_resourcePointPosition) + new Vector3(0.5f, 0.5f, 0));
    }
}
