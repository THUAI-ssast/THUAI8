using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 单例，用于游戏的Build配置
/// </summary>
public class AddService : MonoBehaviour
{
    public static AddService Instance;

    /// <summary>
    /// Build模式枚举类。
    /// <para>AppIsMatchServer：服务器上的匹配Server；</para>
    /// <para>AppIsMatchServer：服务器上的房间游戏Server；</para>
    /// <para>AppIsClient：与服务器交互的Client；</para>
    /// <para>ApplsHost：本地Host；</para>
    /// <para>AppIsLocalServer：本地Server；</para>
    /// <para>AppIsLocalClient：本地Client。</para>
    /// </summary>
    public enum AppBuildMode
    {
        AppIsMatchServer,
        AppIsGameServer,
        AppIsClient,
        AppIsHost,
        AppIsLocalServer,
        AppIsLocalClient
    }

    public AppBuildMode appBuildMode;

    public string ServerNetworkAddress = "150.158.44.119";
    public ushort MatchServerPort = 8000;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        switch (appBuildMode)
        {
            case AppBuildMode.AppIsMatchServer:
                
                break;
            case AppBuildMode.AppIsGameServer:
                SceneManager.LoadScene("RoomStartScene");
                break;
            case AppBuildMode.AppIsClient:

                break;
            case AppBuildMode.AppIsHost:
                // 直接进入游戏房间用于调试
                SceneManager.LoadScene("RoomStartScene");
                break;
            case AppBuildMode.AppIsLocalServer:
                SceneManager.LoadScene("RoomStartScene");
                break;
            case AppBuildMode.AppIsLocalClient:
                SceneManager.LoadScene("RoomStartScene");
                break;
            default:
                break;
        }
    }

    public void GetNetworkSettings()
    {
        var args = System.Environment.GetCommandLineArgs();
        ServerNetworkAddress = args[1];
        MatchServerPort = (ushort)int.Parse(args[2]);
        MatchMaker.Instance.GameServerPath = args[3];

        string GameServerPortRange = args[4];
        string pattern = @"(\d+)-(\d+)";
        Match match = Regex.Match(GameServerPortRange, pattern);
        if (match.Success)
        {
            string firstNumber = match.Groups[1].Value;
            string secondNumber = match.Groups[2].Value;
            MatchMaker.Instance.MinPort = int.Parse(firstNumber);
            MatchMaker.Instance.MaxPort = int.Parse(secondNumber);
        }
        else
        {
            Application.Quit();
        }
    }
}