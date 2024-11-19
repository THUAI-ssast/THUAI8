using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoDamage : MonoBehaviour
{
    public static DoDamage Instance;

    private GameObject _localPlayer;
    private GameObject _enemyPlayer;

    public GameObject EnemyPlayer
    {
        get => _enemyPlayer;
        private set => _enemyPlayer = value;
    }

    private SlotMenuTrigger _slotMenuTrigger;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _localPlayer = GameObject.FindWithTag("LocalPlayer");
        _enemyPlayer = GameObject.FindWithTag("Player");
    }

    public void OnClickHead()
    {
        if (UIManager.Instance.FollowImage != null)
        {
            _localPlayer.GetComponent<PlayerHealth>().CmdAttack(_localPlayer, _enemyPlayer, (int)PlayerHealth.BodyPosition.Head, UIManager.Instance.FollowImage.gameObject);
            UIManager.Instance.DestroyCurrentFollowImage();
            float costAP = (UIManager.Instance.FollowImage.ItemData as WeaponItemData).AttakAPCost;
            string itemName = UIManager.Instance.FollowImage.ItemData.ItemName;
            PlayerHealth enemyPlayerHealth = EnemyPlayer.GetComponent<PlayerHealth>();
            string enemyName = enemyPlayerHealth.Name;
            float damage = enemyPlayerHealth.GetWeaponDamage(PlayerHealth.BodyPosition.Head, UIManager.Instance.FollowImage);
            string message = $"��ǰ�غϣ�������{costAP}APʹ��{itemName}������{enemyName}��ͷ���������{damage}HP�˺���";
            BattleLogManager.Instance.AddLog(message);
        }
    }

    public void OnClickBody()
    {
        if (UIManager.Instance.FollowImage != null)
        {
            _localPlayer.GetComponent<PlayerHealth>().CmdAttack(_localPlayer, _enemyPlayer, (int)PlayerHealth.BodyPosition.MainBody, UIManager.Instance.FollowImage.gameObject);
            UIManager.Instance.DestroyCurrentFollowImage();
            float costAP = (UIManager.Instance.FollowImage.ItemData as WeaponItemData).AttakAPCost;
            string itemName = UIManager.Instance.FollowImage.ItemData.ItemName;
            PlayerHealth enemyPlayerHealth = EnemyPlayer.GetComponent<PlayerHealth>();
            string enemyName = enemyPlayerHealth.Name;
            float damage = enemyPlayerHealth.GetWeaponDamage(PlayerHealth.BodyPosition.MainBody, UIManager.Instance.FollowImage);
            string message = $"��ǰ�غϣ�������{costAP}APʹ��{itemName}������{enemyName}�����ɣ������{damage}HP�˺���";
            BattleLogManager.Instance.AddLog(message);
        }
    }

    public void OnClickLegs()
    {
        if (UIManager.Instance.FollowImage != null)
        {
            _localPlayer.GetComponent<PlayerHealth>().CmdAttack(_localPlayer, _enemyPlayer, (int)PlayerHealth.BodyPosition.Legs, UIManager.Instance.FollowImage.gameObject);
            UIManager.Instance.DestroyCurrentFollowImage();
            float costAP = (UIManager.Instance.FollowImage.ItemData as WeaponItemData).AttakAPCost;
            string itemName = UIManager.Instance.FollowImage.ItemData.ItemName;
            PlayerHealth enemyPlayerHealth = EnemyPlayer.GetComponent<PlayerHealth>();
            string enemyName = enemyPlayerHealth.Name;
            float damage = enemyPlayerHealth.GetWeaponDamage(PlayerHealth.BodyPosition.Legs, UIManager.Instance.FollowImage);
            string message = $"��ǰ�غϣ�������{costAP}APʹ��{itemName}������{enemyName}���Ȳ��������{damage}HP�˺���";
            BattleLogManager.Instance.AddLog(message);
        }
    }
}
