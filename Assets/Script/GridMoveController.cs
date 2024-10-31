using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Pathfinding;
using Unity.VisualScripting;

public class GridMoveController : MonoBehaviour
{
    public static GridMoveController Instance;
    public PlayerMove Player;

    public Tile brokenGlassTile;

    public Tile doorTileHorizontal;
    public Tile doorTileVertical;

    // A* 寻路相关
    private Seeker _pathSeeker;
    private Transform _pathTarget;
    private AstarPath _pathBaker;

    // 所用Tilemaps
    private Tilemap _groundTilemap;
    private Tilemap _wallTilemap;
    private Tilemap _furnitureTilemap;
    private Tilemap _glassTilemap;
    private Tilemap _stuffTilemap;

    private ShadowCaster2DCreator _shadowCreator;
    private TilemapCollider2D _wallCollider2D;

    private Transform _gridLine;

    private Vector3Int _targetCellPosition;
    private bool _isMovable = true;

    private Vector3 _cellBias;

    private Vector3Int? _lastAdjacentDoor; // 相邻门的位置

    private void Start()
    {
        Instance = this;
        _groundTilemap = transform.GetChild(0).Find("GroundTilemap").GetComponent<Tilemap>();
        _wallTilemap = transform.GetChild(0).Find("WallTilemap").GetComponent<Tilemap>();
        _furnitureTilemap = transform.GetChild(0).Find("FurnitureTilemap").GetComponent<Tilemap>();
        _glassTilemap = transform.GetChild(0).Find("GlassTilemap").GetComponent<Tilemap>();
        _stuffTilemap = transform.GetChild(0).Find("ItemTilemap").GetComponent<Tilemap>();

        _shadowCreator = _wallTilemap.GetComponent<ShadowCaster2DCreator>();
        _wallCollider2D = _wallTilemap.GetComponent<TilemapCollider2D>();

        _gridLine = transform.Find("GridLine");

        _cellBias = _groundTilemap.cellSize * 0.5f;
    }

    private void Update()
    {
        if (!UIManager.Instance.IsUIActivating&&_isMovable&&Player!=null)
            tryMove();

        // 鼠标右键点击门
        if (Input.GetKeyDown(KeyCode.Mouse1) && _lastAdjacentDoor.HasValue)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int doorPosition = _lastAdjacentDoor.Value;

            // 点击的位置在门的Tile上
            if (doorPosition == _wallTilemap.WorldToCell(mousePos))
            {
                Player.CmdRotateDoor(doorPosition);
            }
        }
    }

    /// <summary>
    /// 将player初始化为本机player，绑定移动、寻路等组件。
    /// </summary>
    /// <param name="player">本机player</param>
    public void InitLocalPlayer(PlayerMove player)
    {
        Player = player;
        var position = Player.transform.position;
        Player.SetPosition(position, _groundTilemap.WorldToCell(position));
        var finderObj = Instantiate(Resources.Load<GameObject>(("PathFinder")), Player.transform);
        _pathBaker = finderObj.GetComponent<AstarPath>();
        _pathSeeker = finderObj.GetComponent<Seeker>();
        _pathTarget = finderObj.transform.GetChild(0);
        updateGraph();
        _gridLine.position = position + new Vector3(0.5f, 0.5f);
    }

    /// <summary>
    /// 捕获玩家输入并尝试移动，点击角色范围内7*7Tile或按下wasd移动，有CD
    /// </summary>
    private void tryMove()
    {
        _targetCellPosition = Vector3Int.back;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _groundTilemap.WorldToCell(mousePos);
            if (Math.Abs(targetCellPos.x - Player.TilePosition.x) <= 3 &&
                Math.Abs(targetCellPos.y - Player.TilePosition.y) <= 3)
                _targetCellPosition = targetCellPos;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _targetCellPosition = Player.TilePosition + Vector3Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _targetCellPosition = Player.TilePosition + Vector3Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            _targetCellPosition = Player.TilePosition + Vector3Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _targetCellPosition = Player.TilePosition + Vector3Int.down;
        }

        if (_targetCellPosition != Vector3Int.back)
        {
            var targetWorldPosition = _groundTilemap.CellToWorld(_targetCellPosition) + _cellBias;
            _pathTarget.position = targetWorldPosition - Player.transform.position;
            _pathSeeker.StartPath(_groundTilemap.CellToWorld(Player.TilePosition) + _cellBias, targetWorldPosition, onPathComplete);
        }
    }

    /// <summary>
    /// 检查path的每两个相邻节点之间是否有门和前进方向垂直阻挡前进。
    /// </summary>
    private bool CheckForDoorBlock(Vector3[] path)
    {
        if (IsDoorTile(_wallTilemap.WorldToCell(path[^1])))
            return true;
        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3Int start = _wallTilemap.WorldToCell(path[i]);
            Vector3Int end = _wallTilemap.WorldToCell(path[i + 1]);


            Vector3Int direction = end - start;

            int stepX = direction.x != 0 ? (direction.x > 0 ? 1 : -1) : 0;
            int stepY = direction.y != 0 ? (direction.y > 0 ? 1 : -1) : 0;

            Vector3Int currentPosition = start;

            while (currentPosition != end)
            {
                if (_wallTilemap.HasTile(currentPosition) && IsDoorTile(currentPosition))
                {
                    TileBase tile = _wallTilemap.GetTile(currentPosition);
                    Debug.Log("cannot cross the door at: " + currentPosition);

                    if ((stepX != 0 && tile == doorTileVertical) || (stepY != 0 && tile == doorTileHorizontal))
                    {
                        return true; // 有门阻挡
                    }
                }

                currentPosition += new Vector3Int(stepX, stepY, 0);
            }
        }

        return false;
    }

    /// <summary>
    /// A* seeker寻路完成后所调用的回调函数,若路径有效则进行角色移动。
    /// </summary>
    /// <param name="p">seeker找到的Path路径</param>
    private void onPathComplete(Path p)
    {
        var targetWorldPosition = _groundTilemap.CellToWorld(_targetCellPosition) + _cellBias;
        if (p.error)
        {
            Debug.LogError("path error");
        }
        else
        {
            //成功找到路径
            if (p.vectorPath[^1]==targetWorldPosition&&_isMovable)
            {
                float duration = (p.vectorPath.Count - 1) * 0.6f; // 假设移动速度为每格0.6秒
                var pathArray = p.vectorPath.ToArray();

                if (CheckForDoorBlock(pathArray))
                {
                    Debug.Log("Path is blocked by a door.");
                    return; // 如果被门阻挡，直接返回，不进行后面的操作
                }

                foreach (var pathPoint in pathArray)
                {
                    // 将世界坐标转换为GlassTilemap中的网格坐标
                    Vector3Int cellPosition = _glassTilemap.WorldToCell(pathPoint);

                    Player.CmdBreakGlass(cellPosition);

                    // 记录相邻的门
                    Vector3Int cellPosition_1 = _wallTilemap.WorldToCell(pathPoint);
                    CheckForAdjacentDoor(cellPosition_1);
                }

                StartCoroutine(Player.drawPathLine(pathArray, duration)); // 绘制路径
                // 调用网络同步方法,改变位置并生成对应动画
                Player.SetPosition(targetWorldPosition, _targetCellPosition, pathArray);
                StartCoroutine(setMoveCD(duration));
            }
        }
    }

    /// <summary>
    /// 检查相邻格子是否有门
    /// </summary>
    private void CheckForAdjacentDoor(Vector3Int targetCellPos)
    {
        Vector3Int[] adjacentPositions = {
            targetCellPos + Vector3Int.left,
            targetCellPos + Vector3Int.right,
            targetCellPos + Vector3Int.up,
            targetCellPos + Vector3Int.down
        };

        foreach (var pos in adjacentPositions)
        {
            if (_wallTilemap.HasTile(pos) && (IsDoorTile(pos)))
            {
                _lastAdjacentDoor = pos;
                return;
            }
        }

        _lastAdjacentDoor = null;
    }

    private bool IsDoorTile(Vector3Int position)
    {
        TileBase tile = _wallTilemap.GetTile(position);
        return tile == doorTileHorizontal || tile == doorTileVertical;
    }

    public void BreakGlass(Vector3Int cellPosition)
    {
        if (_glassTilemap.HasTile(cellPosition))
        {
            _glassTilemap.SetTile(cellPosition, brokenGlassTile);
        }
    }

    /// <summary>
    /// 竖直门与水平门切换代表开关门
    /// </summary>
    public void RotateDoor(Vector3Int doorPosition)
    {
        TileBase tile = _wallTilemap.GetTile(doorPosition);
        if (tile != null)
        {
            // 旋转门Tile
            _pathBaker.Scan();
            if (tile == doorTileHorizontal)
            {
                _wallTilemap.SetTile(doorPosition, doorTileVertical);
            }
            else if (tile == doorTileVertical)
            {
                _wallTilemap.SetTile(doorPosition, doorTileHorizontal);
            }
            _wallCollider2D.usedByComposite = true;
            _shadowCreator.Create();
            _wallCollider2D.usedByComposite = false;
        }
    }

    /// <summary>
    /// 将_isMovable设置为false，经过cd秒后恢复为true。
    /// </summary>
    /// <param name="cd">_isMovable持续为false的时间</param>
    /// <returns></returns>
    private IEnumerator setMoveCD(float cd)
    {
        _isMovable = false;
        yield return new WaitForSeconds(cd);
        _gridLine.position = Player.transform.position + new Vector3(0.5f, 0.5f);
        _isMovable = true;
        updateGraph();
    }

    /// <summary>
    /// 更新用于寻路的graph位置到玩家处，并重新扫描附近障碍。
    /// </summary>
    private void updateGraph()
    {
        var graph = _pathBaker.data.gridGraph;
        graph.RelocateNodes(_groundTilemap.WorldToCell(Player.TilePosition), Quaternion.identity, 1);
        graph.is2D = true;
        _pathBaker.Scan();
    }
}