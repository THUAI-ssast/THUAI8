using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MatchMaker : MonoBehaviour
{
    public static MatchMaker Instance;

    public string GameServerPath;
    public int MinPort = 8001;
    public int MaxPort = 8050;
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
            Debug.Log($"-------------------{port} in use {CheckPort(port)}-------------------");
            if (CheckPort(port))
            {
                _portsInUse.Remove(port);
            }

            if(!_portsInUse.Contains(port))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "../gameServer/UrbanOutlastServer.x86_64"; // 需要指定正确的程序路径名
                processStartInfo.Arguments = $"{playerNumberInRoom} {port}";    //指定房间人数和端口号
                Process.Start(processStartInfo);

                _portsInUse.Add(port);
                return port;
            }
        }
        return -1;
    }

    /// <summary>
    /// 判断指定的端口是否被占用，返回值是true则表示端口未被占用
    /// </summary>
    /// <param name="port">指定的端口号</param>
    /// <returns></returns>
    public bool CheckPort(int port)
    {
        string lsofCommand = $"lsof -i :{port}";

        // 执行lsof命令并获取输出
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{lsofCommand}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // 判断输出是否为空，从而判断端口是否被占用
        return string.IsNullOrEmpty(output.Trim());
    }
}
