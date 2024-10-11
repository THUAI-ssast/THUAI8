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

public class MainMenu : MonoBehaviour
{
    [SerializeField] private  GameObject loadingPanel;
    [SerializeField] private  TextMeshProUGUI matchingText;
    private int playerCount = 1;
    private int playerCountLimit = 10;
    private Coroutine matchingCoroutine;
    public void Start()
    {
        loadingPanel.SetActive(false);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public void CancelMatching()
    {
        loadingPanel.SetActive(false);
        StopCoroutine(matchingCoroutine);
    }
    public void StartGame()
    {   
        loadingPanel.SetActive(true);
        // NetworkManager.singleton.StartClient();
        playerCountLimit = GetServerPlayerCountLimit();
        matchingCoroutine = StartCoroutine(StartMatching());
    }
    IEnumerator StartMatching()
    {
        while (playerCount < playerCountLimit)
        {
            matchingText.text = "(" + playerCount.ToString() + "/" + playerCountLimit.ToString() + ")";
            yield return new WaitForSeconds(1);
            playerCount = GetServerPlayerCount();
        }
        SceneManager.LoadScene("BattleScene");
    }
    private int GetServerPlayerCountLimit()
    {
        // TODO
        return 6;
    }
    private int GetServerPlayerCount()
    {
        // TODO
        return 6;
    }
}
