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

    // ��¼���е�ƥ�䷿��
    private List<RoomInfo> _roomInfos = new List<RoomInfo>();

    // ��Ҽ������ʱ����
    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;
        // ������ʼ����
        _roomInfos.Add(new RoomInfo { RoomID = 0, PlayerInfos = new List<PlayerInfo>() });
    }

    // ����뿪����ʱ����
    public override void OnStopServer()
    {
        base.OnStopServer();
        // ������з���
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


    // ������������Ϸƥ��
    public void JoinGame(NetworkConnection conn = null)
    {
        // ���������
        var playerInfo = new PlayerInfo
        {
            PlayerID = conn.connectionId
        };
        // ������Ҽ��뷿��
        if (CurrentRoomPlayerCount == MaxPlayersPerRoom)
        {
            _roomInfos.Add(new RoomInfo { RoomID = _roomInfos.Count,PlayerInfos = new List<PlayerInfo>() });
        }
        RoomInfo room = _roomInfos[^1];
        room.PlayerInfos.Add(playerInfo);
        CurrentRoomPlayerCount = _roomInfos[^1].PlayerInfos.Count;
    }

    // ����˳���Ϸƥ��
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