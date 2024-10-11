using Mirror;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    // ֱ��ʹ�� Player Ԥ�Ƽ�
    public GameObject playerPrefab; // �������õ� Player Ԥ����
    public Vector2 spawnAreaMin; // ������ɵ���Сλ��
    public Vector2 spawnAreaMax; // ������ɵ����λ��

    public override void OnStartServer()
    {
        // ֻ����һ�����
        SpawnPlayer();
    }

    [Server]
    private void SpawnPlayer()
    {
        // �������λ��
        Vector3 randomPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0);

        // ʵ�������Ԥ����
        GameObject player = Instantiate(playerPrefab, randomPosition, Quaternion.identity);

        // Ϊ����ҷ����������
        NetworkServer.Spawn(player);
    }
}
