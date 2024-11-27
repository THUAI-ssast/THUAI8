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
/// 供玩家在地图上通过鼠标悬停查看信息的管理类
/// </summary>
public class MouseHoverManager : MonoBehaviour
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
    /// <summary>
    /// 悬停显示UI相对于鼠标位置的偏置
    /// </summary>
    private Vector3 _panelBias;
    /// <summary>
    /// 悬停显示UI下的文本UI
    /// </summary>
    private TextMeshProUGUI _panelText;

    /// <summary>
    /// 记录上一次鼠标悬停位置的tile坐标
    /// </summary>
    private Vector3Int _lastCellPosition;

    /// <summary>
    /// 鼠标悬停显示的协程
    /// </summary>
    private Coroutine _showCoroutine;

    /// <summary>
    /// 从玩家将鼠标放在tile上到悬停显示UI内容的时延
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

    /// <summary>
    /// 记录当玩家打开其他阻塞式UI之前鼠标悬停显示的状态，用于在玩家关闭所有阻塞式UI后恢复之前的设置
    /// </summary>
    private bool _isShowUIBefore;


    private void Start()
    {
        // 获取所有的tilemap
        _groundTilemap = transform.GetChild(0).Find("GroundTilemap").GetComponent<Tilemap>();
        _wallTilemap = transform.GetChild(0).Find("WallTilemap").GetComponent<Tilemap>();
        _furnitureTilemap = transform.GetChild(0).Find("FurnitureTilemap").GetComponent<Tilemap>();
        _glassTilemap = transform.GetChild(0).Find("GlassTilemap").GetComponent<Tilemap>();
        _itemTilemap = transform.GetChild(0).Find("ItemTilemap").GetComponent<Tilemap>();

        // 初始化悬停显示UI相关的对象
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
        if (UIManager.Instance.GetActiveUINumber == 0)  // 当存在其他阻塞式UI时，禁止鼠标悬停功能（例如背包UI、资源点UI、战斗界面UI等）
        {   
            if (Input.GetKeyDown(KeyCode.C) && !_isSwitchCD)    // 键盘按C切换是否启用鼠标悬停功能的状态
            {
                _isSwitchCD = true;
                ReverseShowUIStatus();
                _isSwitchCD = false;
            }
            if (_isShowUIBefore)
            {
                _isShowUIBefore = false;
                _isSwitchCD = true;
                ReverseShowUIStatus(false);
                _isSwitchCD = false;
            }
        }
        else
        {
            // 当其他阻塞式UI被打开时，关闭鼠标悬停功能
            if (_isShowUI)
            {
                _isShowUIBefore = true;
                ReverseShowUIStatus(false);
            }
        }
    }

    /// <summary>
    /// 持续进行悬停显示的协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowUI()
    {
        while (true)
        {
            // 获取鼠标位置和对应的cellPosition
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _groundTilemap.WorldToCell(mousePos);

            if (targetCellPos == _lastCellPosition) // 如果鼠标位置所在的tile与上次检测时相同，则直接使展示的内容跟随鼠标位置移动，而不再继续后续判断
            {
                yield return new WaitForSeconds(0.05f); // 跟随鼠标的间隔为0.05s
                _mouseHoverPanel.GetComponent<RectTransform>().position = Input.mousePosition + _panelBias;
                continue;
            }

            // 如果鼠标位置所在的tile与上次检测时不同，则先隐藏旧的UI，后续再进行更新
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = targetCellPos;

            // 鼠标悬停在可见范围外，则直接返回
            if (!GridMoveController.Instance.IsInBoundary(targetCellPos))
            {
                continue;
            }

            // 从玩家将鼠标放在tile上到悬停显示UI内容是存在时延的，正好利用该时延进行寻路判断
            GridMoveController.Instance.JudgeReachable(targetCellPos);
            yield return new WaitForSeconds(_showDelay);

            bool isTileOccupied = false;
            string descriptionText = "";
            // 判断悬停在哪类tile上
            if (_wallTilemap.HasTile(targetCellPos))    // WallTilemap（包括墙、门和其他类型的障碍物）
            {
                isTileOccupied = true;
                descriptionText = IsDoorTile(targetCellPos) ? "门：靠近  <sprite=1>右键打开或关闭\n" : "此处无法到达\n"; // TextMeshPro的富文本功能，可以插入图片
            }
            else if (_furnitureTilemap.HasTile(targetCellPos))  // FurnitureTilemap
            {
                isTileOccupied = true;
                descriptionText = "此处无法到达\n";
            }
            else if (_groundTilemap.HasTile(targetCellPos)) // GroundTilemap，该类型的tile在地图范围内的每一格都存在
            {
                isTileOccupied = true;
                if (GridMoveController.Instance.IsMovable)  // 玩家移动时禁用悬停显示
                {
                    // 获取之前计算好的寻路结果
                    bool isReachable = GridMoveController.Instance.IsReachable;
                    string requiredActionPoint = GridMoveController.Instance.RequiredActionPoint().ToString("0.0");
                    descriptionText = isReachable ? $"<sprite=0>左键移动到这里 {requiredActionPoint}AP\n" : "此处无法到达\n";
                }
            }

            // 单独判断是否悬停在玻璃上
            if (_glassTilemap.HasTile(targetCellPos) && !IsGlassBroken(targetCellPos))  // GlassTilemap，忽略破碎的玻璃
            {
                descriptionText += "玻璃：移动经过可撞破\n";
            }

            // 判断是否悬停在资源点上
            bool resourcePointExists = ResourcePointController.ResourcePointDictionary.ContainsKey(targetCellPos);
            if (resourcePointExists)
            {
                descriptionText += "资源点：靠近  <sprite=1>右键查看资源\n";
            }

            // 判断是否悬停在物品上，使用射线检测方法
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            bool isFirst = true;
            if (hits.Length > 0)
            {
                foreach (var item in hits)
                {
                    if (item.collider.GetComponent<Item>() != null) // 要求碰撞体必须是Item类
                    {
                        if (isFirst)    // 在显示物品列表前提示物品的拾取信息
                        {
                            descriptionText += "靠近  <sprite=1>右键拾取物品\n";
                            isFirst = false;
                        }
                        descriptionText += $"物品：{item.collider.GetComponent<Item>().ItemData.ItemName}\n";
                    }
                }
            }

            // 更新悬停显示UI的文本内容、位置（跟随鼠标）、可见状态
            _panelText.text = descriptionText;
            if (isTileOccupied && descriptionText != "")
            {
                _mouseHoverPanel.GetComponent<RectTransform>().position = Input.mousePosition + _panelBias;
                _mouseHoverPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 判断tile是否为门
    /// </summary>
    /// <param name="position">需要进行判断的tilePosition</param>
    /// <returns>返回true则表示判断的tile是门</returns>
    private bool IsDoorTile(Vector3Int position)
    {
        TileBase tile = _wallTilemap.GetTile(position);
        return tile == DoorTileHorizontal || tile == DoorTileVertical;
    }

    /// <summary>
    /// 判断tile是否为破碎的玻璃
    /// </summary>
    /// <param name="position">需要进行判断的tilePosition</param>
    /// <returns>返回true则表示判断的tile是破碎的玻璃</returns>
    private bool IsGlassBroken(Vector3Int position)
    {
        TileBase tile = _glassTilemap.GetTile(position);
        return tile == BrokenGlassTile;
    }

    /// <summary>
    /// 切换是否启用鼠标悬停功能的状态
    /// </summary>
    private void ReverseShowUIStatus(bool isDisplayText=true)
    {
        string displayText;
        _isShowUI = !_isShowUI;
        if (_isShowUI)  // 启用鼠标悬停功能
        {
            _showCoroutine = StartCoroutine(ShowUI());
            displayText = "鼠标悬停显示已开";
        }
        else  // 关闭鼠标悬停功能
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = Vector3Int.back;
            displayText = "鼠标悬停显示已关";
        }
        // 显示UI以提示玩家当前鼠标悬停功能是否启用
        if(isDisplayText)
        {
            UIManager.Instance.DisplayHoverStatusPanel(displayText);
        }
    }

}
