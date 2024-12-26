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
            StartMenuManager.Instance.WarningDisplay("���䴴��ʧ��");
            return;
        }
        if (port == 0)
        {
            StartMenuManager.Instance.WarningDisplay("�÷��䲻����");
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
        if (MatchMaker.Instance.CheckPort(port)) // ����˿�δ��ռ�ã����޷����뷿�䣬��port=0Ϊ��ʶ
        {
            port = 0;
        }

        TargetInformPort(port);
    }
}
