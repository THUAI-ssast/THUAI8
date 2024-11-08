using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 左下角玩家信息面板UI
/// </summary>
public class PlayerInfoUI : MonoBehaviour
{
    /// <summary>
    /// 信息面板中的体力值部分
    /// </summary>
    [SerializeField] private GameObject APPanel;
    /// <summary>
    /// 信息面板中的头部血量部分
    /// </summary>
    [SerializeField] private GameObject HeadHPPanel;
    /// <summary>
    /// 信息面板中的身体血量部分
    /// </summary>
    [SerializeField] private GameObject BodyHPPanel;
    /// <summary>
    /// 信息面板中的腿部血量部分
    /// </summary>
    [SerializeField] private GameObject LegHPPanel;

    void Start()
    {
        // 显示玩家名字
        transform.Find("NameText").gameObject.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("Name");
    }

    /// <summary>
    /// 更新信息面板中的体力值部分
    /// </summary>
    /// <param name="newActionPoint">更新后的体力值</param>
    /// <param name="maxActionPoint">体力值上限</param>
    public void UpdateActionPoint(float newActionPoint, float maxActionPoint)
    {  
        APPanel.transform.Find("Bar").GetComponent<UnityEngine.UI.Image>().fillAmount = newActionPoint/ maxActionPoint;
        Debug.Log(newActionPoint / maxActionPoint);
        APPanel.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = newActionPoint.ToString();
    }

    /// <summary>
    /// 更新信息面板中的血量部分，包括头部、身体和腿部
    /// </summary>
    /// <param name="newHealth">更新后的血量</param>
    /// <param name="maxHealth">血量上限</param>
    /// <param name="bodyPart">BodyPart类型的枚举变量，用于指定更新的血量属于头部、身体和腿部中的哪一个</param>
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
/// 身体部位枚举类
/// </summary>
public enum BodyPart
{
    Head,
    Body,
    Leg
}
