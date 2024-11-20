using DG.Tweening;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ���ڹ�����ڵ�ͼ��ͨ�������ͣ�鿴��Ϣ�Ĺ�����
/// </summary>
public class MouseHoverUI : MonoBehaviour
{
    // ����Tilemaps
    private Tilemap _groundTilemap;
    private Tilemap _wallTilemap;
    private Tilemap _furnitureTilemap;
    private Tilemap _glassTilemap;
    private Tilemap _itemTilemap;

    // ���������жϵĸ���Tile
    public Tile DoorTileHorizontal;
    public Tile DoorTileVertical;
    public Tile BrokenGlassTile;

    /// <summary>
    /// �����ͣ��ʾ��UI
    /// </summary>
    private GameObject _mouseHoverPanel;
    private Vector3 _panelBias;
    private TextMeshProUGUI _panelText;

    /// <summary>
    /// ��¼��һ�ε�tile����
    /// </summary>
    private Vector3Int _lastCellPosition;

    private Coroutine _showCoroutine;

    /// <summary>
    /// ������ͣ��ʾUI���ݵ�ʱ��
    /// </summary>
    private float _showDelay = 0.6f;

    /// <summary>
    /// �Ƿ�����ͣʱ��ʾUI��ָʾ����������ҿ���
    /// </summary>
    private bool _isShowUI = false;

    /// <summary>
    /// ��ʾ����л���ͣ��ʾ״̬��CD��ֻ��ֵΪfalseʱ�л���������Ч
    /// </summary>
    private bool _isSwitchCD = false;

    /// <summary>
    /// ������ʾ�����ͣ�Ƿ�򿪵���Ϣ��Э��
    /// </summary>
    private Coroutine _displayCoroutine;

    private PlayerMove _player;

    private void Start()
    {
        _groundTilemap = transform.GetChild(0).Find("GroundTilemap").GetComponent<Tilemap>();
        _wallTilemap = transform.GetChild(0).Find("WallTilemap").GetComponent<Tilemap>();
        _furnitureTilemap = transform.GetChild(0).Find("FurnitureTilemap").GetComponent<Tilemap>();
        _glassTilemap = transform.GetChild(0).Find("GlassTilemap").GetComponent<Tilemap>();
        _itemTilemap = transform.GetChild(0).Find("ItemTilemap").GetComponent<Tilemap>();

        _mouseHoverPanel = UIManager.Instance.MainCanvas.transform.Find("MouseHoverPanel").gameObject;
        _panelBias = new Vector3(120,-60);
        _panelText = _mouseHoverPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        if(_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
        }    
    }

    private void Update()
    {
        if (UIManager.Instance.GetActiveUINumber == 0)
        {
            if (Input.GetKeyDown(KeyCode.C) && !_isSwitchCD)
            {
                _isSwitchCD = true;
                ReverseShowUIStatus();
                _isSwitchCD = false;
            }
        }
        else
        {
            if (_isShowUI)
            {
                ReverseShowUIStatus();
            }
        }
    }

    private IEnumerator ShowUI()
    {
        while (true)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _groundTilemap.WorldToCell(mousePos);
            if (targetCellPos == _lastCellPosition)
            {
                yield return new WaitForSeconds(0.05f);
                _mouseHoverPanel.GetComponent<RectTransform>().position = Input.mousePosition + _panelBias;
                continue;
            }

            // ���ؾɵ�UI���ݣ���ʾ�µ�UI����
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = targetCellPos;

            // �����ͣ�ڿɼ���Χ�⣬��ֱ�ӷ���
            if (!GridMoveController.Instance.IsInBoundary(targetCellPos))
            {
                continue;
            }

            GridMoveController.Instance.JudgeReachable(targetCellPos);
            yield return new WaitForSeconds(_showDelay);

            bool isTileOccupied = false;
            string descriptionText = "";
            // �ж���ͣ������tile��
            if (_wallTilemap.HasTile(targetCellPos))
            {
                isTileOccupied = true;
                descriptionText = IsDoorTile(targetCellPos) ? "�ţ�<sprite=1>�Ҽ��򿪻�ر�\n" : "�˴��޷�����\n";
            }
            else if (_furnitureTilemap.HasTile(targetCellPos))
            {
                isTileOccupied = true;
                descriptionText = "�˴��޷�����\n";
            }
            else if (_groundTilemap.HasTile(targetCellPos))
            {
                isTileOccupied = true;
                if (GridMoveController.Instance.IsMovable)
                {
                    bool isReachable = GridMoveController.Instance.IsReachable;
                    string requiredActionPoint = GridMoveController.Instance.RequiredActionPoint().ToString("0.0");
                    Debug.Log($"isReachable:{isReachable}");
                    descriptionText = isReachable ? $"<sprite=0>����ƶ������� {requiredActionPoint}AP\n" : "�˴��޷�����\n";
                }
            }

            // �����ж��Ƿ���ͣ�ڲ�����
            if (_glassTilemap.HasTile(targetCellPos) && !IsGlassBroken(targetCellPos))
            {
                descriptionText += "�������ƶ�������ײ��\n";
            }

            // �ж��Ƿ���ͣ����Դ����
            bool resourcePointExists = ResourcePointController.ResourcePointDictionary.ContainsKey(targetCellPos);
            if (resourcePointExists)
            {
                descriptionText += "��Դ�㣺����  <sprite=1>�Ҽ��鿴��Դ\n";
            }

            // �ж��Ƿ���ͣ����Ʒ��
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            bool isFirst = true;
            if (hits.Length > 0)
            {
                foreach (var item in hits)
                {
                    if (item.collider.GetComponent<Item>() != null)
                    {
                        if (isFirst)
                        {
                            descriptionText += "����  <sprite=1>�Ҽ�ʰȡ��Ʒ\n";
                            isFirst = false;
                        }
                        descriptionText += $"��Ʒ��{item.collider.GetComponent<Item>().ItemData.ItemName}\n";
                    }
                }
            }

            if (isTileOccupied && descriptionText != "")
            {
                
                _mouseHoverPanel.GetComponent<RectTransform>().position = Input.mousePosition + _panelBias;
                _mouseHoverPanel.SetActive(true);
            }
            _panelText.text = descriptionText;
        }
    }

    private bool IsDoorTile(Vector3Int position)
    {
        TileBase tile = _wallTilemap.GetTile(position);
        return tile == DoorTileHorizontal || tile == DoorTileVertical;
    }

    private bool IsGlassBroken(Vector3Int position)
    {
        TileBase tile = _glassTilemap.GetTile(position);
        return tile == BrokenGlassTile;
    }

    private bool IsResourcePoint(Vector3Int position)
    {
        Vector3 realPosition = _furnitureTilemap.CellToWorld(position);
        foreach(var rpTransform in _furnitureTilemap.transform.GetComponentsInChildren<Transform>())
        {
            if(realPosition + new Vector3(0.5f, 0.5f, 0) == rpTransform.position)
                return true;
        }
        return false;
    }

    private void ReverseShowUIStatus()
    {
        string displayText;
        _isShowUI = !_isShowUI;
        if (_isShowUI)
        {
            _showCoroutine = StartCoroutine(ShowUI());
            displayText = "�����ͣ��ʾ�ѿ�";
        }
        else
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = Vector3Int.back;
            displayText = "�����ͣ��ʾ�ѹ�";
        }
        UIManager.Instance.DisplayHoverStatusPanel(displayText);
    }

}
