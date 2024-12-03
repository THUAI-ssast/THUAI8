using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class BornUIManager : NetworkBehaviour
{
    public static BornUIManager Instance;

    private GameObject _mapPanel;
    private GameObject _bigMapPanel;
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

        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void GenerateGrid()
    {
        RectTransform bigMapRect = _bigMapPanel.GetComponent<RectTransform>();
        float cellWidth = bigMapRect.rect.width / Columns;
        float cellHeight = bigMapRect.rect.height / Rows;

        // ����С����
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                GameObject gridCell = Instantiate(GridCellPrefab, _bigMapPanel.transform);
                RectTransform cellRect = gridCell.GetComponent<RectTransform>();

                // ����С�����λ��
                float xPos = j * cellWidth - bigMapRect.rect.width / 2 + cellWidth / 2;
                float yPos = i * cellHeight - bigMapRect.rect.height / 2 + cellHeight / 2;

                cellRect.localPosition = new Vector2(xPos, yPos);
                cellRect.sizeDelta = new Vector2(cellWidth, cellHeight); // ����С����Ĵ�С

                _gridCells.Add(gridCell);
            }
        }
    }

    private void HandleClick()
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _bigMapPanel.GetComponent<RectTransform>(), // ת��Ŀ��RectTransform
            Input.mousePosition, // ������Ļλ��
            null, // ��������� null Ĭ��ʹ�õ�ǰ�����
            out localPos); // �������������

        RectTransform bigMapRect = _bigMapPanel.GetComponent<RectTransform>();

        // ���������к���
        float cellWidth = bigMapRect.rect.width / Columns;
        float cellHeight = bigMapRect.rect.height / Rows;

        int clickedColumn = Mathf.FloorToInt((localPos.x + bigMapRect.rect.width / 2) / cellWidth);
        int clickedRow = Mathf.FloorToInt((localPos.y + bigMapRect.rect.height / 2) / cellHeight);

        Vector2 cellCenterPos = new Vector2(
        (clickedColumn + 0.5f) * cellWidth - bigMapRect.rect.width / 2,
        (clickedRow + 0.5f) * cellHeight - bigMapRect.rect.height / 2
        );

        // ���ݸ�������λ�ü��� tilePos
        Vector3Int tilePos = MapUIManager.Instance.ImagePosToTilePos(cellCenterPos);

        // �������� tilePos
        Debug.Log($"Tile position for clicked cell center: {tilePos}");

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
