using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class GameNetworkManager : NetworkManager
{
    public static GameNetworkManager Instance;

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            GameNetworkManager.Instance = this;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update(); 
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // 取消匹配
        LobbyManager.Instance.CancelGame(conn);

        // 销毁该连接拥有的所有对象
        int ownedLength = conn.owned.Count;
        NetworkIdentity[] identityArray = conn.owned.ToArray();
        for (int i = 0; i < ownedLength; i++) 
        {
            NetworkServer.Destroy(identityArray[i].gameObject);
        }
        // 调用基类的OnServerDisconnect来确保处理完所有断开连接的逻辑
        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        if (MainMenu.Instance.isJoinGame)
        {
            MainMenu.Instance.Player = null;
            MainMenu.Instance.CancelMatching();
        }
    }
}
