using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New ResourcePointTile", menuName = "Tiles/ResourcePointTile")]
public class ResourcePointTile : Tile
{
    public string resourceID; // ÿ����Դ���Ψһ��ʶ��
    public string resourceName; // ��Դ�������
    public Sprite resourceIcon; // ��Դ���ͼ�꣬����UIչʾ��
    public int resourceAmount; // ��Դ�����Դ����
}
