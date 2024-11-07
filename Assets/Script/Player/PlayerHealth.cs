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
    [SyncVar] private string _name;

    public string Name { get => _name; }
    
    /// <summary>
    /// ��ҵ���Ѫ������
    /// </summary>
    public float TotalMaxHealth { get => _headMaxHealth + _bodyMaxHealth + _legMaxHealth; }
    /// <summary>
    /// ��ҵ�ͷ��Ѫ������
    /// </summary>
    private float _headMaxHealth = 10;
    /// <summary>
    /// ��ҵ�����Ѫ������
    /// </summary>
    private float _bodyMaxHealth = 10;
    /// <summary>
    /// ��ҵ��Ȳ�Ѫ������
    /// </summary>
    private float _legMaxHealth = 10;

    public float HeadMaxHealth { get => _headMaxHealth; }
    public float BodyMaxHealth { get => _bodyMaxHealth; }
    public float LegMaxHealth { get => _legMaxHealth; }

    /// <summary>
    /// ��ҵĵ�ǰ��Ѫ��
    /// </summary>
    public float TotalHealth { get => _headHealth + _bodyHealth + _legHealth; }
    /// <summary>
    /// ��ҵĵ�ǰͷ��Ѫ�����仯ʱ�����HeadHealthChange����
    /// </summary>
    [SyncVar(hook = nameof(HeadHealthChange))] private float _headHealth = 10;
    /// <summary>
    /// ��ҵĵ�ǰ����Ѫ�����仯ʱ�����BodyHealthChange����
    /// </summary>
    [SyncVar(hook = nameof(BodyHealthChange))] private float _bodyHealth = 10;
    /// <summary>
    /// ��ҵĵ�ǰ�Ȳ�Ѫ�����仯ʱ�����LegHealthChange����
    /// </summary>
    [SyncVar(hook = nameof(LegHealthChange))] private float _legHealth = 10;

    public float HeadHealth { get => _headHealth; }
    public float BodyHealth { get => _bodyHealth; }
    public float LegHealth { get => _legHealth; }

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
    private PlayerInfoUI LocalPlayerInfoPanel;

    // Start is called before the first frame update 
    void Start()
    {
        Debug.Log(TargetPlayer.gameObject.tag);
        if (isLocalPlayer) 
        {
            // ��ȡ�������
            CmdSetName(PlayerPrefs.GetString("Name"));
            // ��ȡ���������Ϣ���
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject.GetComponent<PlayerInfoUI>();
            LocalPlayerInfoPanel.UpdateHealthPoint(_headHealth, _headMaxHealth, BodyPart.Head);
            LocalPlayerInfoPanel.UpdateHealthPoint(_bodyHealth, _bodyMaxHealth, BodyPart.Body);
            LocalPlayerInfoPanel.UpdateHealthPoint(_legHealth, _legMaxHealth, BodyPart.Leg);
        }
        // �����ʼѪ��
    }


    /// <summary>
    /// hook��������HeadHealth�ı���Զ�������
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void HeadHealthChange(float oldHealth, float newHealth)
    {
        // �ҵ���Ӧ��Ѫ��������ʾ
        HeadHealthGUI.UpdateHealthGUILength(_headMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _headMaxHealth, BodyPart.Head);
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
        BodyHealthGUI.UpdateHealthGUILength(_bodyMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _bodyMaxHealth, BodyPart.Body);
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
        LegHealthGUI.UpdateHealthGUILength(_legMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _legMaxHealth, BodyPart.Leg);
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
        _name = name;
    }

}
