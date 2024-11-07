using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// ���½������Ϣ���UI
/// </summary>
public class PlayerInfoUI : MonoBehaviour
{
    /// <summary>
    /// ��Ϣ����е�����ֵ����
    /// </summary>
    [SerializeField] private GameObject APPanel;
    /// <summary>
    /// ��Ϣ����е�ͷ��Ѫ������
    /// </summary>
    [SerializeField] private GameObject HeadHPPanel;
    /// <summary>
    /// ��Ϣ����е�����Ѫ������
    /// </summary>
    [SerializeField] private GameObject BodyHPPanel;
    /// <summary>
    /// ��Ϣ����е��Ȳ�Ѫ������
    /// </summary>
    [SerializeField] private GameObject LegHPPanel;

    // Start is called before the first frame update
    void Start()
    {
        // ��ʾ�������
        transform.Find("NameText").gameObject.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("Name");
    }

    /// <summary>
    /// ������Ϣ����е�����ֵ����
    /// </summary>
    /// <param name="newActionPoint">���º������ֵ</param>
    /// <param name="maxActionPoint">����ֵ����</param>
    public void UpdateActionPoint(float newActionPoint, float maxActionPoint)
    {  
        APPanel.transform.Find("Bar").GetComponent<UnityEngine.UI.Image>().fillAmount = newActionPoint/ maxActionPoint;
        Debug.Log(newActionPoint / maxActionPoint);
        APPanel.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = newActionPoint.ToString();
    }

    /// <summary>
    /// ������Ϣ����е�Ѫ�����֣�����ͷ����������Ȳ�
    /// </summary>
    /// <param name="newHealth">���º��Ѫ��</param>
    /// <param name="maxHealth">Ѫ������</param>
    /// <param name="bodyPart">BodyPart���͵�ö�ٱ���������ָ�����µ�Ѫ������ͷ����������Ȳ��е���һ��</param>
    public void UpdateHealthPoint(float newHealth, float maxHealth, BodyPart bodyPart)
    {
        GameObject HPPanel;
        switch (bodyPart)
        {
            case BodyPart.Head:
                HPPanel = HeadHPPanel;
                break;
            case BodyPart.Body:
                HPPanel = BodyHPPanel;
                break;
            case BodyPart.Leg:
                HPPanel = LegHPPanel;
                break;
            default:
                return;
        }
        HPPanel.transform.Find("Bar").GetComponent<UnityEngine.UI.Image>().fillAmount = newHealth / maxHealth;
        HPPanel.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = newHealth.ToString();
    }
}

/// <summary>
/// ���岿λö����
/// </summary>
public enum BodyPart
{
    Head,
    Body,
    Leg
}
