using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Runtime.CompilerServices;
using System.Threading;
using Mirror.Examples.TopDownShooter;
using Unity.VisualScripting;
using System;

/// <summary>
/// 单例Manager，管理开始菜单界面的行为
/// </summary>
public class TutorialMenu : MonoBehaviour
{
    public static TutorialMenu Instance;
    private NetPlayer _player;

    // 定义一个事件，当 Player 被赋值时触发
    public event Action<NetPlayer> OnPlayerAssigned;
    public NetPlayer Player
    {
        get => _player;
        set
        {
            // 只有当值发生变化时才触发事件
            if (_player != value)
            {
                _player = value;
                OnPlayerAssigned?.Invoke(_player); // 触发事件
                OnPlayerAssigned = null;
            }
        }
    }

    private RoomManager _roomManager;
    private int playerCount
    {
        get
        {
            int count = 0;
            foreach (NetworkRoomPlayer player in RoomManager.Instance.roomSlots)
            {
                if (player.readyToBegin)
                {
                    count++;
                }
            }
            return count;
        }
    }
    private int playerCountLimit { get => RoomManager.Instance.maxConnections; }
    private Coroutine matchingCoroutine;

    public void Start()
    {
        Instance = this;
        _roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
    }

    public void StartTutorial()
    {
        //_roomManager.GameplayScene = "TutorialScene";
        //TODO:改端口号
        Debug.Log("StartTutorial func");
        _roomManager.StartHost();
        OnPlayerAssigned += player => { player.StartMatching("玩家"); };
    }

}
