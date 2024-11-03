using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRound : NetworkBehaviour
{
    [Command]
    public void CmdReady(bool isReady)
    {
        Debug.Log("CmdReady");
        RoundManager.Instance.Ready(GetComponent<NetworkIdentity>().netId, isReady);
    }
}
