using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Pathfinding;

public class GridMoveController : MonoBehaviour
{
    public static GridMoveController Instance;
    public PlayerMove Player;

    public Tile brokenGlassTile;

    public Tile doorTileHorizontal;
    public Tile doorTileVertical;

    // A* Ѱ·���
    private Seeker _pathSeeker;
    private Transform _pathTarget;
    private AstarPath _pathBaker;

    // ����Tilemaps
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

    private Vector3Int? _lastAdjacentDoor; // �����ŵ�λ��

    private void Start()
    {
        Instance = this;
        _groundTilemap = transform.GetChild(0).Find("GroundTilemap").GetComponent<Tilemap>();
        _wallTilemap = transform.GetChild(0).Find("WallTilemap").GetComponent<Tilemap>();
        _furnitureTilemap = transform.GetChild(0).Find("FurnitureTilemap").GetComponent<Tilemap>();
        _glassTilemap = transform.GetChild(0).Find("GlassTilemap").GetComponent<Tilemap>();
        _stuffTilemap = transform.GetChild(0).Find("StuffTilemap").GetComponent<Tilemap>();

        _shadowCreator = _wallTilemap.GetComponent<ShadowCaster2DCreator>();
        _wallCollider2D = _wallTilemap.GetComponent<TilemapCollider2D>();

        _gridLine = transform.Find("GridLine");

        _cellBias = _groundTilemap.cellSize * 0.5f;
    }

    private void Update()
    {
        if (_isMovable && Player != null)
            tryMove();

        // ����Ҽ������
        if (Input.GetKeyDown(KeyCode.Mouse1) && _lastAdjacentDoor.HasValue)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int doorPosition = _lastAdjacentDoor.Value;

            // �����λ�����ŵ�Tile��
            if (doorPosition == _wallTilemap.WorldToCell(mousePos))
            {
                Player.CmdRotateDoor(doorPosition);
            }
        }
    }

    /// <summary>
    /// ��player��ʼ��Ϊ����player�����ƶ���Ѱ·�������
    /// </summary>
    /// <param name="player">����player</param>
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
    /// ����������벢�����ƶ��������ɫ��Χ��7*7Tile����wasd�ƶ�����CD
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
    /// ���path��ÿ�������ڽڵ�֮���Ƿ����ź�ǰ������ֱ�赲ǰ����
    /// </summary>
    private bool CheckForDoorBlock(Vector3[] path)
    {
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
                        return true; // �����赲
                    }
                }

                currentPosition += new Vector3Int(stepX, stepY, 0);
            }
        }

        return false;
    }

    /// <summary>
    /// A* seekerѰ·��ɺ������õĻص�����,��·����Ч����н�ɫ�ƶ���
    /// </summary>
    /// <param name="p">seeker�ҵ���Path·��</param>
    private void onPathComplete(Path p)
    {
        var targetWorldPosition = _groundTilemap.CellToWorld(_targetCellPosition) + _cellBias;
        if (p.error)
        {
            Debug.Log("path error");
        }
        else
        {
            //�ɹ��ҵ�·��
            Debug.Log(p.vectorPath[^1]);
            if (p.vectorPath[^1] == targetWorldPosition && _isMovable)
            {
                float duration = (p.vectorPath.Count - 1) * 0.6f; // �����ƶ��ٶ�Ϊÿ��0.6��
                var pathArray = p.vectorPath.ToArray();

                if (CheckForDoorBlock(pathArray))
                {
                    Debug.Log("Path is blocked by a door.");
                    return; // ��������赲��ֱ�ӷ��أ������к���Ĳ���
                }

                foreach (var pathPoint in pathArray)
                {
                    // ����������ת��ΪGlassTilemap�е���������
                    Vector3Int cellPosition = _glassTilemap.WorldToCell(pathPoint);

                    Player.CmdBreakGlass(cellPosition);

                    // ��¼���ڵ���
                    Vector3Int cellPosition_1 = _wallTilemap.WorldToCell(pathPoint);
                    CheckForAdjacentDoor(cellPosition_1);
                }

                StartCoroutine(Player.drawPathLine(pathArray, duration)); // ����·��
                // ��������ͬ������,�ı�λ�ò����ɶ�Ӧ����
                Player.SetPosition(targetWorldPosition, _targetCellPosition, pathArray);
                StartCoroutine(setMoveCD(duration));
            }
        }
    }

    /// <summary>
    /// ������ڸ����Ƿ�����
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
    /// ��ֱ����ˮƽ���л���������
    /// </summary>
    public void RotateDoor(Vector3Int doorPosition)
    {
        TileBase tile = _wallTilemap.GetTile(doorPosition);
        if (tile != null)
        {
            // ��ת��Tile
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
    /// ��_isMovable����Ϊfalse������cd���ָ�Ϊtrue��
    /// </summary>
    /// <param name="cd">_isMovable����Ϊfalse��ʱ��</param>
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
    /// ��������Ѱ·��graphλ�õ���Ҵ���������ɨ�踽���ϰ���
    /// </summary>
    private void updateGraph()
    {
        var graph = _pathBaker.data.gridGraph;
        graph.RelocateNodes(_groundTilemap.WorldToCell(Player.TilePosition), Quaternion.identity, 1);
        graph.is2D = true;
        _pathBaker.Scan();
    }
}