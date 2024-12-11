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
    [SyncVar(hook = nameof(UpdateNumUI))] int _alivePlayerCount;
    int _tmpDeadPlayerCount;
    void Start()
    {
        if(isServer)
        {
            _connectingPlayerCount = 0;
            _alivePlayerCount = 0;
            _tmpDeadPlayerCount = 0;
            StartCoroutine(UpdatePlayerCount());
        }
    }
    IEnumerator UpdatePlayerCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            _connectingPlayerCount = NetworkServer.connections.Count;
            _totalPlayerCount = Math.Max(_connectingPlayerCount, _totalPlayerCount);
            _alivePlayerCount = _connectingPlayerCount - _tmpDeadPlayerCount;
        }
    }
    void UpdateNumUI(int oldAlivePlayer, int newAlivePlayer)
    {
        Debug.Log("hook: UpdateNumUI");
        UIManager.Instance.UpdateAlivePlayersNumUI(newAlivePlayer);
    }
    public void DeployPlayerDie()
    {
        _tmpDeadPlayerCount++;
    }
    public void DeployDeadPlayerLogout()
    {
        _tmpDeadPlayerCount--;
    }
}
