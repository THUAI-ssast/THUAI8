using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using Pathfinding;
using UnityEngine.UIElements;

public class GridMoveController : MonoBehaviour
{
    public static GridMoveController Instance;
    public PlayerMove Player;

    //A* ?��???
    private Seeker _pathSeeker;
    private Transform _pathTarget;
    private AstarPath _pathBaker;
    
    //????Tilemaps
    private Tilemap _groundTilemap;
    private Tilemap _wallTilemap;
    private Tilemap _furnitureTilemap;
    private Tilemap _glassTilemap;
    private Tilemap _stuffTilemap;

    private Transform _gridLine;

    private Vector3Int _targetCellPosition;
    private bool _isMovable = true;

    private Vector3 _cellBias;

    private void Start()
    {
        Instance = this;
        _groundTilemap = transform.GetChild(0).Find("GroundTilemap").GetComponent<Tilemap>();
        _wallTilemap = transform.GetChild(0).Find("WallTilemap").GetComponent<Tilemap>();
        _furnitureTilemap = transform.GetChild(0).Find("FurnitureTilemap").GetComponent<Tilemap>();
        _glassTilemap = transform.GetChild(0).Find("GlassTilemap").GetComponent<Tilemap>();
        _stuffTilemap = transform.GetChild(0).Find("ItemTilemap").GetComponent<Tilemap>();

        _gridLine = transform.Find("GridLine");

        _cellBias = _groundTilemap.cellSize * 0.5f;
    }

    private void Update()
    {
        if (!UIManager.Instance.IsUIActivating&&_isMovable&&Player!=null)
            tryMove();
    }
    /// <summary>
    /// ??player??????????player??????????��???????
    /// </summary>
    /// <param name="player">????player</param>
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
    /// ????????????????????????????��??7*7Tile????wasd???????CD
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
    /// A* seeker?��?????????????????,??��????��????��???????
    /// </summary>
    /// <param name="p">seeker?????Path��??</param>
    private void onPathComplete(Path p)
    {
        var targetWorldPosition = _groundTilemap.CellToWorld(_targetCellPosition) + _cellBias;
        if (p.error)
        {
            Debug.LogError("path error");
        }
        else
        {
            //??????��??
            if (p.vectorPath[^1]==targetWorldPosition&&_isMovable)
            {
                float duration = (p.vectorPath.Count - 1) * 0.6f; // ??????????????0.6??
                var pathArray = p.vectorPath.ToArray();
                StartCoroutine(Player.drawPathLine(pathArray, duration)); // ????��??
                // ???????????????,???��?��???????????
                Player.SetPosition(targetWorldPosition, _targetCellPosition, pathArray);
                StartCoroutine(setMoveCD(duration));
            }
            
        }
    }

    /// <summary>
    /// ??_isMovable?????false??????cd??????true??
    /// </summary>
    /// <param name="cd">_isMovable?????false?????</param>
    /// <returns></returns>
    private IEnumerator setMoveCD(float cd)
    {
        _isMovable = false;
        yield return new WaitForSeconds(cd);
        _gridLine.position = Player.transform.position + new Vector3(0.5f,0.5f);
        _isMovable = true;
        updateGraph();
    }

    /// <summary>
    /// ?????????��??graph��?????????????????��???????
    /// </summary>
    private void updateGraph()
    {
        var graph = _pathBaker.data.gridGraph;
        graph.RelocateNodes(_groundTilemap.WorldToCell(Player.TilePosition), Quaternion.identity, 1);
        graph.is2D = true;
        _pathBaker.Scan();
    }
}