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
    private int playerCount { get => LobbyManager.Instance.CurrentRoomPlayerCount;  }
    private int playerCountLimit { get => LobbyManager.Instance.MaxPlayersPerRoom;}
    private Coroutine matchingCoroutine; 
    public bool isJoinGame = false;

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
        Debug.Log(Player == null); 
        if (Player != null)
        {
            Player.CancelMatching();
            isJoinGame = false;
        }
        StopCoroutine(matchingCoroutine);
    }
    public void StartGame()
    {   
        loadingPanel.SetActive(true);
        if (Player != null)
        {
            Player.StartMatching();
            isJoinGame = true;
        }
        matchingCoroutine = StartCoroutine(StartMatching());
    }
    IEnumerator StartMatching()
    {
        while (playerCount < playerCountLimit)
        {
            matchingText.text = "(" + playerCount.ToString() + "/" + playerCountLimit.ToString() + ")";
            yield return new WaitForSeconds(0.1f);
        }
        SceneManager.LoadScene("BattleScene");
    }

}
