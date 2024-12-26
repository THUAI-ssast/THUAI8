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
        // ����δ��ռ�õĶ˿ڣ����ڸö˿ڴ�����Ϸ����
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
                processStartInfo.FileName = "../gameServer/UrbanOutlastServer.x86_64"; // ��Ҫָ����ȷ�ĳ���·����
                processStartInfo.Arguments = $"{playerNumberInRoom} {port}";    //ָ�����������Ͷ˿ں�
                Process.Start(processStartInfo);

                _portsInUse.Add(port);
                return port;
            }
        }
        return -1;
    }

    /// <summary>
    /// �ж�ָ���Ķ˿��Ƿ�ռ�ã�����ֵ��true���ʾ�˿�δ��ռ��
    /// </summary>
    /// <param name="port">ָ���Ķ˿ں�</param>
    /// <returns></returns>
    public bool CheckPort(int port)
    {
        string lsofCommand = $"lsof -i :{port}";

        // ִ��lsof�����ȡ���
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{lsofCommand}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // �ж�����Ƿ�Ϊ�գ��Ӷ��ж϶˿��Ƿ�ռ��
        return string.IsNullOrEmpty(output.Trim());
    }
}
