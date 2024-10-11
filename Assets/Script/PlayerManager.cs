using Mirror;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    // 直接使用 Player 预制件
    public GameObject playerPrefab; // 用于设置的 Player 预制体
    public Vector2 spawnAreaMin; // 随机生成的最小位置
    public Vector2 spawnAreaMax; // 随机生成的最大位置

    public override void OnStartServer()
    {
        // 只生成一个玩家
        SpawnPlayer();
    }

    [Server]
    private void SpawnPlayer()
    {
        // 生成随机位置
        Vector3 randomPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0);

        // 实例化玩家预制体
        GameObject player = Instantiate(playerPrefab, randomPosition, Quaternion.identity);

        // 为该玩家分配网络身份
        NetworkServer.Spawn(player);
    }
}
