using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using UnityEngine.UIElements;
using Pathfinding;

public class BornUIManager : NetworkBehaviour
{
    public static BornUIManager Instance;

    public float delayTime;

    private Vector3Int _bornPos;

    //public Vector3Int BornPos
    //{
    //    get => _bornPos;
    //}

    private PlayerMove _playerMove;

    private GameObject _mapPanel;
    private GameObject _bigMapPanel;
    private GameObject _blockerPanel;
    public GameObject GridCellPrefab;
    public int Rows;
    public int Columns;

    private List<GameObject> _gridCells = new List<GameObject>();
    private GameObject _currentRedCell;

    public GameObject CurrentRedCell
    {
        get => _currentRedCell;
        set => _currentRedCell = value;
    }

    public List<GameObject> GridCells
    {
        get => _gridCells;
    }

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

    void Start()
    {
        _mapPanel = UIManager.Instance.MainCanvas.transform.Find("BornMapPanel").gameObject;
        _bigMapPanel = _mapPanel.transform.Find("BigMapImage").gameObject;
        _blockerPanel = UIManager.Instance.MainCanvas.transform.Find("BlockerPanel").gameObject;

        UIManager.Instance.ActiveUIList.Add(_bigMapPanel);

        GenerateGrid();

        StartCoroutine(CloseAfterDelay());
    }

    void Update()
    {
        if (_bigMapPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        Debug.Log("----------------");
        Debug.Log(GameObject.FindWithTag("LocalPlayer"));
        Debug.Log(GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerMove>());
        _playerMove = GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerMove>();
        if (_bigMapPanel != null)
        {
            _bigMapPanel.SetActive(false);
            _blockerPanel.SetActive(false);
            UIManager.Instance.ActiveUIList.Remove(_bigMapPanel);

            var tilePosition = GridMoveController.Instance.GroundTilemap.WorldToCell(_bornPos);
            _playerMove.transform.position = _bornPos;
            _playerMove.CmdSetPosition(_bornPos, tilePosition, null);
        }
    }

    private void GenerateGrid()
    {
        RectTransform bigMapRect = _bigMapPanel.GetComponent<RectTransform>();
        float cellWidth = bigMapRect.rect.width / Columns;
        float cellHeight = bigMapRect.rect.height / Rows;

        // 创建小方格
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                GameObject gridCell = Instantiate(GridCellPrefab, _bigMapPanel.transform);
                RectTransform cellRect = gridCell.GetComponent<RectTransform>();

                // 计算小方格的位置
                float xPos = j * cellWidth - bigMapRect.rect.width / 2 + cellWidth / 2;
                float yPos = i * cellHeight - bigMapRect.rect.height / 2 + cellHeight / 2;

                cellRect.localPosition = new Vector2(xPos, yPos);
                cellRect.sizeDelta = new Vector2(cellWidth, cellHeight); // 设置小方格的大小

                _gridCells.Add(gridCell);
            }
        }
    }

    private void HandleClick()
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _bigMapPanel.GetComponent<RectTransform>(), // 转换目标RectTransform
            Input.mousePosition, // 鼠标的屏幕位置
            null, // 摄像机，传 null 默认使用当前摄像机
            out localPos); // 输出到本地坐标

        RectTransform bigMapRect = _bigMapPanel.GetComponent<RectTransform>();

        // 计算点击的列和行
        float cellWidth = bigMapRect.rect.width / Columns;
        float cellHeight = bigMapRect.rect.height / Rows;

        int clickedColumn = Mathf.FloorToInt((localPos.x + bigMapRect.rect.width / 2) / cellWidth);
        int clickedRow = Mathf.FloorToInt((localPos.y + bigMapRect.rect.height / 2) / cellHeight);

        Vector2 cellCenterPos = new Vector2(
        (clickedColumn + 0.5f) * cellWidth - bigMapRect.rect.width / 2,
        (clickedRow + 0.5f) * cellHeight - bigMapRect.rect.height / 2
        );

        // 根据格子中心位置计算 tilePos
        _bornPos = MapUIManager.Instance.ImagePosToTilePos(cellCenterPos);

        // 输出计算的 tilePos
        Debug.Log($"Tile position for clicked cell center: {_bornPos}");

        int newIndex = clickedRow * Columns + clickedColumn;

        if (newIndex >= 0 && newIndex < _gridCells.Count)
        {
            int oldIndex = -1;

            if (_currentRedCell != null)
            {
                oldIndex = _gridCells.IndexOf(_currentRedCell);
            }

            GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerBorn>().CmdHandleCellClick(oldIndex, newIndex);
        }
    }
}
