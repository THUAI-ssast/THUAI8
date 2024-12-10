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

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
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
    [SerializeField] private  GameObject loadingPanel;
    [SerializeField] private  TextMeshProUGUI matchingText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    private RoomManager _roomManager;
    private int playerCount 
    { 
        get 
        {   
            int count = 0;
            foreach (NetworkRoomPlayer player in RoomManager.Instance.roomSlots)
            {
                if(player.readyToBegin)
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
        loadingPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void CancelMatching()
    {
        loadingPanel.SetActive(false);
        if (Player != null)
        {
            Player.CancelMatching();
        }
        if (matchingCoroutine != null)
        {
            StopCoroutine(matchingCoroutine);
        }
 
    }
    public void StartGame()
    {
        _roomManager.GameplayScene = "BattleScene";
        loadingPanel.SetActive(true);
        if (Player != null)
        {
            Player.StartMatching(playerNameText.text);
        }
        matchingCoroutine = StartCoroutine(StartMatching());
    }

    public void StartTutorial()
    {
        _roomManager.GameplayScene = "TutorialScene";
        //TODO:�Ķ˿ں�
        _roomManager.StartHost();
        OnPlayerAssigned += player => { player.StartMatching(playerNameText.text);};
    }

    IEnumerator StartMatching()
    {
        while (playerCount < playerCountLimit)
        {
            matchingText.text = "(" + playerCount.ToString() + "/" + playerCountLimit.ToString() + ")";
            yield return new WaitForSeconds(0.1f);
        }
    }

}
