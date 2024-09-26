using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMoveController : MonoBehaviour
{
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private Tilemap _grounTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _stuffTilmap;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = _grounTilemap.WorldToCell(mousePos);
            if (_grounTilemap.HasTile(cellPos))
            {
                if (!_wallTilemap.HasTile(cellPos))
                {
                    _playerMove.SetPosition(_grounTilemap.CellToWorld(cellPos)+_grounTilemap.cellSize*0.5f, cellPos);
                }
            }
        }
    }
}
