using System;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// 玩家数据类，保存了玩家的名字、血量上限和当前血量，以及处理血量变化逻辑的方法
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    /// <summary>
    /// 用于玩家死亡后创建资源点
    /// </summary>
    private Tilemap _furnitureTilemap;

    /// <summary>
    /// 玩家是否存活
    /// </summary>
    public bool IsAlive => _isAlive;
    [SyncVar] bool _isAlive;

    /// <summary>
    /// 玩家的名字
    /// </summary>
    [SyncVar] private string _name;
    /// <summary>
    /// 玩家的名字，会显示在角色下方
    /// </summary>
    public string Name => _name;
    /// <summary>
    /// 玩家健康的部位
    /// </summary>
    public enum BodyPosition
    {
        Head,
        MainBody,
        Legs
    }
    /// <summary>
    /// 将部位信息转换成中文string的字典
    /// </summary>
    public static Dictionary<BodyPosition, string> BodyToChinese = new Dictionary<BodyPosition, string>()
    {
        { BodyPosition.Head, "头部" },
        { BodyPosition.MainBody, "躯干" },
        { BodyPosition.Legs, "腿部" }
    };
    /// <summary>
    /// 玩家护甲信息，记录了每个部位的装备情况
    /// </summary>
    private Dictionary<BodyPosition, Item> _armorEquipments = new Dictionary<BodyPosition, Item>();
    /// <summary>
    /// 获取玩家对应部位的护甲Item
    /// </summary>
    /// <param name="position">部位信息</param>
    /// <returns>玩家对应部位的护甲Item，若无护甲则为null</returns>
    public Item GetItemAt(BodyPosition position)
    {
        if (_armorEquipments.TryGetValue(position, out var armorItem))
        {
            if (armorItem.CurrentDurability <= 0)
                return null;
            return armorItem;
        }

        return null;
    }
    /// <summary>
    /// 卸下原有护甲，装备护甲到对应部位
    /// </summary>
    /// <param name="position">角色部位</param>
    /// <param name="armorItem">要装备的护甲Item</param>
    /// <returns>此部位原有的护甲，若无则为null</returns>
    public Item EquipArmor(BodyPosition position, Item armorItem)
    {
        Item oldArmor = null;
        if (armorItem.ItemData is ArmorItemData armorData && armorData.EquipBodyPosition == position)
        {
            if (_armorEquipments.TryGetValue(position, out var equipment))
            {
                oldArmor = equipment;
            }

            CmdChangeArmorEquipments((int)position, armorItem.gameObject);
        }

        return oldArmor;
    }
    /// <summary>
    /// 卸下对应部位的装备
    /// </summary>
    /// <param name="position">角色部位</param>
    /// <returns>此部位原有的护甲</returns>
    public Item UnEquipArmor(BodyPosition position)
    {
        Item oldArmor = null;
        if (_armorEquipments.TryGetValue(position, out var equipment))
        {
            oldArmor = equipment;
            CmdRemoveArmorEquipments((int)position);
        }

        return oldArmor;
    }

    [Command]
    private void CmdChangeArmorEquipments(int position, GameObject armorObject)
    {
        _armorEquipments[(BodyPosition)position] = armorObject.GetComponent<Item>();
        RpcChangeArmorEquipments(position, armorObject);
    }

    [ClientRpc]
    private void RpcChangeArmorEquipments(int position, GameObject armorObject)
    {
        _armorEquipments[(BodyPosition)position] = armorObject.GetComponent<Item>();
        if (isLocalPlayer)
        {
            BackpackManager.Instance.RefreshArmorDisplay();
        }
    }

    [Command]
    private void CmdRemoveArmorEquipments(int position)
    {
        _armorEquipments.Remove((BodyPosition)position);
        RpcRemoveArmorEquipments(position);
    }

    [ClientRpc]
    private void RpcRemoveArmorEquipments(int position)
    {
        _armorEquipments.Remove((BodyPosition)position);
        if (isLocalPlayer)
        {
            BackpackManager.Instance.RefreshArmorDisplay();
        }
    }


    /// <summary>
    /// 玩家的总血量上限
    /// </summary>
    public float TotalMaxHealth
    {
        get => _headMaxHealth + _bodyMaxHealth + _legMaxHealth;
    }

    /// <summary>
    /// 玩家的头部血量上限
    /// </summary>
    private readonly float _headMaxHealth = 10;

    /// <summary>
    /// 玩家的身体血量上限
    /// </summary>
    private readonly float _bodyMaxHealth = 10;

    /// <summary>
    /// 玩家的腿部血量上限
    /// </summary>
    private readonly float _legMaxHealth = 10;
    /// <summary>
    /// 玩家头部血量上限
    /// </summary>
    public float HeadMaxHealth
    {
        get => _headMaxHealth;
    }
    /// <summary>
    /// 玩家躯干血量上限
    /// </summary>
    public float BodyMaxHealth
    {
        get => _bodyMaxHealth;
    }
    /// <summary>
    /// 玩家腿部血量上限
    /// </summary>
    public float LegMaxHealth
    {
        get => _legMaxHealth;
    }

    /// <summary>
    /// 玩家的当前总血量
    /// </summary>
    public float TotalHealth
    {
        get => _headHealth + _bodyHealth + _legHealth;
    }

    /// <summary>
    /// 玩家的当前头部血量，变化时会调用HeadHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(HeadHealthChange))]
    private float _headHealth = 10;

    /// <summary>
    /// 玩家的当前身体血量，变化时会调用BodyHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(BodyHealthChange))]
    private float _bodyHealth = 10;

    /// <summary>
    /// 玩家的当前腿部血量，变化时会调用LegHealthChange函数
    /// </summary>
    [SyncVar(hook = nameof(LegHealthChange))]
    private float _legHealth = 10;

    public float HeadHealth
    {
        get => _headHealth;
    }

    public float BodyHealth
    {
        get => _bodyHealth;
    }

    public float LegHealth
    {
        get => _legHealth;
    }

    /// <summary>
    /// 对应的玩家对象Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// 玩家的头部血条UI
    /// </summary>
    public HealthGUI HeadHealthGUI;

    /// <summary>
    /// 玩家的身体血条UI
    /// </summary>
    public HealthGUI BodyHealthGUI;

    /// <summary>
    /// 玩家的腿部血条UI
    /// </summary>
    public HealthGUI LegHealthGUI;

    /// <summary>
    /// 位于左下角的客户端玩家信息面板
    /// </summary>
    private PlayerInfoUI LocalPlayerInfoPanel;

    // Start is called before the first frame update 
    void Start()
    {
        if (isLocalPlayer)
        {
            // 获取玩家名字
            CmdSetName(PlayerPrefs.GetString("Name"));
            // 获取本地玩家信息面板
            LocalPlayerInfoPanel = UIManager.Instance.MainCanvas.transform.Find("PlayerInfoPanel").gameObject
                .GetComponent<PlayerInfoUI>();
            LocalPlayerInfoPanel.UpdateHealthPoint(_headHealth, _headMaxHealth, BodyPart.Head);
            LocalPlayerInfoPanel.UpdateHealthPoint(_bodyHealth, _bodyMaxHealth, BodyPart.Body);
            LocalPlayerInfoPanel.UpdateHealthPoint(_legHealth, _legMaxHealth, BodyPart.Leg);
        }

        _isAlive = true;
        _furnitureTilemap = GridMoveController.Instance.FurnitureTilemap;
    }


    /// <summary>
    /// hook函数，当HeadHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void HeadHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        HeadHealthGUI.UpdateHealthGUILength(_headMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _headMaxHealth, BodyPart.Head);
            BackpackManager.Instance.HeadHealthPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_headHealth}/{HeadMaxHealth}";
            
        }
        if(UIManager.Instance.BattlePanel.activeSelf)
        {
            if (isLocalPlayer)
            {
                BackpackManager.Instance.BattleHeadHealthPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_headHealth}/{HeadMaxHealth}";
            }
            else if (gameObject == HealthPanelEnemy.Instance.EnemyPlayer.gameObject)
            {
                BackpackManager.Instance.BattleHeadHealthEnemyPanel.GetChild(0).GetComponent<TMP_Text>().text =
                    $"{HeadHealth}/{HeadMaxHealth}";
            }
        }
    }

    /// <summary>
    /// hook函数，当BodyHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void BodyHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        BodyHealthGUI.UpdateHealthGUILength(_bodyMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _bodyMaxHealth, BodyPart.Body);
            BackpackManager.Instance.BodyHealthPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_bodyHealth}/{BodyMaxHealth}";
        }
        if(UIManager.Instance.BattlePanel.activeSelf)
        {
            if (isLocalPlayer)
            {   
                BackpackManager.Instance.BattleBodyHealthPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{BodyHealth}/{BodyMaxHealth}";
            }
            else if (gameObject == HealthPanelEnemy.Instance.EnemyPlayer.gameObject)
            {
                BackpackManager.Instance.BattleBodyHealthEnemyPanel.GetChild(0).GetComponent<TMP_Text>().text =
                    $"{BodyHealth}/{BodyMaxHealth}";
            }
        }
    }

    /// <summary>
    /// hook函数，当LegHealth改变后自动被调用
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    public void LegHealthChange(float oldHealth, float newHealth)
    {
        // 找到对应的血条更新显示
        LegHealthGUI.UpdateHealthGUILength(_legMaxHealth, newHealth);
        if (isLocalPlayer)
        {
            LocalPlayerInfoPanel.UpdateHealthPoint(newHealth, _legMaxHealth, BodyPart.Leg);
            BackpackManager.Instance.LegsHealthPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_legHealth}/{LegMaxHealth}";
            
        }
        if(UIManager.Instance.BattlePanel.activeSelf)
        {
            if (isLocalPlayer)
            {
                BackpackManager.Instance.BattleLegsHealthPanel.GetChild(0).GetComponent<TMP_Text>().text =
                    $"{_legHealth}/{LegMaxHealth}";
            }
            else if (gameObject == HealthPanelEnemy.Instance.EnemyPlayer.gameObject)
            {
                BackpackManager.Instance.BattleLegsHealthEnemyPanel.GetChild(0).GetComponent<TMP_Text>().text =
                    $"{LegHealth}/{LegMaxHealth}";
            }
        }
    }

    /// <summary>
    /// 供外部调用的攻击函数，攻击方消耗体力用指定武器攻击目标的某一部位，扣除对应的血量(保留2位小数)和武器/防具耐久度；
    /// 若体力不足则不会扣除血量并提示体力不足
    /// </summary>
    /// <param name="attacker">攻击的发起方，会扣除对应的体力</param>
    /// <param name="target">被攻击的目标</param>
    /// <param name="position">被攻击的部位，会扣除对应部位血量和防具耐久</param>
    /// <param name="weapon">攻击所使用的武器，会扣除对应的武器耐久度</param>
    public static void Attack(PlayerActionPoint attacker, PlayerHealth target, BodyPosition position, Item weapon)
    {
        WeaponItemData weaponData = weapon.ItemData as WeaponItemData;
        if (weaponData == null)
            return;
        if (attacker.DecreaseActionPoint(weaponData.AttakAPCost))
            target.TakeWeaponDamage(attacker.GetComponent<PlayerItemInteraction>(),
                target.GetComponent<PlayerItemInteraction>(), position, weapon);
    }

    [Command]
    public void CmdAttack(GameObject attacker, GameObject target, int position, GameObject weapon)
    {
        Attack(attacker.GetComponent<PlayerActionPoint>(), target.GetComponent<PlayerHealth>(), (BodyPosition)position,
            weapon.GetComponent<Item>());
    }

    /// <summary>
    /// 受到武器的伤害，会根据对应部位、武器和防具计算伤害(保留2位小数)，扣除对应的血量和武器/防具耐久度
    /// </summary>
    /// <param name="attacker">攻击的发起方，会扣除对应的体力</param>
    /// <param name="target">被攻击的目标</param>
    /// <param name="position">受击位置</param>
    /// <param name="weaponItem">攻击方使用的武器</param>
    private void TakeWeaponDamage(PlayerItemInteraction attacker, PlayerItemInteraction target, BodyPosition position,
        Item weaponItem)
    {
        //武器伤害计算公式为：Dmg(伤害)= Tch(机制乘区)*Bdy(部位乘区)*Bsc(基础伤害)
        WeaponItemData weaponData = weaponItem.ItemData as WeaponItemData;
        if (weaponData == null)
            return;
        //基础伤害
        float damage = weaponData.BasicDamage;
        //部位乘区，默认为1
        if (weaponData.BodyDamageDictionary.ContainsKey(position))
            damage *= weaponData.BodyDamageDictionary.Get(position);
        attacker.DecreaseDurability(weaponItem.gameObject);
        //清除已经被摧毁的防具
        foreach (BodyPosition bodyPosition in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
        {
            if (_armorEquipments.TryGetValue(bodyPosition, out var armor) && armor == null)
            {
                _armorEquipments.Remove(bodyPosition);
            }
        }
        //机制乘区，默认为1，仅当对应部位有护甲&&护甲对武器伤害类型有特殊乘区时启用
        if (_armorEquipments.TryGetValue(position, out var armorItem) && armorItem.ItemData is ArmorItemData armorData)
        {
            if (armorData.DamageTypeDictionary.ContainsKey(weaponData.AttackDamageType))
            {
                damage *= armorData.DamageTypeDictionary.Get(weaponData.AttackDamageType);
            }
            damage = (float)Math.Round(damage, 1);
            target.DecreaseDurability(armorItem.gameObject, damage);
        }
        damage = (float)Math.Round(damage, 1);
        ChangeHealth((int)position, -damage);
    }

    public float GetWeaponDamage(BodyPosition position, Item weaponItem)
    {
        //武器伤害计算公式为：Dmg(伤害)= Tch(机制乘区)*Bdy(部位乘区)*Bsc(基础伤害)
        WeaponItemData weaponData = weaponItem.ItemData as WeaponItemData;
        if (weaponData == null)
            return 0;
        //基础伤害
        float damage = weaponData.BasicDamage;
        //部位乘区，默认为1
        if (weaponData.BodyDamageDictionary.ContainsKey(position))
            damage *= weaponData.BodyDamageDictionary.Get(position);
        //清除已经被摧毁的防具
        foreach (BodyPosition bodyPosition in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
        {
            if (_armorEquipments.TryGetValue(bodyPosition, out var armor) && armor == null)
            {
                _armorEquipments.Remove(bodyPosition);
            }
        }
        //机制乘区，默认为1，仅当对应部位有护甲&&护甲对武器伤害类型有特殊乘区时启用
        if (_armorEquipments.TryGetValue(position, out var armorItem) && armorItem.ItemData is ArmorItemData armorData)
        {
            if (armorData.DamageTypeDictionary.ContainsKey(weaponData.AttackDamageType))
            {
                damage *= armorData.DamageTypeDictionary.Get(weaponData.AttackDamageType);
            }
        }
        return damage;
    }

    public static void Heal(PlayerHealth healer, BodyPosition position, Item medicine, bool isGlobalHeal = false)
    {
        MedicineItemData medicineData = medicine.ItemData as MedicineItemData;
        if (medicineData == null)
            return;
        healer.TakeMedicineHeal(healer.GetComponent<PlayerItemInteraction>(), position, medicine, isGlobalHeal);
    }

    [Command]
    public void CmdHeal(GameObject healer, int position, GameObject medicine, bool isGlobalHeal)
    {
        Heal(healer.GetComponent<PlayerHealth>(), (BodyPosition)position, medicine.GetComponent<Item>(), isGlobalHeal);
    }

    private void TakeMedicineHeal(PlayerItemInteraction healer, BodyPosition position, Item medicineItem, bool isGlobalHeal = false)
    {
        MedicineItemData medicineData = medicineItem.ItemData as MedicineItemData;
        if (medicineData == null)
            return;
        float heal = 0;
        if (medicineData.BodyHealDictionary.ContainsKey(position))
            heal = medicineData.BodyHealDictionary.Get(position);
        if (!isGlobalHeal)
        {
            healer.DecreaseDurability(medicineItem.gameObject);
        }
        ChangeHealth((int)position, heal);
    }


    /// <summary>
    /// Command函数，在客户端被调用，但在服务端执行。
    /// 向服务端同步玩家的名字
    /// </summary>
    /// <param name="name">玩家的名字</param>
    [Command]
    private void CmdSetName(string name)
    {
        _name = name;
    }

    /// <summary>
    /// 改变血量的同一接口(保留2位小数)，每个部位血量范围为0到MaxHealth，低于0会触发死亡
    /// </summary>
    /// <param name="bodyPosition">血量改变部位，需要使用(int)BodyPosition.xxx</param>
    /// <param name="healthChange">血量改变量，正数回血，负数扣血</param>
    public void ChangeHealth(int bodyPosition, float healthChange)
    {
        if (isServer)
        {
            DeployChangeHealth(bodyPosition, healthChange);
        }
        else
        {
            CmdChangeHealth(bodyPosition, healthChange);
        }
    }
    [Command]
    private void CmdChangeHealth(int bodyPosition, float healthChange)
    {
        DeployChangeHealth(bodyPosition, healthChange);
    }
    private void DeployChangeHealth(int bodyPosition, float healthChange)
    {
        if(!_isAlive || PlayerManager.Instance.IfVictory)
        {
            return;
        }
        BodyPosition pos = (BodyPosition)bodyPosition;
        switch (pos)
        {
            case BodyPosition.Head:
                _headHealth = (float)Math.Round(Mathf.Clamp(_headHealth + healthChange, 0, HeadMaxHealth), 1);
                break;
            case BodyPosition.MainBody:
                _bodyHealth = (float)Math.Round(Mathf.Clamp(_bodyHealth + healthChange, 0, BodyMaxHealth), 1);
                break;
            case BodyPosition.Legs:
                _legHealth = (float)Math.Round(Mathf.Clamp(_legHealth + healthChange, 0, LegMaxHealth), 1);
                break;
        }

        deathCheck();
    }

    /// <summary>
    /// 死亡检测，若头部或躯干生命值等于0则触发死亡
    /// </summary>
    private void deathCheck()
    {
        if (_headHealth <= 0 || _bodyHealth <= 0)
        {
            if(gameObject.GetComponent<PlayerLog>().LogList.Last().Type == LogInfo.DamageType.fight)
            {
                gameObject.GetComponent<PlayerFight>().Enemy.GetComponent<PlayerLog>().CheckFlag = false;
            }
            _isAlive = false;
            TargetCreateRP();
            TargetPlayerDie(gameObject.GetComponent<NetworkIdentity>().connectionToClient, 
                            gameObject.GetComponent<PlayerLog>().LogList.Last(),  
                            gameObject.GetComponent<PlayerLog>().EliminationCount);
            RpcPlayerDie(gameObject.GetComponent<PlayerLog>().LogList.Last(), gameObject);
            if(gameObject.GetComponent<PlayerLog>().LogList.Last().Type == LogInfo.DamageType.fight)
            {   
                while(!gameObject.GetComponent<PlayerLog>().CheckFlag)
                {
                    continue;
                }
            }
            PlayerManager.Instance.DeployPlayerDie();
        }
    }
    [ClientRpc]
    void RpcPlayerDie(LogInfo logInfo, GameObject deadPlayer)
    {
        deadPlayer.transform.Find("SpriteDisplay").gameObject.SetActive(false);
        deadPlayer.transform.Find("Canvas").gameObject.SetActive(false);
        string deadPlayerName = deadPlayer.GetComponent<PlayerHealth>().Name;
        switch (logInfo.Type)
        {
            case LogInfo.DamageType.fight:
                string pattern = @"被(?<localName>.+?)用";
                Match match = Regex.Match(logInfo.Message, pattern);
                string enemyName = match.Groups["localName"].Value;
                UIManager.Instance.AddKillLog(LogInfo.DamageType.fight, deadPlayerName, enemyName);
                break;
            case LogInfo.DamageType.poison:
                UIManager.Instance.AddKillLog(LogInfo.DamageType.poison, deadPlayerName);
                break;
            case LogInfo.DamageType.other:
                UIManager.Instance.AddKillLog(LogInfo.DamageType.other, null);
                break;
        }
    }

    [TargetRpc]
    public void TargetCreateRP()
    {
        List<Item> itemlist = BackpackManager.Instance.ItemList;
        foreach (var armorSlot in BackpackManager.Instance.ArmorSlots)
        {
            var slot = armorSlot.Value;
            var item = slot.GetItem();
            if (item != null)
            {
                itemlist.Add(item);
            }
        }

        List<GameObject> itemObjList = new List<GameObject>();
        itemlist.ForEach(i=>itemObjList.Add(i.gameObject));
        CmdCreateRP(itemObjList);
    }


    [Command]
    public void CmdCreateRP(List<GameObject> itemList)
    {
        Vector3Int tempPosition = _furnitureTilemap.WorldToCell(transform.position);
        Vector3 cellPosition = _furnitureTilemap.GetCellCenterWorld(tempPosition);
        GameObject instance = Instantiate(Resources.Load<GameObject>("ResourcePoint"));
        instance.transform.position = cellPosition;
        instance.transform.SetParent(_furnitureTilemap.transform);
        ResourcePointController resourcePointController = instance.GetComponent<ResourcePointController>();
        List<Item> emptyItemList = new List<Item>();
        resourcePointController.InitializeWithCustomItems(emptyItemList);

        foreach (var item in itemList)
        {
            resourcePointController.AddItemToResourcePoint(item.GetComponent<Item>());
        }
        NetworkServer.Spawn(instance);
        RpcSyncInstance(instance, cellPosition, itemList);
    }


    /// <summary>
    /// 在客户端将实例设置为 Tilemap 的子对象，并更新位置
    /// </summary>
    [ClientRpc]
    private void RpcSyncInstance(GameObject instance, Vector3 cellPosition, List<GameObject> itemList)
    {
        ResourcePointController resourcePointController = instance.GetComponent<ResourcePointController>();
        instance.transform.position = cellPosition;
        instance.transform.SetParent(_furnitureTilemap.transform);

        foreach (var item in itemList)
        {
            resourcePointController.AddItemToResourcePoint(item.GetComponent<Item>());
        }
    }
    
    [TargetRpc]
    public void TargetPlayerDie(NetworkConnection conn, LogInfo logInfo, int eliminationCount)
    {
        if(gameObject.GetComponent<PlayerFight>().IsFighting)
        {
            gameObject.GetComponent<PlayerFight>().CmdDead();
        }
        GameObject playerDeadUI = GameObject.Find("Canvas").transform.Find("PlayerDead").gameObject;
        playerDeadUI.transform.Find("RankInfo").Find("Rank").GetChild(1).GetComponent<TMP_Text>().text = 
            PlayerManager.Instance.AlivePlayerCount.ToString();
        playerDeadUI.transform.Find("RankInfo").Find("Elimination").GetChild(1).GetComponent<TMP_Text>().text = 
            eliminationCount.ToString();
        playerDeadUI.transform.Find("DeadInfo").GetComponent<TMP_Text>().text = 
            logInfo.Message;
        playerDeadUI.SetActive(true);
    }

    /// <summary>
    /// 删除安全区并更新
    /// </summary>
    [Command]
    public void CmdDeleteSafeArea()
    {
        SafeAreaManager.Instance.DeleteSafeAreaOnServer();
    }
}