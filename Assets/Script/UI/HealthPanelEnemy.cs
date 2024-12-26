using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// UI行为/数据类，管理战斗UI内的敌人显示，以及对应的攻击触发
/// </summary>
public class HealthPanelEnemy : MonoBehaviour
{
    public static HealthPanelEnemy Instance;
    public bool IfStart = true;

    private GameObject _localPlayer;
    private GameObject _enemyPlayer;
    private GameObject _apBar;

    public GameObject LocalPlayer
    {
        get => _localPlayer;
        private set => _localPlayer = value;
    }

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
        IfStart = false;
        // _enemyPlayer = GameObject.FindWithTag("Player");
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
            float costAP = (UIManager.Instance.FollowImage.ItemData as WeaponItemData).AttakAPCost;
            string itemName = UIManager.Instance.FollowImage.ItemData.ItemName;
            PlayerHealth enemyPlayerHealth = _enemyPlayer.GetComponent<PlayerHealth>();
            PlayerHealth localPlayerHealth = _localPlayer.GetComponent<PlayerHealth>();
            string enemyName = enemyPlayerHealth.Name;
            string localName = localPlayerHealth.Name;
            float damage = enemyPlayerHealth.GetWeaponDamage(PlayerHealth.BodyPosition.Head, UIManager.Instance.FollowImage);
            string message = $"{localName}消耗{costAP}AP使用{itemName}攻击了{enemyName}的头部，造成了{damage}HP伤害。";
            string enemyDeadMsg = $"被{localName}用{itemName}攻击了头部…";
            _localPlayer.GetComponent<PlayerLog>().CmdAddLog(_enemyPlayer, enemyDeadMsg, LogInfo.DamageType.fight);
            UIManager.Instance.DestroyCurrentFollowImage();
            _localPlayer.GetComponent<PlayerFight>().CmdAttackHappened(message, costAP);

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
            float costAP = (UIManager.Instance.FollowImage.ItemData as WeaponItemData).AttakAPCost;
            string itemName = UIManager.Instance.FollowImage.ItemData.ItemName;
            PlayerHealth enemyPlayerHealth = _enemyPlayer.GetComponent<PlayerHealth>();
            PlayerHealth localPlayerHealth = _localPlayer.GetComponent<PlayerHealth>();
            string enemyName = enemyPlayerHealth.Name;
            string localName = localPlayerHealth.Name;
            float damage = enemyPlayerHealth.GetWeaponDamage(PlayerHealth.BodyPosition.MainBody, UIManager.Instance.FollowImage);
            string message = $"{localName}消耗{costAP}AP使用{itemName}攻击了{enemyName}的躯干，造成了{damage}HP伤害。";
            string enemyDeadMsg = $"被{localName}用{itemName}攻击了躯干…";
            _localPlayer.GetComponent<PlayerLog>().CmdAddLog(_enemyPlayer, enemyDeadMsg, LogInfo.DamageType.fight);
            UIManager.Instance.DestroyCurrentFollowImage();
            _localPlayer.GetComponent<PlayerFight>().CmdAttackHappened(message, costAP);

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
            float costAP = (UIManager.Instance.FollowImage.ItemData as WeaponItemData).AttakAPCost;
            string itemName = UIManager.Instance.FollowImage.ItemData.ItemName;
            PlayerHealth enemyPlayerHealth = _enemyPlayer.GetComponent<PlayerHealth>();
            PlayerHealth localPlayerHealth = _localPlayer.GetComponent<PlayerHealth>();
            string enemyName = enemyPlayerHealth.Name;
            string localName = localPlayerHealth.Name;
            float damage = enemyPlayerHealth.GetWeaponDamage(PlayerHealth.BodyPosition.Legs, UIManager.Instance.FollowImage);
            string message = $"{localName}消耗{costAP}AP使用{itemName}攻击了{enemyName}的腿部，造成了{damage}HP伤害。";
            string enemyDeadMsg = $"被{localName}用{itemName}攻击了腿部…";
            _localPlayer.GetComponent<PlayerLog>().CmdAddLog(_enemyPlayer, enemyDeadMsg, LogInfo.DamageType.fight);
            UIManager.Instance.DestroyCurrentFollowImage();
            _localPlayer.GetComponent<PlayerFight>().CmdAttackHappened(message, costAP);

            float currentAP = _localPlayer.GetComponent<PlayerActionPoint>().CurrentActionPoint;
            float maxAP = _localPlayer.GetComponent<PlayerActionPoint>().MaxActionPoint;
            UpdateActionPoint(currentAP - costAP, maxAP);
        }
    }

    /// <summary>
    /// 更新 AP 条的长度和显示的数值
    /// </summary>
    /// <param name="currentAP">当前 AP 值 ֵ</param>
    /// <param name="maxAP">最大 AP 值ֵ</param>
    public void UpdateActionPoint(float currentAP, float maxAP)
    {
        // AP条显示
        RectTransform apBarRect = _apBar.GetComponent<RectTransform>();
        _apBar.GetComponent<UnityEngine.UI.Image>().fillAmount = currentAP / maxAP;

        // AP文字显示
        Transform valueText = _apBar.transform.Find("Value");
        if (valueText != null)
        {
            valueText.GetComponent<TextMeshProUGUI>().text = currentAP.ToString("0.0") + "\n /\n20";
        }
    }
    public void SetEnemy(GameObject enemyPlayer)
    {
        _enemyPlayer = enemyPlayer;
    }
}
