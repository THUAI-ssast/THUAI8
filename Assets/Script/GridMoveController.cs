using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class GridMoveController : MonoBehaviour
{
    public static GridMoveController Instance;
    public PlayerMove Player;
    [SerializeField] private Tilemap _grounTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _stuffTilmap;

    [SerializeField] private float _moveCD;
    private bool _isMovable = true;

    private void Start()
    {
        Instance = this;
       
    }

    private void Update()
    {
        if (_isMovable&&Player!=null)
            tryMove();
    }

    public void InitPlayerPosition()
    {
        var position = Player.transform.position;
        Player.SetPosition(position, _grounTilemap.WorldToCell(position));
    }

    /// <summary>
    /// 捕获玩家输入并尝试移动，点击角色范围内7*7Tile或按下wasd移动，有CD
    /// </summary>
    private void tryMove()
    {
        Vector3Int cellPos = Vector3Int.back;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _grounTilemap.WorldToCell(mousePos);
            if (Math.Abs(targetCellPos.x - Player.TilePosition.x) <= 3 &&
                Math.Abs(targetCellPos.y - Player.TilePosition.y) <= 3)
                cellPos = targetCellPos;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            cellPos = Player.TilePosition + Vector3Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            cellPos = Player.TilePosition + Vector3Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            cellPos = Player.TilePosition + Vector3Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            cellPos = Player.TilePosition + Vector3Int.down;
        }

        List<Vector3Int> path = new List<Vector3Int>();
        if (findTilePath(Player.TilePosition, cellPos, path))
        {
            Vector3 bias = _grounTilemap.cellSize * 0.5f;
            List<Vector3> worldPath = new List<Vector3>(path.Count);
            foreach (var t in path)
                worldPath.Add(_grounTilemap.CellToWorld(t) + bias);

            float duration = (path.Count - 1) * 0.6f; // 假设移动速度为每格0.6秒
            StartCoroutine(Player.drawPathLine(worldPath.ToArray(), duration)); // 绘制路径

            // 调用网络同步方法
            Player.SetPosition(_grounTilemap.CellToWorld(cellPos) + _grounTilemap.cellSize * 0.5f, cellPos, worldPath.ToArray());
            StartCoroutine(setMoveCD());
        }
    }

    //[Command]
    private void CmdMovePlayer(Vector3 worldPosition, Vector3Int tilePosition, Vector3[] path)
    {
        RpcMovePlayer(worldPosition, tilePosition, path);
    }

    //[ClientRpc]
    private void RpcMovePlayer(Vector3 worldPosition, Vector3Int tilePosition, Vector3[] path)
    {
        
    }

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

    private IEnumerator setMoveCD()
    {
        _isMovable = false;
        yield return new WaitForSeconds(_moveCD);
        _isMovable = true;
    }
}