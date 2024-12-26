using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI类，用于显示玩家在BattleScene中的名字 
/// </summary>
public class NameGUI : NetworkBehaviour
{
    /// <summary>
    /// 对应的玩家Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// 相对于玩家位置的偏移量，用于调整玩家名字的显示位置
    /// </summary>
    public Vector3 Offset;


    void Update() 
    {
        gameObject.GetComponent<TMP_Text>().text = TargetPlayer.gameObject.GetComponent<PlayerHealth>().Name;
        transform.position = TargetPlayer.transform.position + Offset;
        transform.rotation = Quaternion.identity;
    }
}
