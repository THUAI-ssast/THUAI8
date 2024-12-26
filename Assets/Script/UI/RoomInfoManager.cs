using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;

public class RoomInfoManager : NetworkBehaviour
{
    public static RoomInfoManager Instance;

    public int PlayerNumberLimit;
    [SyncVar] public int PlayerNumberInRoom;
    [SyncVar] public int ReadyPlayerNumber;
    public int RoomPort;
    
    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (isServer)
        {
            PlayerNumberLimit = RoomManager.Instance.maxConnections;
            RoomPort = RoomManager.Instance.GetComponent<KcpTransport>().port;
            StartCoroutine(ListenToNewConnectionOnServer());
        }
        if(isClient)
        {
            InitializeRoomInfo();
        }
    }

    private IEnumerator ListenToNewConnectionOnServer()
    {
        while(true)
        {
            PlayerNumberInRoom = NetworkServer.connections.Count;
            
            int count = 0;
            foreach (NetworkRoomPlayer player in RoomManager.Instance.roomSlots)
            {
                if (player.readyToBegin)
                {
                    count++;
                }
            }
            ReadyPlayerNumber = count;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void InitializeRoomInfo()
    {
        MainMenu.Instance.Player.CmdInitializeRoomInfo();
    }
}
