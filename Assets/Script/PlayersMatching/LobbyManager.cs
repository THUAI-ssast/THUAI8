using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [SyncVar]
    public int MaxPlayersPerRoom;

    
    [SyncVar]
    public int CurrentRoomPlayerCount;

    // 记录所有的匹配房间
    private List<RoomInfo> _roomInfos = new List<RoomInfo>();

    // 玩家加入大厅时调用
    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;
        // 创建初始房间
        _roomInfos.Add(new RoomInfo { RoomID = 0, PlayerInfos = new List<PlayerInfo>() });
    }

    // 玩家离开大厅时调用
    public override void OnStopServer()
    {
        base.OnStopServer();
        // 清除所有房间
        _roomInfos.Clear();

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Instance = this;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }


    // 玩家请求加入游戏匹配
    public void JoinGame(NetworkConnection conn = null)
    {
        // 创建新玩家
        var playerInfo = new PlayerInfo
        {
            PlayerID = conn.connectionId
        };
        // 将新玩家加入房间
        if (CurrentRoomPlayerCount == MaxPlayersPerRoom)
        {
            _roomInfos.Add(new RoomInfo { RoomID = _roomInfos.Count,PlayerInfos = new List<PlayerInfo>() });
        }
        RoomInfo room = _roomInfos[^1];
        room.PlayerInfos.Add(playerInfo);
        CurrentRoomPlayerCount = _roomInfos[^1].PlayerInfos.Count;
    }

    // 玩家退出游戏匹配
    public void CancelGame(NetworkConnection conn = null)
    {
        _roomInfos[^1].PlayerInfos.RemoveAll(item => item.PlayerID == conn.connectionId);
        CurrentRoomPlayerCount = _roomInfos[^1].PlayerInfos.Count;
    }
}


public class PlayerInfo
{
    public int PlayerID;
    public string PlayerName;
}


public class RoomInfo
{
    public int RoomID;
    public List<PlayerInfo> PlayerInfos;
}