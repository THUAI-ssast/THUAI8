using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMoveController : MonoBehaviour
{
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private Tilemap _grounTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _stuffTilmap;

    [SerializeField] private float _moveCD;
    private bool _isMovable = true;

    private void Start()
    {
        var position = _playerMove.transform.position;
        //初始化玩家位置
        _playerMove.SetPosition(position, _grounTilemap.WorldToCell(position));
    }

    private void Update()
    {
        if (_isMovable)
            tryMove();
    }

    /// <summary>
    /// 捕获玩家输入并尝试移动，点击角色范围内7*7Tile或按下wasd移动，有CD
    /// </summary>
    private async void tryMove()
    {
        Vector3Int cellPos = Vector3Int.back;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _grounTilemap.WorldToCell(mousePos);
            if (Math.Abs(targetCellPos.x - _playerMove.TilePosition.x) <= 3 &&
                Math.Abs(targetCellPos.y - _playerMove.TilePosition.y) <= 3)
                cellPos = targetCellPos;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            cellPos = _playerMove.TilePosition + Vector3Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            cellPos = _playerMove.TilePosition + Vector3Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            cellPos = _playerMove.TilePosition + Vector3Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            cellPos = _playerMove.TilePosition + Vector3Int.down;
        }

        List<Vector3Int> path = new List<Vector3Int>();
        if (findTilePath(_playerMove.TilePosition, cellPos, path))
        {
            Vector3 bias = _grounTilemap.cellSize * 0.5f;
            List<Vector3> worldPath = new List<Vector3>(path.Count);
            foreach (var t in path)
                worldPath.Add(_grounTilemap.CellToWorld(t) + bias);

            _playerMove.SetPosition(_grounTilemap.CellToWorld(cellPos) + _grounTilemap.cellSize * 0.5f, cellPos,
                worldPath.ToArray());
            _isMovable = false;
            await Task.Delay((int)(_moveCD * 1000));
            _isMovable = true;
        }
    }

    /// <summary>
    /// 查找一条可以移动的路径，使用递归枚举
    /// </summary>
    /// <param name="from">起点，使用Tilemap坐标</param>
    /// <param name="to">目标点，使用Tilemap坐标</param>
    /// <param name="list">查找路径结果，使用Tilemap坐标</param>
    /// <returns>是否成功查找，查找结果则保存在list中</returns>
    private bool findTilePath(Vector3Int from, Vector3Int to, List<Vector3Int> list)
    {
        if ((!_grounTilemap.HasTile(from)) || _wallTilemap.HasTile(from))
            return false;
        list.Add(from);
        if (from == to)
            return true;
        Vector3Int horizontal = Math.Sign(to.x - from.x) * Vector3Int.right;
        Vector3Int vertical = Math.Sign(to.y - from.y) * Vector3Int.up;
        if ((horizontal != Vector3Int.zero && findTilePath(from + horizontal, to, list))
            || (vertical != Vector3Int.zero && findTilePath(from + vertical, to, list)))
            return true;
        list.RemoveAt(list.Count - 1);
        return false;
    }
}