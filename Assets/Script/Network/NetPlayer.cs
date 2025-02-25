using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 匹配阶段的玩家预制体，在StartScene中生成，用于处理在匹配阶段玩家客户端与服务端的通信事件
/// </summary>
public class NetPlayer : NetworkRoomPlayer
{
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.Player = this;
        }
        if (TutorialMenu.Instance != null)
        {
            TutorialMenu.Instance.Player = this;
        }
    }


    /// <summary>
    /// 通知服务端玩家开始匹配，同时记录有关该玩家的信息
    /// </summary>
    /// <param name="playerName">玩家的名字</param>
    public void StartMatching(string playerName)
    {
        Debug.Log("StartMatching func");
        // 通知服务端该玩家已准备
        CmdChangeReadyState(true);
        // 记录该玩家的名字
        PlayerPrefs.SetString("Name", playerName);  
    } 

    /// <summary>
    /// 通知服务端该玩家取消匹配
    /// </summary>
    public void CancelMatching()
    {
        // 通知服务端该玩家取消准备
        CmdChangeReadyState(false); 
    }

    [Command]
    public void CmdInitializeRoomInfo()
    {
        TargetInitializeRoomInfo(RoomInfoManager.Instance.PlayerNumberLimit, RoomInfoManager.Instance.RoomPort);
    }

    [TargetRpc]
    public void TargetInitializeRoomInfo(int playerNumberLimit, int port)
    {
        RoomInfoManager.Instance.PlayerNumberLimit = playerNumberLimit;
        RoomInfoManager.Instance.RoomPort = port;
    }
}
