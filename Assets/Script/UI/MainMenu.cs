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

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    public NetPlayer Player;
    [SerializeField] private  GameObject loadingPanel;
    [SerializeField] private  TextMeshProUGUI matchingText;
    [SerializeField] private TextMeshProUGUI playerNameText;
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
        loadingPanel.SetActive(true);
        if (Player != null)
        {
            Player.StartMatching(playerNameText.text);
        }
        matchingCoroutine = StartCoroutine(StartMatching());
    }

    public void StartTutorial()
    {
        StartGame();
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
