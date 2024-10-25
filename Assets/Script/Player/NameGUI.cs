using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������ʾ�����BattleScene�е����� 
/// </summary>
public class NameGUI : NetworkBehaviour
{
    /// <summary>
    /// ��Ӧ�����Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// ��������λ�õ�ƫ���������ڵ���������ֵ���ʾλ��
    /// </summary>
    public Vector3 Offset;


    // Start is called before the first frame update 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() 
    {
        gameObject.GetComponent<TMP_Text>().text = TargetPlayer.gameObject.GetComponent<PlayerHealth>().Name;
        transform.position = TargetPlayer.transform.position + Offset;
        transform.rotation = Quaternion.identity;
    }
}
