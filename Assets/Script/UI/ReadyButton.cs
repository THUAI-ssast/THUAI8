using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Basic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI行为类，界面右下角世界回合准备按钮的行为。
/// </summary>
public class ReadyButton : MonoBehaviour
{
    /// <summary>
    /// 初始化准备按钮，初始为浅蓝色。
    /// </summary>
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        gameObject.GetComponent<Image>().color= new Color32(203, 255, 252, 255); // blue
    }
    /// <summary>
    /// 点击准备按钮的行为。循环： 未准备 -> 准备中 -> 已准备 -> 未准备 。
    /// </summary>
    public void OnButtonClick()
    {
        if(RoundManager.Instance.State == RoundManager.RoundState.NotReady)
        {   
            RoundManager.Instance.State = RoundManager.RoundState.PreReady;
            gameObject.GetComponent<Image>().color = new Color32(181, 242, 139, 255); // green
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 18;
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "确认\n结束";
            return ;
        }
        if(RoundManager.Instance.State == RoundManager.RoundState.PreReady)
        {
            RoundManager.Instance.State = RoundManager.RoundState.Ready;
            gameObject.GetComponent<Image>().color = new Color32(255, 217, 103, 255); // yellow
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "等待\n其他玩家";
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            player.GetComponent<PlayerRound>().CmdReady(true);
            return ;
        }
        if(RoundManager.Instance.State == RoundManager.RoundState.Ready)
        {
            RoundManager.Instance.State = RoundManager.RoundState.NotReady;
            gameObject.GetComponent<Image>().color = new Color32(203, 255, 252, 255); // blue
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 18;
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "结束\n回合";
            GameObject player = GameObject.FindWithTag("LocalPlayer");
            player.GetComponent<PlayerRound>().CmdReady(false);
            return ;
        }
    }
}
