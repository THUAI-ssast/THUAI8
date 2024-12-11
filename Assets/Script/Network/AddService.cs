using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;

/// <summary>
/// 网络类，开启时自动连接至云服务器
/// </summary>
public class AddService : MonoBehaviour
{
    /// <summary>
    /// 是否以服务器模式启动
    /// </summary>
    public bool AppIsServer = false;
    private NetworkManager networkManager;
    void Start()
    {
        networkManager = GetComponent<RoomManager>();
        networkManager.networkAddress = "150.158.44.119";
        GetComponent<KcpTransport>().port = 8003;
        if (AppIsServer == true)
        {
            networkManager.StartServer();
        }
        else
        {
            networkManager.StartClient();
        }

    }
}