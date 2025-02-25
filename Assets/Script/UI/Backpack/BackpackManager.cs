using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 单例Manager，管理背包中的一切行为，包括对物品丢弃、使用、添加等。
/// </summary>
public class BackpackManager : MonoBehaviour
{
    /// <summary>
    /// 定位背包UI
    /// </summary>
    [SerializeField] private GameObject _bagPanel;

    [SerializeField] private GameObject _battlePanel;

    private AudioClip _useBandageAudioClip;
    private AudioClip[] _equipArmorAudioClips;
    private AudioClip _addItemAudioClip;
    public Transform HeadHealthPanel;
    public Transform LegsHealthPanel;
    public Transform BodyHealthPanel;
    public Transform BattleHeadHealthPanel;
    public Transform BattleLegsHealthPanel;
    public Transform BattleBodyHealthPanel;
    public Transform BattleHeadHealthEnemyPanel;
    public Transform BattleLegsHealthEnemyPanel;
    public Transform BattleBodyHealthEnemyPanel;

    private Dictionary<PlayerHealth.BodyPosition, ArmorSlot> _armorSlots =
        new Dictionary<PlayerHealth.BodyPosition, ArmorSlot>();

    private Transform _slotsTransform;
    private Transform _battleSlotsTransform;

    /// <summary>
    /// 单例模式
    /// </summary>
    public static BackpackManager Instance;

    /// <summary>
    /// 背包中现有所有物品的列表，增删需更改列表。
    /// </summary>
    private List<Item> _itemList = new List<Item>();

    public List<Item> ItemList
    {
        get => _itemList;
        private set => _itemList = value;
    }

    /// <summary>
    /// 背包最大容量
    /// </summary>
    public readonly int ItemSlotsLimit = 20;

    /// <summary>
    /// 背包中当前物品数
    /// </summary>
    public int ItemNumberInBag => _itemList.Count;

    /// <summary>
    /// 背包是否装满了物品，true则表示背包已满
    /// </summary>
    public bool IsBackpackFull => _itemList.Count >= ItemSlotsLimit;

    public Dictionary<PlayerHealth.BodyPosition, ArmorSlot> ArmorSlots
    {
        get => _armorSlots;
        private set => _armorSlots = value;
    }


    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            _addItemAudioClip = Resources.Load<AudioClip>("Sound/UI/pop");
            _useBandageAudioClip = Resources.Load<AudioClip>("Sound/UI/Backpack/UseBandage");
            _equipArmorAudioClips = Resources.LoadAll<AudioClip>("Sound/Items/Equipment");
        }
    }

    /// <summary>
    /// 初始化背包
    /// </summary>
    void Start()
    {
        _slotsTransform = _bagPanel.transform.Find("ItemsPanel/Scroll View/Viewport/Slots");
        HeadHealthPanel = _bagPanel.transform.Find("HealthPanel/Head");
        BodyHealthPanel = _bagPanel.transform.Find("HealthPanel/Body");
        LegsHealthPanel = _bagPanel.transform.Find("HealthPanel/Legs");

        _armorSlots[PlayerHealth.BodyPosition.Head] = HeadHealthPanel.Find("Equipment").GetComponent<ArmorSlot>();
        _armorSlots[PlayerHealth.BodyPosition.MainBody] = BodyHealthPanel.Find("Equipment").GetComponent<ArmorSlot>();
        _armorSlots[PlayerHealth.BodyPosition.Legs] = LegsHealthPanel.Find("Equipment").GetComponent<ArmorSlot>();

        _battleSlotsTransform = _battlePanel.transform.Find("BattleItemsPanel/Scroll View/Viewport/Slots");
        BattleHeadHealthPanel = _battlePanel.transform.Find("HealthPanel/Head");
        BattleBodyHealthPanel = _battlePanel.transform.Find("HealthPanel/Body");
        BattleLegsHealthPanel = _battlePanel.transform.Find("HealthPanel/Legs");
        BattleHeadHealthEnemyPanel = _battlePanel.transform.Find("HealthPanel_enemy/Head");
        BattleBodyHealthEnemyPanel = _battlePanel.transform.Find("HealthPanel_enemy/Body");
        BattleLegsHealthEnemyPanel = _battlePanel.transform.Find("HealthPanel_enemy/Legs");

        RefreshSlots();
        // StartCoroutine(initItems_debug());
    }

    private IEnumerator initItems_debug()
    {
        yield return new WaitForSeconds(1);
        if (GameObject.FindWithTag("LocalPlayer") != null)
        {
            // CreateItem("ScriptableObject/Items/木板");
            // CreateItem("ScriptableObject/Items/锤石");
            // CreateItem("ScriptableObject/Items/Weapons/球棒");
            // CreateItem("ScriptableObject/Items/金属破片");
            // CreateItem("ScriptableObject/Items/石头");
            // CreateItem("ScriptableObject/Items/刀片");
            // CreateItem("ScriptableObject/Items/攀岩绳");
            // CreateItem("ScriptableObject/Items/Armor/摩托头盔");
            // CreateItem("ScriptableObject/Items/Armor/防刺服");
            // CreateItem("ScriptableObject/Items/Weapons/小刀");
            CreateItem("ScriptableObject/Items/Weapons/佩剑");
            // CreateItem("ScriptableObject/Items/Weapons/小刀");
            CreateItem("ScriptableObject/Items/Weapons/佩剑");
            // CreateItem("ScriptableObject/Items/Weapons/小刀");
            CreateItem("ScriptableObject/Items/Weapons/佩剑");
            // CreateItem("ScriptableObject/Items/Weapons/小刀");
            // CreateItem("ScriptableObject/Items/Weapons/佩剑");
            // CreateItem("ScriptableObject/Items/Weapons/小刀");
            // CreateItem("ScriptableObject/Items/Weapons/佩剑");
            // CreateItem("ScriptableObject/Items/Weapons/小刀");
            // CreateItem("ScriptableObject/Items/Weapons/佩剑");
            // CreateItem("ScriptableObject/Items/Medicines/医用酒精");
            // CreateItem("ScriptableObject/Items/Medicines/医用绷带");
            // CreateItem("ScriptableObject/Items/Medicines/止痛药");
        }
        else
        {
            Debug.Log("Local Player is null!");
        }
    }

    /// <summary>
    /// 创建物品并添加到背包中
    /// </summary>
    /// <param name="itemdata_pth">Resources中要创建的物品的信息的路径</param>
    private void CreateItem(string itemdata_pth)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        ItemOwner owner = ItemOwner.PlayerBackpack;
        Vector3 position = Vector3.zero;
        Item.Create(itemdata_pth, owner, player, null);
    }

    /// <summary>
    /// 销毁物品
    /// </summary>
    /// <param name="item">要销毁的物品</param>
    private void DestroyItem(Item item)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        RemoveItem(item);
        Item.Destroy(item, player);
    }

    /// <summary>
    /// 向背包中添加物品
    /// </summary>
    /// <param name="item">要添加的物品</param>
    public void AddItem(Item item)
    {
        // 从背包中添加物品
        _itemList.Add(item);
        AudioManager.Instance.CameraSource.PlayOneShot(_addItemAudioClip);
        RefreshSlots();
    }

    /// <summary>
    /// 从背包中移除物品
    /// </summary>
    /// <param name="item">要移除的物品</param>
    public void RemoveItem(Item item)
    {
        if (_itemList.Remove(item))
            RefreshSlots();
        foreach (PlayerHealth.BodyPosition position in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
        {
            if (_armorSlots[position].GetItem() == item)
            {
                GameObject player = GameObject.FindWithTag("LocalPlayer");
                _armorSlots[position].SetItem(null);
            }
        }
    }

    /// <summary>
    /// 使用背包中的物品
    /// </summary>
    /// <param name="item">要使用的物品</param>
    /// <param name="healHead">是否要治疗头部</param>
    /// <param name="healBody">是否要治疗躯干</param>
    /// <param name="heallegs">是否要治疗腿部</param>
    public void UseItem(Item item, bool isGlobalHeal = true, bool healHead = true, bool healBody = true,
        bool heallegs = true)
    {
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        var playerInteraction = player.GetComponent<PlayerItemInteraction>();
        if (item.ItemData is ArmorItemData armorData)
        {
            AudioManager.Instance.CameraSource.PlayOneShot(
                _equipArmorAudioClips[Random.Range(0, _equipArmorAudioClips.Length - 1)]);
            player.GetComponent<PlayerHealth>().EquipArmor(armorData.EquipBodyPosition, item);
            _itemList.Remove(item);
        }
        else if (item.ItemData is MedicineItemData medicineData)
        {
            AudioManager.Instance.CameraSource.PlayOneShot(_useBandageAudioClip);
            if (isGlobalHeal)
            {
                player.GetComponent<PlayerHealth>()
                    .CmdHeal(player, (int)PlayerHealth.BodyPosition.Head, item.gameObject, true);
                player.GetComponent<PlayerHealth>()
                    .CmdHeal(player, (int)PlayerHealth.BodyPosition.MainBody, item.gameObject, true);
                player.GetComponent<PlayerHealth>()
                    .CmdHeal(player, (int)PlayerHealth.BodyPosition.Legs, item.gameObject, true);
                player.GetComponent<PlayerItemInteraction>().DecreaseDurability(item.gameObject);
            }
            else
            {
                if (healHead)
                    player.GetComponent<PlayerHealth>().CmdHeal(player, (int)PlayerHealth.BodyPosition.Head,
                        item.gameObject, false); // 治疗者，治疗部位，使用物品
                else if (healBody)
                    player.GetComponent<PlayerHealth>().CmdHeal(player, (int)PlayerHealth.BodyPosition.MainBody,
                        item.gameObject, false);
                else if (heallegs)
                    player.GetComponent<PlayerHealth>().CmdHeal(player, (int)PlayerHealth.BodyPosition.Legs,
                        item.gameObject, false);
            }
        }

        RefreshSlots();
    }


    /// <summary>
    /// 丢弃背包中的物品到世界
    /// </summary>
    /// <param name="item">要丢弃的物品</param>
    public void DropItem(Item item)
    {
        RemoveItem(item);
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        player.GetComponent<PlayerItemInteraction>().DropItem(item.gameObject);
        CraftWayUI.UpdateSatisfiedAll();
    }

    public void RefreshSlots()
    {
        UpdateSlots(_slotsTransform, true);
        UpdateSlots(_battleSlotsTransform, false);
    }

    /// <summary>
    /// 更新指定的物品槽内容
    /// </summary>
    /// <param name="slotsTransform">要更新的物品槽的 Transform</param>
    /// <param name="updateCraftWayUI">是否更新 CraftWayUI</param>
    private void UpdateSlots(Transform slotsTransform, bool updateCraftWayUI)
    {
        if (slotsTransform == null)
        {
            return;
        }

        // 移除无效物品
        _itemList.RemoveAll(i => i == null || i.CurrentDurability == 0);

        // 遍历更新槽位
        for (int i = 0; i < slotsTransform.childCount; i++)
        {
            if (i < _itemList.Count)
            {
                // 设置槽位内容
                slotsTransform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = true;
                slotsTransform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = _itemList[i].ItemData.ItemIcon;
                slotsTransform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                    _itemList[i].ItemData.ItemName;

                if (_itemList[i].MaxDurability != -1)
                {
                    slotsTransform.GetChild(i).GetChild(2).GetComponent<Image>().enabled = true;
                    slotsTransform.GetChild(i).GetChild(3).GetComponent<TextMeshProUGUI>().text =
                        $"{_itemList[i].CurrentDurability}/{_itemList[i].MaxDurability}";
                }
                else
                {
                    slotsTransform.GetChild(i).GetChild(2).GetComponent<Image>().enabled = false;
                    slotsTransform.GetChild(i).GetChild(3).GetComponent<TextMeshProUGUI>().text = "";
                }

                slotsTransform.GetChild(i).GetComponent<SlotMenuTrigger>().SetItem(_itemList[i]);
            }
            else
            {
                slotsTransform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
                slotsTransform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                slotsTransform.GetChild(i).GetChild(2).GetComponent<Image>().enabled = false;
                slotsTransform.GetChild(i).GetChild(3).GetComponent<TextMeshProUGUI>().text = "";
                slotsTransform.GetChild(i).GetComponent<SlotMenuTrigger>().SetItem(null);
            }
        }

        if (updateCraftWayUI)
        {
            CraftWayUI.UpdateSatisfiedAll();
        }
    }

    public void RefreshArmorDisplay()
    {
        GameObject localPlayer = GameObject.FindWithTag("LocalPlayer");
        if (localPlayer == null)
            return;

        var localPlayerHealth = localPlayer.GetComponent<PlayerHealth>();
        foreach (PlayerHealth.BodyPosition position in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
        {
            Item newArmor = localPlayerHealth.GetItemAt(position);
            var oldArmor = _armorSlots[position].SetItem(newArmor);
            if (oldArmor != null)
            {
                // 若有护甲变动，将原护甲放回背包，否则仅刷新显示
                if (oldArmor != newArmor)
                {
                    AddItem(oldArmor);
                }
                else
                {
                    _armorSlots[position].UpdateDisplay();
                }
            }
        }
    }


    /// <summary>
    /// 更新战斗面板的护甲显示，在玩家战斗时从客户端调用。
    /// </summary>
    /// <param name="enemyPlayer">当前玩家的敌人</param>
    public void RefreshArmorBattleDisplay(GameObject enemyPlayer)
    {
        GameObject localPlayer = GameObject.FindWithTag("LocalPlayer");
        if (localPlayer != null)
        {
            var playerHealth = localPlayer.GetComponent<PlayerHealth>();
            foreach (PlayerHealth.BodyPosition position in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
            {
                Item playerArmor = playerHealth.GetItemAt(position);
                UpdateBattleArmorDisplay(position, playerArmor, false);
            }
        }

        GameObject enemy = enemyPlayer;
        if (enemy != null)
        {
            var enemyHealth = enemy.GetComponent<PlayerHealth>();
            foreach (PlayerHealth.BodyPosition position in Enum.GetValues(typeof(PlayerHealth.BodyPosition)))
            {
                Item enemyArmor = enemyHealth.GetItemAt(position);
                UpdateBattleArmorDisplay(position, enemyArmor, true);
            }
        }
    }

    /// <summary>
    /// 更新 battlePanel 的 armor display
    /// </summary>
    /// <param name="position">身体部位位置</param>
    /// <param name="newArmor">新装备的护甲</param>
    private void UpdateBattleArmorDisplay(PlayerHealth.BodyPosition position, Item newArmor, bool isEnemy = false)
    {
        Transform battleHealthPanel = null;
        // LocalPlayer
        if (!isEnemy)
        {
            switch (position)
            {
                case PlayerHealth.BodyPosition.Head:
                    battleHealthPanel = BattleHeadHealthPanel;
                    break;
                case PlayerHealth.BodyPosition.MainBody:
                    battleHealthPanel = BattleBodyHealthPanel;
                    break;
                case PlayerHealth.BodyPosition.Legs:
                    battleHealthPanel = BattleLegsHealthPanel;
                    break;
            }
        }
        // Player(enemy)
        else
        {
            switch (position)
            {
                case PlayerHealth.BodyPosition.Head:
                    battleHealthPanel = BattleHeadHealthEnemyPanel;
                    break;
                case PlayerHealth.BodyPosition.MainBody:
                    battleHealthPanel = BattleBodyHealthEnemyPanel;
                    break;
                case PlayerHealth.BodyPosition.Legs:
                    battleHealthPanel = BattleLegsHealthEnemyPanel;
                    break;
            }
        }

        if (battleHealthPanel != null)
        {
            var battleArmorSlot = battleHealthPanel.Find("Equipment").GetComponent<ArmorSlot>();
            battleArmorSlot.SetItem(newArmor);
            battleArmorSlot.UpdateDisplay();
        }
    }

    /// <summary>
    /// 确定背包内是否包含合成所需要物品
    /// </summary>
    /// <param name="craftWay">检测的目标合成路径</param>
    /// <returns>若全部满足则返回100+满足物品个数，否则返回满足物品个数</returns>
    public int IsCraftSatisfied(CraftWayData craftWay)
    {
        int count = 0;
        List<Item> testItemList = new List<Item>(_itemList);
        bool isSatisfied = true;
        foreach (ItemData costItem in craftWay.CostItems)
        {
            if (!testItemList.Remove(testItemList.Find(i => i.ItemData.ItemName == costItem.ItemName)))
                isSatisfied = false;
            else
                count++;
        }

        foreach (ItemData catalystItem in craftWay.CatalystItems)
        {
            if (!testItemList.Remove(testItemList.Find(i => i.ItemData.ItemName == catalystItem.ItemName)))
                isSatisfied = false;
            else
                count++;
        }

        if (!isSatisfied)
            return count;
        else
            return 100 + count;
    }

    /// <summary>
    /// 应用合成，在背包内销毁Cost Item并增加Product Item
    /// </summary>
    /// <param name="craftWay">要应用的目标合成路径</param>
    /// <returns>若大于等于100则全部满足并成功应用，否则返回满足物品个数</returns>
    public int DeployCraft(CraftWayData craftWay)
    {
        if (craftWay == null)
            return 0;
        int count = 0;
        List<Item> testItemList = new List<Item>(_itemList);
        List<Item> destroyList = new List<Item>();
        bool isSatisfied = true;
        foreach (ItemData costItem in craftWay.CostItems)
        {
            var item = testItemList.FindLast(i => i.ItemData.ItemName == costItem.ItemName);
            if (!testItemList.Remove(item))
                isSatisfied = false;
            else
            {
                count++;
                destroyList.Add(item);
            }
        }

        foreach (ItemData catalystItem in craftWay.CatalystItems)
        {
            if (testItemList.Find(i => i.ItemData.ItemName == catalystItem.ItemName) == null)
                isSatisfied = false;
            else
                count++;
        }

        if (!isSatisfied)
            return count;

        foreach (var item in destroyList)
            DestroyItem(item);
        if (craftWay.ProductItem is WeaponItemData weaponitem)
        {
            CreateItem("ScriptableObject/Items/" + "Weapons/" + weaponitem.ItemName);
        }
        else if (craftWay.ProductItem is MedicineItemData medicineitem)
        {
            CreateItem("ScriptableObject/Items/" + "Medicines/" + medicineitem.ItemName);
        }
        else if (craftWay.ProductItem is ArmorItemData armoritem)
        {
            CreateItem("ScriptableObject/Items/" + "Armor/" + armoritem.ItemName);
        }
        else
            CreateItem("ScriptableObject/Items/" + craftWay.ProductItem.ItemName);

        _itemList = testItemList;

        CraftWayUI.UpdateSatisfiedAll();
        return 100 + count;
    }
}