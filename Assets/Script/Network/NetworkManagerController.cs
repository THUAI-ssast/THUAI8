using kcp2k;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class NetworkManagerController : MonoBehaviour
{
    public static NetworkManagerController Instance;

    public int RoomPort;

    public bool IsEnterRoom = true;

    public bool IsEnterTutorial = true;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        switch (scene.name)
        {
            case "StartScene":
                if(AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsClient)
                {
                    IsEnterRoom = true;
                    IsEnterTutorial = true;

                    NetworkManager matchNetworkManager = GameObject.Find("MatchNetworkManager").GetComponent<NetworkManager>();
                    matchNetworkManager.networkAddress = AddService.Instance.ServerNetworkAddress;
                    matchNetworkManager.GetComponent<KcpTransport>().port = AddService.Instance.MatchServerPort;
                    matchNetworkManager.StartClient();
                }
                else if(AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsMatchServer)
                {
                    NetworkManager matchNetworkManager = GameObject.Find("MatchNetworkManager").GetComponent<NetworkManager>();
                    matchNetworkManager.networkAddress = AddService.Instance.ServerNetworkAddress;
                    matchNetworkManager.GetComponent<KcpTransport>().port = AddService.Instance.MatchServerPort;
                    matchNetworkManager.StartServer();
                }
                break;
            case "RoomStartScene":
                if(AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsClient)
                {
                    if(IsEnterRoom)
                    {
                        StartCoroutine(EnterRoomOnClient());
                    }
                    else
                    {
                        if (GameObject.Find("RoomManager"))
                        {
                            Debug.Log("Delete RoomManager");
                            Destroy(GameObject.Find("RoomManager"));
                        }
                        SceneManager.LoadScene("StartScene");
                    }
                }
                else if(AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsGameServer)
                {
                    CreateRoomOnGameServer();
                    StartCoroutine(destroyRoomEnumerator());
                }
                else if (AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsLocalServer)
                {
                    RoomManager.Instance.StartServer();
                    StartCoroutine(destroyRoomEnumerator());
                }
                else if (AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsLocalClient)
                {
                    RoomManager.Instance.StartClient();
                }
                break;
            case "RoomScene":
                if(AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsClient)
                {
                    
                }
                break;
            case "TutorialStartScene":
                if (AddService.Instance.appBuildMode == AddService.AppBuildMode.AppIsClient)
                {
                    if (IsEnterTutorial)
                    {
                        StartCoroutine(EnterTutorial());
                    }
                    else
                    {
                        if (GameObject.Find("RoomManager"))
                        {
                            Debug.Log("Delete RoomManager");
                            Destroy(GameObject.Find("RoomManager"));
                        }
                        SceneManager.LoadScene("StartScene");
                    }
                }
                break;
            default:
                break;
        }
    }

    public IEnumerator EnterRoomOnClient()
    {
        yield return new WaitForSeconds(1f);
        RoomManager.Instance.networkAddress = AddService.Instance.ServerNetworkAddress;
        RoomManager.Instance.GetComponent<KcpTransport>().port = (ushort)RoomPort;
        RoomManager.Instance.StartClient();
    }

    public void CreateRoomOnGameServer()
    {
        RoomManager.Instance.networkAddress = AddService.Instance.ServerNetworkAddress;
        var args = System.Environment.GetCommandLineArgs();
        int playerNumber;
        if (int.TryParse(args[1], out playerNumber))
        {
            RoomManager.Instance.maxConnections = RoomManager.Instance.minPlayers = playerNumber;
        }
        else
        {
            Application.Quit();
        }
        
        int port;
        if (int.TryParse(args[3], out port))
        {
            RoomManager.Instance.GetComponent<KcpTransport>().port = (ushort)port;
        }
        else
        {
            Application.Quit();
        }
        RoomManager.Instance.StartServer();
    }

    public IEnumerator EnterTutorial()
    {
        yield return new WaitForSeconds(0.1f);
        TutorialMenu.Instance.StartTutorial();
    }

    IEnumerator destroyRoomEnumerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if (NetworkServer.connections.Count == 0)
            {
                Process currentProcess = Process.GetCurrentProcess();
                // 结束当前进程
                currentProcess.Kill();
            }
        }
    }
}
