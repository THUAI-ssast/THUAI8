using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Basic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    /// <summary>
    /// 准备按钮的状态, 0为未准备，1为中间状态（进行准备/取消准备），2为已准备
    /// </summary>
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        gameObject.GetComponent<Image>().color= new Color32(203, 255, 252, 255); // blue
    }
    public void OnButtonClick()
    {
        Debug.Log("OnClick.");
        if(RoundManager.Instance.State == RoundManager.RoundState.NotReady)
        {   
            RoundManager.Instance.State = RoundManager.RoundState.PreReady;
            gameObject.GetComponent<Image>().color = new Color32(181, 242, 139, 255); // green
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 20;
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "准备\n结束";
            return ;
        }
        if(RoundManager.Instance.State == RoundManager.RoundState.PreReady)
        {
            if(RoundManager.Instance.IsReady == false)
            {
                RoundManager.Instance.State = RoundManager.RoundState.Ready;
                gameObject.GetComponent<Image>().color = new Color32(255, 217, 103, 255); // yellow
                gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "等待\n其他玩家";
                gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 14;
                GameObject player = GameObject.FindWithTag("LocalPlayer");
                player.GetComponent<PlayerRound>().CmdReady(true);
                return ;
            }
            else
            {
                RoundManager.Instance.State = RoundManager.RoundState.NotReady;
                gameObject.GetComponent<Image>().color = new Color32(203, 255, 252, 255); // blue
                gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 20;
                GameObject player = GameObject.FindWithTag("LocalPlayer");
                player.GetComponent<PlayerRound>().CmdReady(false);
                return ;
            }
        }
        if(RoundManager.Instance.State == RoundManager.RoundState.Ready)
        {
            RoundManager.Instance.State = RoundManager.RoundState.PreReady;
            gameObject.GetComponent<Image>().color = new Color32(181, 242, 139, 255); // green
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 20;
            gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "取消\n准备";
            return ;
        }
    }
}
