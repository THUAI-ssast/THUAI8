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
            string message = $"当前回合：你消耗{costAP}AP使用{itemName}攻击了{enemyName}的头部，造成了{damage}HP伤害。";
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
            string message = $"当前回合：你消耗{costAP}AP使用{itemName}攻击了{enemyName}的躯干，造成了{damage}HP伤害。";
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
            string message = $"当前回合：你消耗{costAP}AP使用{itemName}攻击了{enemyName}的腿部，造成了{damage}HP伤害。";
            BattleLogManager.Instance.AddLog(message);

            float currentAP = _localPlayer.GetComponent<PlayerActionPoint>().CurrentActionPoint;
            float maxAP = _localPlayer.GetComponent<PlayerActionPoint>().MaxActionPoint;
            UpdateActionPoint(currentAP - costAP, maxAP);
        }
    }

    /// <summary>
    /// 更新 AP 条的长度和显示的数值
    /// </summary>
    /// <param name="currentAP">当前 AP 值</param>
    /// <param name="maxAP">最大 AP 值</param>
    public void UpdateActionPoint(float currentAP, float maxAP)
    {
        // 更新 AP 条长度
        RectTransform apBarRect = _apBar.GetComponent<RectTransform>();
        _apBar.GetComponent<UnityEngine.UI.Image>().fillAmount = currentAP / maxAP;

        // 更新 AP 条文本
        Transform valueText = _apBar.transform.Find("Value");
        if (valueText != null)
        {
            valueText.GetComponent<TextMeshProUGUI>().text = currentAP.ToString() + "\n /\n20";
        }
    }
}
