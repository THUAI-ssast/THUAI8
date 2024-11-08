using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New ResourcePointTile", menuName = "Tiles/ResourcePointTile")]
public class ResourcePointTile : Tile
{
    public string resourceID; // 每个资源点的唯一标识符
    public string resourceName; // 资源点的名称
    public Sprite resourceIcon; // 资源点的图标，用于UI展示等
    public int resourceAmount; // 资源点的资源数量
}
