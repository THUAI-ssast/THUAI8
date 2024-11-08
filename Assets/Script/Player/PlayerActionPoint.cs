using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class PlayerActionPoint : NetworkBehaviour
{
    /// <summary>
    /// 玩家的体力值
    /// </summary>
    [SyncVar(hook = nameof(ActionPointChange))] private float _currentActionPoint = 15;

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
        // 更新左下角的信息面板
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateActionPoint(newActionPoint, MaxActionPoint);
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
        bool isEnough = (_currentActionPoint - requiredActionPoint) >= 0;
        if (!isEnough && isDisplayUI)
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            displayCoroutine = StartCoroutine(DisplayAPNotEnoughWarning());
        }
        return isEnough;
    }

    /// <summary>
    /// 增加玩家的体力，最高增加至体力上限
    /// </summary>
    /// <param name="increase">大于0的数，表示要增加的体力值</param>
    public void IncreaseActionPoint(float increase)
    {
        CmdChangeActionPoint(increase);
    }

    /// <summary>
    /// 减少玩家的体力，若体力不足则不会减少体力值。在调用该方法之前应调用CheckForEnoughPoint进行体力是否足够的判断
    /// </summary>
    /// <param name="decrease">大于0的数，表示要减少的体力值</param>
    public void DecreaseActionPoint(float decrease)
    {
        CmdChangeActionPoint(-decrease);
    }

    /// <summary>
    /// 改变玩家体力的统一接口。
    /// </summary>
    /// <param name="increase">要改变的体力值：当increase大于0时，增加相应的体力值，最高增加至体力值上限；当increase小于0时，减少相应的体力值</param>
    [Command]
    private void CmdChangeActionPoint(float increase)
    {
        float tempActionPoint = _currentActionPoint + increase;
        if (tempActionPoint > MaxActionPoint)
        {
            _currentActionPoint = MaxActionPoint;
        }
        else if(tempActionPoint >= 0)
        {
            _currentActionPoint = tempActionPoint;
        }
    }

    /// <summary>
    /// 弹出UI提示玩家体力值不足
    /// </summary>
    private IEnumerator DisplayAPNotEnoughWarning()
    {
        GameObject ActionPointWarningPanel = GameObject.Find("Canvas").transform.Find("ActionPointWarningPanel").gameObject;
        ActionPointWarningPanel.GetComponent<CanvasGroup>().alpha = 1f;
        ActionPointWarningPanel.SetActive(true);
        yield return new WaitForSeconds(1);
        ActionPointWarningPanel.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        yield return new WaitForSeconds(1);
        ActionPointWarningPanel.SetActive(false);
    }

}
