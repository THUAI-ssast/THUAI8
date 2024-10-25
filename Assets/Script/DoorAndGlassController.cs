using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;

public class DoorAndGlassController : NetworkBehaviour
{
    public static DoorAndGlassController Instance;
    public Tile brokenGlassTile;
    public Tile doorTileHorizontal;
    public Tile doorTileVertical;

    private Tilemap _wallTilemap;
    private Tilemap _glassTilemap;

    private void Awake()
    {
        if(DoorAndGlassController.Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    public void RotateDoor(Vector3Int doorPosition)
    {
        TileBase tile = _wallTilemap.GetTile(doorPosition);
        if (tile != null)
        {
            if (tile == doorTileHorizontal)
            {
                _wallTilemap.SetTile(doorPosition, doorTileVertical);
            }
            else if (tile == doorTileVertical)
            {
                _wallTilemap.SetTile(doorPosition, doorTileHorizontal);
            }
        }
    }

    public void BreakGlass(Vector3Int position)
    {
        if (_glassTilemap.HasTile(position))
        {
            _glassTilemap.SetTile(position, brokenGlassTile);
        }
    }
}
