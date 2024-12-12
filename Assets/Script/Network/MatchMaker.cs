using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

public class MatchMaker : MonoBehaviour
{
    public static MatchMaker Instance;

    [ReadOnly] public int MinPort = 8001;
    [ReadOnly] public int MaxPort = 8050;
    private List<int> _portsInUse = new List<int>();

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public int CreateRoomOnServer(int playerNumberInRoom)
    {
        // 搜索未被占用的端口，并在该端口创建游戏进程
        for(int port=MinPort; port <= MaxPort; port++)
        {
            if(!_portsInUse.Contains(port))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "/home/ubuntu/gameServer/UrbanOutlastServer.x86_64"; // 需要指定正确的程序路径名
                processStartInfo.Arguments = $"{playerNumberInRoom} {port}";    //指定房间人数和端口号
                Process.Start(processStartInfo);

                _portsInUse.Add(port);
                return port;
            }
        }
        return -1;
    }
}
