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
    [SerializeField] public TextMeshProUGUI roomIDText;
    private int playerCount => RoomInfoManager.Instance.ReadyPlayerNumber;
    private int playerCountLimit { get => RoomInfoManager.Instance.PlayerNumberLimit; }
    private Coroutine matchingCoroutine; 

    public void Start()
    {
        Instance = this;
        loadingPanel.SetActive(false);
        if(roomIDText != null)
        {
            roomIDText.text = $"·¿¼äºÅ: {NetworkManagerController.Instance.RoomPort - AddService.Instance.MatchServerPort}";
        }
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

    public void QuitRoom()
    {
        NetworkManagerController.Instance.IsEnterRoom = false;
        RoomManager.Instance.StopClient();
    }
}
