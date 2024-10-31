using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ƥ��׶ε����Ԥ���壬��StartScene�����ɣ����ڴ�����ƥ��׶���ҿͻ��������˵�ͨ���¼�
/// </summary>
public class NetPlayer : NetworkRoomPlayer
{
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        MainMenu.Instance.Player = this;
    }


    /// <summary>
    /// ֪ͨ�������ҿ�ʼƥ�䣬ͬʱ��¼�йظ���ҵ���Ϣ
    /// </summary>
    /// <param name="playerName">��ҵ�����</param>
    public void StartMatching(string playerName)
    {
        // ֪ͨ����˸������׼��
        CmdChangeReadyState(true);
        // ��¼����ҵ�����
        PlayerPrefs.SetString("Name", playerName);  
    } 

    /// <summary>
    /// ֪ͨ����˸����ȡ��ƥ��
    /// </summary>
    public void CancelMatching()
    {
        // ֪ͨ����˸����ȡ��׼��
        CmdChangeReadyState(false); 
    }

}
