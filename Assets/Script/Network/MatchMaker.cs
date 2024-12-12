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
        // ����δ��ռ�õĶ˿ڣ����ڸö˿ڴ�����Ϸ����
        for(int port=MinPort; port <= MaxPort; port++)
        {
            if(!_portsInUse.Contains(port))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "/home/ubuntu/gameServer/UrbanOutlastServer.x86_64"; // ��Ҫָ����ȷ�ĳ���·����
                processStartInfo.Arguments = $"{playerNumberInRoom} {port}";    //ָ�����������Ͷ˿ں�
                Process.Start(processStartInfo);

                _portsInUse.Add(port);
                return port;
            }
        }
        return -1;
    }
}
