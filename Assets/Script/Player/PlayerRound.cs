using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 玩家行为类，管理回合、玩家准备状态，挂载在player上，定义[Command]方法。
/// </summary>
public class PlayerRound : NetworkBehaviour
{
    [Command]
    public void CmdReady(bool isReady)
    {
        RoundManager.Instance.Ready(GetComponent<NetworkIdentity>().netId, isReady);
    }
}
