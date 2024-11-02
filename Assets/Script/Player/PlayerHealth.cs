using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 该类中保存了玩家的名字、血量上限和当前血量，以及处理血量变化逻辑的方法
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    /// <summary>
    /// 玩家的名字
    /// </summary>
    [SyncVar] public string Name;
    
    /// <summary>
    /// 玩家的总血量上限
    /// </summary>
    public float TotalMaxHealth { get => HeadMaxHealth + BodyMaxHealth + LegMaxHealth; }
    /// <summary>
    /// 玩家的头部血量上限
    /// </summary>
    public float HeadMaxHealth;
    /// <summary>
    /// 玩家的身体血量上限
    /// </summary>
    public float BodyMaxHealth;
    /// <summary>
    /// 玩家的腿部血量上限
    /// </summary>
    public float LegMaxHealth;

    /// <summary>
    /// 玩家的当前总血量
    /// </summary>
    public float TotalHealth { get => HeadHealth + BodyHealth + LegHealth; }
    /// <summary>
    /// 玩家的当前头部血量，变化时会调用HeadHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(HeadHealthChange))] public float HeadHealth;
    /// <summary>
    /// 玩家的当前身体血量，变化时会调用BodyHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(BodyHealthChange))] public float BodyHealth;
    /// <summary>
    /// 玩家的当前腿部血量，变化时会调用LegHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(LegHealthChange))] public float LegHealth;

    /// <summary>
    /// 对应的玩家对象Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// 玩家的头部血条UI
    /// </summary>
    public HealthGUI HeadHealthGUI;
    /// <summary>
    /// 玩家的身体血条UI
    /// </summary>
    public HealthGUI BodyHealthGUI;
    /// <summary>
    /// 玩家的腿部血条UI
    /// </summary>
    public HealthGUI LegHealthGUI;

    /// <summary>
    /// 位于左下角的客户端玩家信息面板
    /// </summary>
    public PlayerInfoUI LocalPlayerInfoPanel;

    // Start is called before the first frame update 
    void Start()
    {
        if (isLocalPlayer) 
        {
            // 获取玩家名字
            CmdSetName(PlayerPrefs.GetString("Name"));
            // 获取本地玩家信息面板
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject.GetComponent<PlayerInfoUI>();
        }
        // 赋予初始血量
        HeadHealth = BodyHealth = LegHealth = HeadMaxHealth = BodyMaxHealth = LegMaxHealth = 10;
    }


    /// <summary>
    /// hook函数，当HeadHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void HeadHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        HeadHealthGUI.UpdateHealthGUILength(HeadMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, HeadMaxHealth, BodyPart.Head);
        }
    }

    /// <summary>
    /// hook函数，当BodyHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void BodyHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        BodyHealthGUI.UpdateHealthGUILength(BodyMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, BodyMaxHealth, BodyPart.Body);
        }
    }

    /// <summary>
    /// hook函数，当LegHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void LegHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        LegHealthGUI.UpdateHealthGUILength(LegMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, LegMaxHealth, BodyPart.Leg);
        }
    }

    /// <summary>
    /// Command函数，在客户端被调用，但在服务端执行。
    /// 向服务端同步玩家的名字
    /// </summary>
    /// <param name="name">玩家的名字</param>
    [Command]
    private void CmdSetName(string name)
    {
        Name = name;
    }

}
