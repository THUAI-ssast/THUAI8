using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// �����б�������ҵ����֡�Ѫ�����޺͵�ǰѪ�����Լ�����Ѫ���仯�߼��ķ���
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    /// <summary>
    /// ��ҵ�����
    /// </summary>
    [SyncVar] public string Name;
    
    /// <summary>
    /// ��ҵ���Ѫ������
    /// </summary>
    public float TotalMaxHealth { get => HeadMaxHealth + BodyMaxHealth + LegMaxHealth; }
    /// <summary>
    /// ��ҵ�ͷ��Ѫ������
    /// </summary>
    public float HeadMaxHealth;
    /// <summary>
    /// ��ҵ�����Ѫ������
    /// </summary>
    public float BodyMaxHealth;
    /// <summary>
    /// ��ҵ��Ȳ�Ѫ������
    /// </summary>
    public float LegMaxHealth;

    /// <summary>
    /// ��ҵĵ�ǰ��Ѫ��
    /// </summary>
    public float TotalHealth { get => HeadHealth + BodyHealth + LegHealth; }
    /// <summary>
    /// ��ҵĵ�ǰͷ��Ѫ�����仯ʱ�����HeadHealthChange����
    /// </summary>
    [SyncVar(hook = nameof(HeadHealthChange))] public float HeadHealth;
    /// <summary>
    /// ��ҵĵ�ǰ����Ѫ�����仯ʱ�����BodyHealthChange����
    /// </summary>
    [SyncVar(hook = nameof(BodyHealthChange))] public float BodyHealth;
    /// <summary>
    /// ��ҵĵ�ǰ�Ȳ�Ѫ�����仯ʱ�����LegHealthChange����
    /// </summary>
    [SyncVar(hook = nameof(LegHealthChange))] public float LegHealth;

    /// <summary>
    /// ��Ӧ����Ҷ���Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// ��ҵ�ͷ��Ѫ��UI
    /// </summary>
    public HealthGUI HeadHealthGUI;
    /// <summary>
    /// ��ҵ�����Ѫ��UI
    /// </summary>
    public HealthGUI BodyHealthGUI;
    /// <summary>
    /// ��ҵ��Ȳ�Ѫ��UI
    /// </summary>
    public HealthGUI LegHealthGUI;

    /// <summary>
    /// λ�����½ǵĿͻ��������Ϣ���
    /// </summary>
    public PlayerInfoUI LocalPlayerInfoPanel;

    // Start is called before the first frame update 
    void Start()
    {
        if (isLocalPlayer) 
        {
            // ��ȡ�������
            CmdSetName(PlayerPrefs.GetString("Name"));
            // ��ȡ���������Ϣ���
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject.GetComponent<PlayerInfoUI>();
        }
        // �����ʼѪ��
        HeadHealth = BodyHealth = LegHealth = HeadMaxHealth = BodyMaxHealth = LegMaxHealth = 10;
    }


    /// <summary>
    /// hook��������HeadHealth�ı���Զ�������
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void HeadHealthChange(float oldHealth, float newHealth)
    {
        // �ҵ���Ӧ��Ѫ��������ʾ
        HeadHealthGUI.UpdateHealthGUILength(HeadMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, HeadMaxHealth, BodyPart.Head);
        }
    }

    /// <summary>
    /// hook��������BodyHealth�ı���Զ�������
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void BodyHealthChange(float oldHealth, float newHealth)
    {
        // �ҵ���Ӧ��Ѫ��������ʾ
        BodyHealthGUI.UpdateHealthGUILength(BodyMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, BodyMaxHealth, BodyPart.Body);
        }
    }

    /// <summary>
    /// hook��������LegHealth�ı���Զ�������
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void LegHealthChange(float oldHealth, float newHealth)
    {
        // �ҵ���Ӧ��Ѫ��������ʾ
        LegHealthGUI.UpdateHealthGUILength(LegMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, LegMaxHealth, BodyPart.Leg);
        }
    }

    /// <summary>
    /// Command�������ڿͻ��˱����ã����ڷ����ִ�С�
    /// ������ͬ����ҵ�����
    /// </summary>
    /// <param name="name">��ҵ�����</param>
    [Command]
    private void CmdSetName(string name)
    {
        Name = name;
    }

}
