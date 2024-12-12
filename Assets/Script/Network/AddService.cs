using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddService : MonoBehaviour
{
    public static AddService Instance; 

    public enum AppBuildMode
    {
        AppIsMatchServer,
        AppIsGameServer,
        AppIsClient,
        AppIsHost
    }

    public AppBuildMode appBuildMode;

    public static string ServerNetworkAddress = "150.158.44.119";
    public static ushort MatchServerPort = 8000;

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
            default:
                break;
        }
    }
}