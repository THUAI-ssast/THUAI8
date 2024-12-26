using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

/// <summary>
/// 玩家数据类，管理玩家的行动点(AP)相关
/// </summary>
public class PlayerActionPoint : NetworkBehaviour
{
    /// <summary>
    /// 玩家的体力值
    /// </summary>
    [SyncVar(hook = nameof(ActionPointChange))] private float _currentActionPoint = 15;

    /// <summary>
    /// 玩家当前的AP值
    /// </summary>
    public float CurrentActionPoint { get => _currentActionPoint; }

    /// <summary>
    /// 玩家的体力值上限
    /// </summary>
    [SerializeField] private float _maxActionPoint = 20;

    public float MaxActionPoint { get => _maxActionPoint; }

    /// <summary>
    /// 位于左下角的客户端玩家信息面板
    /// </summary>
    public PlayerInfoUI LocalPlayerInfoPanel;

    /// <summary>
    /// 用于显示体力不足的警告信息的协程
    /// </summary>
    private Coroutine displayCoroutine;

    /// <summary>
    /// 初始化玩家体力值
    /// </summary>
    void Start()
    {
        // 获取信息面板
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject.GetComponent<PlayerInfoUI>();
            LocalPlayerInfoPanel.UpdateActionPoint(_currentActionPoint, MaxActionPoint);
        }
    }

    /// <summary>
    /// hook函数，当ActionPoint改变后自动被调用
    /// </summary>
    /// <param name="oldActionPoint"></param>
    /// <param name="newActionPoint"></param>
    public void ActionPointChange(float oldActionPoint, float newActionPoint)
    {
        if (isLocalPlayer)
        {
            // 更新左下角的信息面板
            LocalPlayerInfoPanel.UpdateActionPoint(newActionPoint, MaxActionPoint);
            // 安全区伤害检测和施行
            float decreaseActionPoint = oldActionPoint - newActionPoint;
            if(decreaseActionPoint > 0)
            {
                SafeAreaManager.Instance.DoSafeAreaDamageByActionPoint(decreaseActionPoint);
            }
        }
    }

    /// <summary>
    /// 用于判断当前体力是否足够
    /// </summary>
    /// <param name="requiredActionPoint">大于0的数，代表操作需要的体力值</param>
    /// <param name="isDisplayUI">在体力不足时是否向玩家显示警告，默认是true</param>
    /// <returns>当前体力是否足以完成操作，true代表体力足够</returns>
    public bool CheckForEnoughActionPoint(float requiredActionPoint, bool isDisplayUI = true)
    {
        bool isEnough = (_currentActionPoint - requiredActionPoint) >= -0.01f;
        if (!isEnough && isDisplayUI)
        {
            UIManager.Instance.DisplayHoverStatusPanel("你的体力不足！");
        }
        return isEnough;
    }

    /// <summary>
    /// 增加玩家的体力，最高增加至体力上限
    /// </summary>
    /// <param name="increase">大于0的数，表示要增加的体力值</param>
    public void IncreaseActionPoint(float increase)
    {
        ChangeActionPoint(increase);
    }

    /// <summary>
    /// 减少玩家的体力，若体力不足则不会减少体力值。在调用该方法之前应调用CheckForEnoughPoint进行体力是否足够的判断
    /// </summary>
    /// <param name="decrease">大于0的数，表示要减少的体力值</param>
    /// <param name="isDisplayUI">在体力不足时是否向玩家显示警告，默认是true</param>
    public bool DecreaseActionPoint(float decrease, bool isDisplayUI = true)
    {
        if (!CheckForEnoughActionPoint(decrease, isDisplayUI))
            return false;
        ChangeActionPoint(-decrease);
        return true;
    }

    /// <summary>
    /// 改变玩家体力的统一接口
    /// </summary>
    /// <param name="increase">要改变的体力值：当increase大于0时，增加相应的体力值，最高增加至体力值上限；当increase小于0时，减少相应的体力值</param>
    private void ChangeActionPoint(float increase)
    {
        if (isServer)
        {
            DeployChangeActionPoint(increase);
        }
        else
        {
            CmdChangeActionPoint(increase);
        }
    }
    [Command]
    private void CmdChangeActionPoint(float increase)
    {
        DeployChangeActionPoint(increase);
    }
    /// <summary>
    /// 应用AP改变
    /// </summary>
    /// <param name="increase">变动值，正数为增加AP，负数为减少AP</param>
    private void DeployChangeActionPoint(float increase)
    {
        _currentActionPoint = Mathf.Clamp(_currentActionPoint + increase, 0, MaxActionPoint);
    }


}
