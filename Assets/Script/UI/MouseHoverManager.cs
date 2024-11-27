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
/// 用于供玩家在地图上通过鼠标悬停查看信息的管理类
/// </summary>
public class MouseHoverUI : MonoBehaviour
{
    // 所用Tilemaps
    private Tilemap _groundTilemap;
    private Tilemap _wallTilemap;
    private Tilemap _furnitureTilemap;
    private Tilemap _glassTilemap;
    private Tilemap _itemTilemap;

    // 用于类型判断的各种Tile
    public Tile DoorTileHorizontal;
    public Tile DoorTileVertical;
    public Tile BrokenGlassTile;

    /// <summary>
    /// 鼠标悬停显示的UI
    /// </summary>
    private GameObject _mouseHoverPanel;
    private Vector3 _panelBias;
    private TextMeshProUGUI _panelText;

    /// <summary>
    /// 记录上一次的tile坐标
    /// </summary>
    private Vector3Int _lastCellPosition;

    private Coroutine _showCoroutine;

    /// <summary>
    /// 更新悬停显示UI内容的时延
    /// </summary>
    private float _showDelay = 0.6f;

    /// <summary>
    /// 是否在悬停时显示UI的指示变量，由玩家控制
    /// </summary>
    private bool _isShowUI = false;

    /// <summary>
    /// 表示玩家切换悬停显示状态的CD，只有值为false时切换操作才有效
    /// </summary>
    private bool _isSwitchCD = false;

    /// <summary>
    /// 用于显示鼠标悬停是否打开的信息的协程
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

            // 隐藏旧的UI内容，显示新的UI内容
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = targetCellPos;

            // 鼠标悬停在可见范围外，则直接返回
            if (!GridMoveController.Instance.IsInBoundary(targetCellPos))
            {
                continue;
            }

            GridMoveController.Instance.JudgeReachable(targetCellPos);
            yield return new WaitForSeconds(_showDelay);

            bool isTileOccupied = false;
            string descriptionText = "";
            // 判断悬停在哪类tile上
            if (_wallTilemap.HasTile(targetCellPos))
            {
                isTileOccupied = true;
                descriptionText = IsDoorTile(targetCellPos) ? "门：<sprite=1>右键打开或关闭\n" : "此处无法到达\n";
            }
            else if (_furnitureTilemap.HasTile(targetCellPos))
            {
                isTileOccupied = true;
                descriptionText = "此处无法到达\n";
            }
            else if (_groundTilemap.HasTile(targetCellPos))
            {
                isTileOccupied = true;
                if (GridMoveController.Instance.IsMovable)
                {
                    bool isReachable = GridMoveController.Instance.IsReachable;
                    string requiredActionPoint = GridMoveController.Instance.RequiredActionPoint().ToString("0.0");
                    Debug.Log($"isReachable:{isReachable}");
                    descriptionText = isReachable ? $"<sprite=0>左键移动到这里 {requiredActionPoint}AP\n" : "此处无法到达\n";
                }
            }

            // 单独判断是否悬停在玻璃上
            if (_glassTilemap.HasTile(targetCellPos) && !IsGlassBroken(targetCellPos))
            {
                descriptionText += "玻璃：移动经过可撞破\n";
            }

            // 判断是否悬停在资源点上
            bool resourcePointExists = ResourcePointController.ResourcePointDictionary.ContainsKey(targetCellPos);
            if (resourcePointExists)
            {
                descriptionText += "资源点：靠近  <sprite=1>右键查看资源\n";
            }

            // 判断是否悬停在物品上
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
                            descriptionText += "靠近  <sprite=1>右键拾取物品\n";
                            isFirst = false;
                        }
                        descriptionText += $"物品：{item.collider.GetComponent<Item>().ItemData.ItemName}\n";
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
            displayText = "鼠标悬停显示已开";
        }
        else
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = Vector3Int.back;
            displayText = "鼠标悬停显示已关";
        }
        UIManager.Instance.DisplayHoverStatusPanel(displayText);
    }

}
