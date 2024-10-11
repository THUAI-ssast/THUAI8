using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : NetworkBehaviour
{
    private void Start()
    {
        if (isLocalPlayer)
        {
            MainMenu.Instance.Player = this;
        }
    }

    [Command]
    public void StartMatching(NetworkConnectionToClient conn = null)
    {
        LobbyManager.Instance.JoinGame(conn);
    }

    [Command]
    public void CancelMatching(NetworkConnectionToClient conn = null)
    {
        LobbyManager.Instance.CancelGame(conn);
    }
}
