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
    /// ��ҵ�����ֵ
    /// </summary>
    [SyncVar(hook = nameof(ActionPointChange))] private float _currentActionPoint;

    /// <summary>
    /// ��ҵ�����ֵ����
    /// </summary>
    [SerializeField] public float MaxActionPoint;

    /// <summary>
    /// λ�����½ǵĿͻ��������Ϣ���
    /// </summary>
    public PlayerInfoUI LocalPlayerInfoPanel;

    /// <summary>
    /// ������ʾ��������ľ�����Ϣ��Э��
    /// </summary>
    private Coroutine displayCoroutine;

    /// <summary>
    /// ��ʼ���������ֵ
    /// </summary>
    void Start()
    {
        // ��ȡ��Ϣ���
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject.GetComponent<PlayerInfoUI>();
        }
        // ��ʼ������ֵ���޺�����ֵ
        MaxActionPoint = 20;
        _currentActionPoint = 15;
    }

    /// <summary>
    /// hook��������ActionPoint�ı���Զ�������
    /// </summary>
    /// <param name="oldActionPoint"></param>
    /// <param name="newActionPoint"></param>
    public void ActionPointChange(float oldActionPoint, float newActionPoint)
    {
        // �������½ǵ���Ϣ���
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateActionPoint(newActionPoint, MaxActionPoint);
        }
    }

    /// <summary>
    /// �����жϵ�ǰ�����Ƿ��㹻
    /// </summary>
    /// <param name="requiredActionPoint">����0���������������Ҫ������ֵ</param>
    /// <param name="isDisplayUI">����������ʱ�Ƿ��������ʾ���棬Ĭ����true</param>
    /// <returns>��ǰ�����Ƿ�������ɲ�����true���������㹻</returns>
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
    /// ������ҵ������������������������
    /// </summary>
    /// <param name="increase">����0��������ʾҪ���ӵ�����ֵ</param>
    public void IncreaseActionPoint(float increase)
    {
        ChangeActionPoint(increase);
    }

    /// <summary>
    /// ������ҵ������������������򲻻��������ֵ���ڵ��ø÷���֮ǰӦ����CheckForEnoughPoint���������Ƿ��㹻���ж�
    /// </summary>
    /// <param name="decrease">����0��������ʾҪ���ٵ�����ֵ</param>
    public void DecreaseActionPoint(float decrease)
    {
        ChangeActionPoint(-decrease);
    }

    /// <summary>
    /// �ı����������ͳһ�ӿڡ�
    /// </summary>
    /// <param name="increase">Ҫ�ı������ֵ����increase����0ʱ��������Ӧ������ֵ���������������ֵ���ޣ���increaseС��0ʱ��������Ӧ������ֵ</param>
    private void ChangeActionPoint(float increase)
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
    /// ����UI��ʾ�������ֵ����
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
