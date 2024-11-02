using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject APPanel;
    [SerializeField] private GameObject HeadHPPanel;
    [SerializeField] private GameObject BodyHPPanel;
    [SerializeField] private GameObject LegHPPanel;

    // Start is called before the first frame update
    void Start()
    {
        // 显示玩家名字
        transform.Find("NameText").gameObject.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("Name");
    }

    public void UpdateActionPoint(float newActionPoint, float maxActionPoint)
    {
        APPanel.transform.Find("Bar").GetComponent<UnityEngine.UI.Image>().fillAmount = newActionPoint/ maxActionPoint;
        Debug.Log(newActionPoint / maxActionPoint);
        APPanel.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = newActionPoint.ToString();
    }

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
