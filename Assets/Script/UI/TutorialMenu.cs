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
/// ����Manager������ʼ�˵��������Ϊ
/// </summary>
public class TutorialMenu : MonoBehaviour
{
    public static TutorialMenu Instance;
    private NetPlayer _player;

    // ����һ���¼����� Player ����ֵʱ����
    public event Action<NetPlayer> OnPlayerAssigned;
    public NetPlayer Player
    {
        get => _player;
        set
        {
            // ֻ�е�ֵ�����仯ʱ�Ŵ����¼�
            if (_player != value)
            {
                _player = value;
                OnPlayerAssigned?.Invoke(_player); // �����¼�
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
        //TODO:�Ķ˿ں�
        Debug.Log("StartTutorial func");
        _roomManager.StartHost();
        OnPlayerAssigned += player => { player.StartMatching("���"); };
    }

}
