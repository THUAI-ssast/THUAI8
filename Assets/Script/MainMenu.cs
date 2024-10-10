using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Runtime.CompilerServices;
using System.Threading;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private  GameObject loadingPanel;
    [SerializeField] private  TextMeshProUGUI matchingText;
    private int playercount = 1;
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
        matchingCoroutine = StartCoroutine(StartMatching());
    }
    IEnumerator StartMatching()
    {
        while (playercount < 6)
        {
            playercount = 1;
            matchingText.text = "(" + playercount.ToString() + "/6)";
            yield return new WaitForSeconds(1);
            playercount = 6;
        }
        SceneManager.LoadScene("BattleScene");
    }
}
