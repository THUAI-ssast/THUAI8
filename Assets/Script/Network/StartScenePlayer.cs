using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScenePlayer : NetworkBehaviour
{
    public static StartScenePlayer LocalStartScenePlayer;

    public void Start()
    {
        if(isLocalPlayer)
        {
            LocalStartScenePlayer = this;
        }
    }

    [Command]
    public void CmdCreateRoom(int playerNumberInRoom)
    {
        int processPort = MatchMaker.Instance.CreateRoomOnServer(playerNumberInRoom);
        TargetInformPort(processPort);
    }

    [TargetRpc]
    public void TargetInformPort(int port)
    {
        if (port == -1)
        {
            StartMenuManager.Instance.WarningDisplay("房间创建失败");
            return;
        }
        if (port == 0)
        {
            StartMenuManager.Instance.WarningDisplay("该房间不存在");
            return;
        }

        NetworkManagerController.Instance.RoomPort = port;
        GameObject.Find("MatchNetworkManager").GetComponent<NetworkManager>().StopClient();
        NetworkManagerController.Instance.IsEnterRoom = true;
        SceneManager.LoadScene("RoomStartScene");
    }

    [Command]
    public void CmdJoinRoom(int roomID)
    {
        int port = roomID + AddService.Instance.MatchServerPort;
        if (MatchMaker.Instance.CheckPort(port)) // 如果端口未被占用，则无法加入房间，以port=0为标识
        {
            port = 0;
        }

        TargetInformPort(port);
    }
}
