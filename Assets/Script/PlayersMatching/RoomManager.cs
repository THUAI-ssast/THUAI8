using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 单例Manager类，用于管理场景同步加载、房间匹配、预制体生成等
/// </summary>
public class RoomManager : NetworkRoomManager
{
    /// <summary>
    /// RoomManager的单例
    /// </summary>
    public static RoomManager Instance;

    public override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            base.Awake();
            Instance = this;
        }

    }

    // Start is called before the first frame update
    public override void Start()
    {

    }

}
