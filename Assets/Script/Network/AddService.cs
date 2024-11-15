using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;

public class AddService : MonoBehaviour
{
    public bool AppIsServer = false;
    private NetworkManager networkManager;
    void Start()
    {
        networkManager = GetComponent<RoomManager>();
        networkManager.networkAddress = "150.158.44.119";
        GetComponent<KcpTransport>().port = 8003;
        if (AppIsServer == true)
        {
            networkManager.StartServer();
        }
        else
        {
            networkManager.StartClient();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}