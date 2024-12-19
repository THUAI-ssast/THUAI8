using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Pathfinding;
using Unity.VisualScripting;

/// <summary>
/// 单例Manager，地图交互类，管理本机角色点击地图的移动行为
/// </summary>
public class GridMoveController : MonoBehaviour
{
    public static GridMoveController Instance;
    /// <summary>
    /// 本机玩家
    /// </summary>
    public PlayerMove Player;
    /// <summary>
    /// 本机玩家是否存活
    /// </summary>
    private PlayerHealth _playerHealth;
    /// <summary>
    /// 破碎的玻璃对应的Tile
    /// </summary>
    public Tile brokenGlassTile;
    /// <summary>
    /// 水平的门Tile
    /// </summary>
    public Tile doorTileHorizontal;
    /// <summary>
    /// 垂直的门Tile
    /// </summary>
    public Tile doorTileVertical;

    // A* 寻路相关
    private Seeker _pathSeeker;
    private AstarPath _pathBaker;

    public Seeker PathSeeker
    {
        get => _pathSeeker;
        set => _pathSeeker = value;
    }

    public AstarPath PathBaker
    {
        get => _pathBaker;
        set => _pathBaker = value;
    }


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

    public Tilemap GroundTilemap
    {
        get => _groundTilemap;
        private set => _groundTilemap = value;
    }

    public Tilemap WallTilemap
    {
        get => _wallTilemap;
        private set => _wallTilemap = value;
    }

    public Tilemap GlassTilemap
    {
        get => _glassTilemap;
        private set => _glassTilemap = value;
    }

    public Tilemap FurnitureTilemap
    {
        get => _furnitureTilemap;
        private set => _furnitureTilemap = value;
    }

    /// <summary>
    /// 配合JudgeReachable函数使用，判断路径是否可行的指示变量
    /// </summary>
    private bool _isReachable;
    /// <summary>
    /// 配合JudgeReachable函数使用，存储的路径
    /// </summary>
    private Path _path;
    /// <summary>
    /// 配合JudgeReachable函数使用，用于悬停显示使用的targetWorldPosition
    /// </summary>
    private Vector3 _hoverUseTargetWorldPosition;

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
        UpdateShadowCaster();

        _gridLine = transform.Find("GridLine");

        _cellBias = _groundTilemap.cellSize * 0.5f;
    }

    private void Update()
    {
        if (!UIManager.Instance.IsUIActivating && _isMovable && Player != null && _playerHealth.IsAlive)
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
        var position = player.transform.position;
        Player.SetPosition(position, _groundTilemap.WorldToCell(position));
        var finderObj = Instantiate(Resources.Load<GameObject>(("PathFinder")), Player.transform);
        _pathBaker = finderObj.GetComponent<AstarPath>();
        _pathSeeker = finderObj.GetComponent<Seeker>();
        UpdateGraph();
        _gridLine.position = position + new Vector3(0.5f, 0.5f);
        _playerHealth = Player.GetComponent<PlayerHealth>();
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
            if (IsInBoundary(targetCellPos) && Player.TilePosition != targetCellPos)
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
            // debug
            Debug.Log(_targetCellPosition);
            var targetWorldPosition = _groundTilemap.CellToWorld(_targetCellPosition) + _cellBias;
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
            if (p.vectorPath[^1] == targetWorldPosition && _isMovable)
            {
                float duration = (p.vectorPath.Count - 1) * 0.5f; // 假设移动速度为每格0.6秒
                var pathArray = p.vectorPath.ToArray();

                if (CheckForDoorBlock(pathArray))
                {
                    Debug.Log("Path is blocked by a door.");
                    return; // 如果被门阻挡，直接返回，不进行后面的操作
                }

                float requiredActionPoint = ComputeRequiredActionPoint(p);
                if (!GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerActionPoint>().DecreaseActionPoint(requiredActionPoint))
                {
                    Debug.Log("ActionPoint is not enough.");
                    return; // 如果当前体力值不够，直接返回
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

                // 调用网络同步方法,改变位置并生成对应动画
                Player.CmdSetPosition(targetWorldPosition, _targetCellPosition, pathArray);
                StartCoroutine(setMoveCD(duration));

                //移动消耗体力
                
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

    public void BreakGlass(Vector3Int cellPosition, PlayerSound playerSound)
    {
        if (_glassTilemap.HasTile(cellPosition) && _glassTilemap.GetTile(cellPosition) != brokenGlassTile)
        {
            playerSound.PlayGlassBreakSound();
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
            UpdateShadowCaster();
        }
    }

    private void UpdateShadowCaster()
    {
        _wallCollider2D.usedByComposite = true;
        _shadowCreator.Create();
        _wallCollider2D.usedByComposite = false;
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
    }

    /// <summary>
    /// 更新用于寻路的graph位置到玩家处，并重新扫描附近障碍。
    /// </summary>
    public void UpdateGraph()
    {
        var graph = _pathBaker.data.gridGraph;
        Debug.Log(Player.TilePosition);
        graph.RelocateNodes(_groundTilemap.WorldToCell(Player.TilePosition), Quaternion.identity, 1);
        graph.is2D = true;
        _pathBaker.Scan();
    }
    public void ToggleMovementState(bool state)
    {
        _isMovable = state;
    }

    /// <summary>
    /// 判断某个tile位置是否可达。
    /// 注意：该函数没有返回值，是否可达的bool变量通过GridMoveController.Instance.IsReachable获取，可达情况下所需的体力值通过GridMoveController.Instance.RequiredActionPoint()获取
    /// </summary>
    /// <param name="targetCellPosition">需要进行判断的tile的位置</param>
    public void JudgeReachable(Vector3Int targetCellPosition)
    {
        _hoverUseTargetWorldPosition = _groundTilemap.CellToWorld(targetCellPosition) + _cellBias;
        _pathSeeker.StartPath(_groundTilemap.CellToWorld(Player.TilePosition) + _cellBias, _hoverUseTargetWorldPosition, onPathJudge);
    }

    /// <summary>
    /// 判断目标tile是否可达的回调函数
    /// </summary>
    /// <param name="p">通过寻路算法计算好的路径</param>
    private void onPathJudge(Path p)
    {
        var pathArray = p.vectorPath.ToArray();
        // 在目标不可达时，算法提供的路径只有可行的一部分，该路径中的最后一个位置不等于目标位置
        if (p.vectorPath[^1] != _hoverUseTargetWorldPosition || CheckForDoorBlock(pathArray))
        {
            _isReachable = false;
            _path = null;
            return;
        }
        _isReachable = true;
        _path = p;
    }

    /// <summary>
    /// 用以判断玩家是否正在移动
    /// </summary>
    public bool IsMovable => _isMovable;

    /// <summary>
    /// 配合JudgeReachable函数使用，表示目标位置是否可达
    /// </summary>
    public bool IsReachable => _isReachable;

    /// <summary>
    /// 配合JudgeReachable函数使用，获取到达目标所需要的体力值
    /// </summary>
    /// <returns>到达目标所需要的体力值，如果返回值是0则代表使用JudgeReachable函数出错</returns>
    public float RequiredActionPoint()
    {
        return (_path != null && _path.vectorPath != null) ? ComputeRequiredActionPoint(_path) : 0;
    }

    /// <summary>
    /// 判断某个tile的位置是否处在以玩家为中心的7*7范围内
    /// </summary>
    /// <param name="targetCellPos">需要判断的tile的位置</param>
    /// <returns>返回true则表示指定的tile处在以玩家为中心的7*7范围内</returns>
    public bool IsInBoundary(Vector3Int targetCellPos)
    {
        return Math.Abs(targetCellPos.x - Player.TilePosition.x) <= 3 &&
               Math.Abs(targetCellPos.y - Player.TilePosition.y) <= 3;
    }

    /// <summary>
    /// 计算玩家经过某条路径所需要的体力值
    /// </summary>
    /// <param name="path">用以计算的路径</param>
    /// <returns>所需的体力值</returns>
    private float ComputeRequiredActionPoint(Path path)
    {
        return 0.2f * (path.vectorPath.ToArray().Length - 1);
    }
}