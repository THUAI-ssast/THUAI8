using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// �̳���NetworkRoomManager�����ڹ�����ͬ�����ء�����ƥ�䡢Ԥ�������ɵ�
/// </summary>
public class RoomManager : NetworkRoomManager
{
    /// <summary>
    /// RoomManager�ĵ���
    /// </summary>
    public static RoomManager Instance;

    // Start is called before the first frame update
    public override void Start()
    {
        Instance = this;
    }
}
