using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthPanelEnemy : MonoBehaviour
{
    public static HealthPanelEnemy Instance;

    private GameObject _localPlayer;
    private GameObject _enemyPlayer;
    private GameObject _apBar;

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
        _apBar = transform.parent.Find("APPanel").GetChild(0).gameObject;
        float currentAP = _localPlayer.GetComponent<PlayerActionPoint>().CurrentActionPoint;
        float maxAP = _localPlayer.GetComponent<PlayerActionPoint>().MaxActionPoint;
        UpdateActionPoint(currentAP, maxAP);
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

            float currentAP = _localPlayer.GetComponent<PlayerActionPoint>().CurrentActionPoint;
            float maxAP = _localPlayer.GetComponent<PlayerActionPoint>().MaxActionPoint;
            UpdateActionPoint(currentAP - costAP, maxAP);
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

            float currentAP = _localPlayer.GetComponent<PlayerActionPoint>().CurrentActionPoint;
            float maxAP = _localPlayer.GetComponent<PlayerActionPoint>().MaxActionPoint;
            UpdateActionPoint(currentAP - costAP, maxAP);
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

            float currentAP = _localPlayer.GetComponent<PlayerActionPoint>().CurrentActionPoint;
            float maxAP = _localPlayer.GetComponent<PlayerActionPoint>().MaxActionPoint;
            UpdateActionPoint(currentAP - costAP, maxAP);
        }
    }

    /// <summary>
    /// ���� AP ���ĳ��Ⱥ���ʾ����ֵ
    /// </summary>
    /// <param name="currentAP">��ǰ AP ֵ</param>
    /// <param name="maxAP">��� AP ֵ</param>
    public void UpdateActionPoint(float currentAP, float maxAP)
    {
        // ���� AP ������
        RectTransform apBarRect = _apBar.GetComponent<RectTransform>();
        _apBar.GetComponent<UnityEngine.UI.Image>().fillAmount = currentAP / maxAP;

        // ���� AP ���ı�
        Transform valueText = _apBar.transform.Find("Value");
        if (valueText != null)
        {
            valueText.GetComponent<TextMeshProUGUI>().text = currentAP.ToString() + "\n /\n20";
        }
    }
}
