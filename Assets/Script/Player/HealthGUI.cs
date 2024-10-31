using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// ������ʾ�͸��������BattleScene�е�Ѫ��
/// </summary>
public class HealthGUI : MonoBehaviour
{
    /// <summary>
    /// ��Ӧ�����Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// ��������λ�õ�ƫ���������ڵ������Ѫ������ʾλ��
    /// </summary>
    public Vector3 Offset;


    // Start is called before the first frame update
    void Start()
    {
        transform.position = TargetPlayer.transform.position + Offset;
    }


    /// <summary>
    /// �������Ѫ���ĳ��ȣ������Ѫ������ʱ���Զ�����
    /// </summary>
    /// <param name="totalHealth"></param>
    /// <param name="newHealth"></param>
    public void UpdateHealthGUILength(float totalHealth, float newHealth)
    {
        gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount = newHealth/totalHealth;
    }
}
