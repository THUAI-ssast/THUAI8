using System;
using System.Collections;
using Mirror;
using UnityEngine;

/// <summary>
/// 单例Manager，处理玩家的生成事件
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    static public PlayerManager Instance;
    void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    int _connectingPlayerCount;
    int _totalPlayerCount;
    public int AlivePlayerCount => _alivePlayerCount;
    [SyncVar(hook = nameof(RpcUpdateNumUI))] int _alivePlayerCount;
    void Start()
    {
        if(isServer)
        {
            _connectingPlayerCount = 0;
            _alivePlayerCount = 0;
            StartCoroutine(DeployUpdatePlayerCount());
        }
    }
    IEnumerator DeployUpdatePlayerCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if(NetworkServer.connections.Count != _connectingPlayerCount)
            {
                DeployUpdateAlivePlayer();   
                _connectingPlayerCount = NetworkServer.connections.Count;
            }
            _totalPlayerCount = Math.Max(_connectingPlayerCount, _totalPlayerCount);
        }
    }
    void DeployUpdateAlivePlayer()
    {
        int alivePlayer = 0;
        foreach (var connection in NetworkServer.connections)
        {
            int connectionId = connection.Key;
            PlayerHealth health = connection.Value.identity.GetComponent<PlayerHealth>();
            if(health.IsAlive == true)
            {
                alivePlayer++;
            }
        }
        _alivePlayerCount = alivePlayer;
    }
    void RpcUpdateNumUI(int oldAlivePlayer, int newAlivePlayer)
    {
        UIManager.Instance.UpdateAlivePlayersNumUI(newAlivePlayer);
    }
    public void DeployPlayerDie()
    {
        DeployUpdateAlivePlayer();
    }
}
