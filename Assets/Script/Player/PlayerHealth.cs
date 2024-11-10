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
    [SyncVar] private string _name;

    public string Name { get => _name; }
    
    /// <summary>
    /// 玩家的总血量上限
    /// </summary>
    public float TotalMaxHealth { get => _headMaxHealth + _bodyMaxHealth + _legMaxHealth; }
    /// <summary>
    /// 玩家的头部血量上限
    /// </summary>
    private float _headMaxHealth = 10;
    /// <summary>
    /// 玩家的身体血量上限
    /// </summary>
    private float _bodyMaxHealth = 10;
    /// <summary>
    /// 玩家的腿部血量上限
    /// </summary>
    private float _legMaxHealth = 10;

    public float HeadMaxHealth { get => _headMaxHealth; }
    public float BodyMaxHealth { get => _bodyMaxHealth; }
    public float LegMaxHealth { get => _legMaxHealth; }

    /// <summary>
    /// 玩家的当前总血量
    /// </summary>
    public float TotalHealth { get => _headHealth + _bodyHealth + _legHealth; }
    /// <summary>
    /// 玩家的当前头部血量，变化时会调用HeadHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(HeadHealthChange))] private float _headHealth = 10;
    /// <summary>
    /// 玩家的当前身体血量，变化时会调用BodyHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(BodyHealthChange))] private float _bodyHealth = 10;
    /// <summary>
    /// 玩家的当前腿部血量，变化时会调用LegHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(LegHealthChange))] private float _legHealth = 10;

    public float HeadHealth { get => _headHealth; }
    public float BodyHealth { get => _bodyHealth; }
    public float LegHealth { get => _legHealth; }

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
    private PlayerInfoUI LocalPlayerInfoPanel;

    // Start is called before the first frame update 
    void Start()
    {
        if (isLocalPlayer) 
        {
            // 获取玩家名字
            CmdSetName(PlayerPrefs.GetString("Name"));
            // 获取本地玩家信息面板
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject.GetComponent<PlayerInfoUI>();
            LocalPlayerInfoPanel.UpdateHealthPoint(_headHealth, _headMaxHealth, BodyPart.Head);
            LocalPlayerInfoPanel.UpdateHealthPoint(_bodyHealth, _bodyMaxHealth, BodyPart.Body);
            LocalPlayerInfoPanel.UpdateHealthPoint(_legHealth, _legMaxHealth, BodyPart.Leg);
        }
    }


    /// <summary>
    /// hook函数，当HeadHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void HeadHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        HeadHealthGUI.UpdateHealthGUILength(_headMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _headMaxHealth, BodyPart.Head);
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
        BodyHealthGUI.UpdateHealthGUILength(_bodyMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _bodyMaxHealth, BodyPart.Body);
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
        LegHealthGUI.UpdateHealthGUILength(_legMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _legMaxHealth, BodyPart.Leg);
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
        _name = name;
    }

}
