using kcp2k;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public static StartMenuManager Instance;

    // 开始界面中的UI对象
    [SerializeField] private GameObject _createButton;
    [SerializeField] private GameObject _joinButton;
    [SerializeField] private GameObject _quitButton;
    [SerializeField] private GameObject _playerNumberInput;
    [SerializeField] private GameObject _roomIDInput;
    [SerializeField] private GameObject _warningPanel;
    [SerializeField] private GameObject _tutorialButton;

    private int _playerNumberInRoom;


    void Start()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    }


    public void CreateRoom()
    {
        string playerNmuberText = _playerNumberInput.GetComponent<TMP_InputField>().text;
        Debug.Log(playerNmuberText);
        int playerNumberInRoom;
        if(int.TryParse(playerNmuberText, out playerNumberInRoom) && playerNumberInRoom >= 2 && playerNumberInRoom <=10)
        {
            if(StartScenePlayer.LocalStartScenePlayer == null)
            {
                WarningDisplay("未连接上服务器");
                return;
            }
            _playerNumberInRoom = playerNumberInRoom;
            StartScenePlayer.LocalStartScenePlayer.CmdCreateRoom(playerNumberInRoom);
        }
        else
        {
            WarningDisplay("请输入正确的房间人数\n(2到10之间的整数)");
        }
    }

    public void JoinRoom()
    {
        string roomIDText = _roomIDInput.GetComponent<TMP_InputField>().text;
        int roomID;
        if (int.TryParse(roomIDText, out roomID) 
            && roomID >= MatchMaker.Instance.MinPort - AddService.MatchServerPort 
            && roomID <= MatchMaker.Instance.MaxPort - AddService.MatchServerPort)
        {
            if (StartScenePlayer.LocalStartScenePlayer == null)
            {
                WarningDisplay("未连接上服务器");
                return;
            }
            StartScenePlayer.LocalStartScenePlayer.CmdJoinRoom(roomID);
        }
        else
        {
            WarningDisplay("请输入正确的房间号");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void WarningDisplay(string warningText)
    {
        _warningPanel.transform.Find("WarningText").GetComponent<TextMeshProUGUI>().text = warningText;
        _warningPanel.SetActive(true);
    }

    public void WarningClose()
    {
        _warningPanel.SetActive(false);
        _warningPanel.transform.Find("WarningText").GetComponent<TextMeshProUGUI>().text = "";
    }

    public void StartTutorial()
    {
        NetworkManager matchNetworkManager = GameObject.Find("MatchNetworkManager").GetComponent<NetworkManager>();
        matchNetworkManager.StopClient();
        SceneManager.LoadScene("TutorialStartScene");
    }

}
