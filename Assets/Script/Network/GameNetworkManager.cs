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
        // ȡ��ƥ��
        LobbyManager.Instance.CancelGame(conn);

        // ���ٸ�����ӵ�е����ж���
        int ownedLength = conn.owned.Count;
        NetworkIdentity[] identityArray = conn.owned.ToArray();
        for (int i = 0; i < ownedLength; i++) 
        {
            NetworkServer.Destroy(identityArray[i].gameObject);
        }
        // ���û����OnServerDisconnect��ȷ�����������жϿ����ӵ��߼�
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
