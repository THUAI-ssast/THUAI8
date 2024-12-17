using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单例Manager，管理教程提示的逐步显示
/// </summary>
public class TutorialManager : MonoBehaviour
{
    /// <summary>
    /// 教程状态，包含了提示信息的图文内容，以及状态切换条件
    /// </summary>
    private class TutorialState
    {
        public TutorialState(string description, string spritePath1, string spritePath2, boolFunc finishCondition,
            float delayTime = 1.5f)
        {
            DescriptionText = description;
            Sprite1 = Resources.Load<Sprite>(spritePath1);
            Sprite2 = Resources.Load<Sprite>(spritePath2);
            IsTutorialFinished = finishCondition;
            DelayTime = delayTime;
        }

        public readonly string DescriptionText;
        public readonly Sprite Sprite1;
        public readonly Sprite Sprite2;
        public readonly float DelayTime;

        public delegate bool boolFunc();
        /// <summary>
        /// 状态退出条件，返回true时应退出并切换到下一状态
        /// </summary>
        public readonly boolFunc IsTutorialFinished;
    }

    [SerializeField] private Item _medicine;
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private GameObject _firstResourcePoitnUI;
    [SerializeField] private GameObject _secondResourcePoitnSlots;
    [SerializeField] private GameObject _thirdResourcePoitnSlots;
    private GameObject _firstRPSlots;
    private GameObject _craftPanel;
    private TMP_Text _tutorialText;
    private Image _tutorialImage1;
    private Image _tutorialImage2;
    private Queue<TutorialState> _stateQueue = new Queue<TutorialState>();
    private TutorialState _currentState;
    private GameObject _playerObject;
    private PlayerActionPoint _playerActionPoint;


    private void Start()
    {
        _medicine.Initialize(_medicine.ItemData, ItemOwner.World, 0);
        _tutorialText = _tutorialPanel.GetComponentInChildren<TMP_Text>();
        _tutorialImage1 = _tutorialPanel.transform.Find("Image1").GetComponent<Image>();
        _tutorialImage2 = _tutorialPanel.transform.Find("Image2").GetComponent<Image>();
        _firstRPSlots = _firstResourcePoitnUI.transform.Find("Scroll View/Viewport/Slots").gameObject;
        _craftPanel = UIManager.Instance.BagPanel.transform.Find("CraftPanel").gameObject;

        setState(new TutorialState("点击(C键)打开悬停显示\n将鼠标悬停在地块上可以查看信息",
            "UI/Sprite/KeyboardSprite/C", null, () => Input.GetKeyDown(KeyCode.C)));
        _stateQueue.Enqueue(new TutorialState("在地块上点击(鼠标左键)移动\n每移动1格会消耗0.2AP",
    "UI/Sprite/KeyboardSprite/Mouse0", null,
    () => _playerObject.transform.position.x > 3));
        _stateQueue.Enqueue(new TutorialState("移动到物品旁\n(鼠标右键)点击物品进行拾取",
            "UI/Sprite/Tutorial/ItemExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _medicine.GetComponent<SpriteRenderer>().enabled == false, 0.8f));
        _stateQueue.Enqueue(new TutorialState("点击背包图标或按(E键)打开背包\n再次按(E键)或按(Esc键)退出",
            "UI/Sprite/Tutorial/BagExample", "UI/Sprite/KeyboardSprite/E",
            () => UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("右侧是现有物品,悬停可显示信息\n使用(鼠标滚轮)可滑动显示区域",
            null, "UI/Sprite/KeyboardSprite/Mouse2",
            () => Input.GetAxis("Mouse ScrollWheel") < 0f, 3f));
        _stateQueue.Enqueue(new TutorialState("左侧是状态栏,分为头、躯干、腿\n每个部位有独立的血量和装备",
            "UI/Sprite/Tutorial/HealthExample", "UI/Sprite/Tutorial/ArmorExample",
            () => true, 7f));
        _stateQueue.Enqueue(new TutorialState("头部或躯干血量归零则角色死亡\n血量可以通过使用药品回复",
            null, "UI/Sprite/Tutorial/MedicineExample",
            () => true, 7f));
        _stateQueue.Enqueue(new TutorialState("躯干和腿部受伤了,需要处理\n(鼠标右键)点击背包内物品使用",
            "UI/Sprite/Tutorial/MedicineExample", "UI/Sprite/Tutorial/UseExample",
            () => !_medicine.gameObject.activeSelf));
        _stateQueue.Enqueue(new TutorialState("只靠一个绷带不能完全康复\n关闭背包,搜寻更多药品",
            "UI/Sprite/KeyboardSprite/Escape",null,
            () => !UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("移动到微弱发光的资源点地块旁\n(鼠标右键)点击资源点进行搜刮",
            "UI/Sprite/Tutorial/ResourcePointExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _firstResourcePoitnUI.activeSelf));
        _stateQueue.Enqueue(new TutorialState("点击下方按钮消耗AP搜刮\n(鼠标左键)点击物品将其加入背包",
            null, "UI/Sprite/KeyboardSprite/Mouse0",
            () => _firstRPSlots.activeSelf, 2));
        _stateQueue.Enqueue(new TutorialState("搜到了药品,按(Esc)退出资源点\n打开背包治疗自己",
            "UI/Sprite/KeyboardSprite/Escape", "UI/Sprite/Tutorial/BagExample",
            () => UIManager.Instance.BagPanel.activeSelf, 2));
        _stateQueue.Enqueue(new TutorialState("关闭背包,继续搜刮\n走到门旁点击(鼠标右键)开/关门",
            "UI/Sprite/KeyboardSprite/Escape", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _secondResourcePoitnSlots.activeInHierarchy));
        _stateQueue.Enqueue(new TutorialState("用胶带和木板可以制作夹板\n打开背包进行合成",
            "UI/Sprite/Tutorial/BagExample", "UI/Sprite/Tutorial/CraftExample",
            () => _craftPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("能够合成的配方左上角为绿色\n(左键)选中后点击合成按钮",
            null, "UI/Sprite/Tutorial/CraftDeployExample",
            () => BackpackManager.Instance.ItemList.Find(i => i.ItemData.ItemName == "夹板")));
        _stateQueue.Enqueue(new TutorialState("关闭背包,继续搜刮\n",
            "UI/Sprite/KeyboardSprite/Escape", null,
            () => !UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("移动和搜刮都会消耗AP点\n显示在左侧角色面板",
            "UI/Sprite/Tutorial/PlayerInfoExample", null,
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("屏幕上方显示了当前世界回合\n每个世界回合开始时会获得12AP",
            "UI/Sprite/Tutorial/TurnDisplayExample", null,
            () => true, 10));
        _stateQueue.Enqueue(new TutorialState("点击右下角按钮两次投票结束回合\n所有玩家均投票则进入下一回合",
            "UI/Sprite/Tutorial/EndTurnExample", "UI/Sprite/Tutorial/ConfirmEndExample",
            () => _playerActionPoint.CurrentActionPoint >= 12, 2));
        _stateQueue.Enqueue(new TutorialState("左上角显示了小地图\n按(Tab键)打开大地图",
            "UI/Sprite/Tutorial/MiniMapExample", "UI/Sprite/KeyboardSprite/Tab",
            () => Input.GetKeyDown(KeyCode.Tab), 1));
        _stateQueue.Enqueue(new TutorialState("地图上显示了当前安全区\n白框内是下一个安全区",
            null, null,
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("每2个世界回合会缩小安全区\n安全区外活动会掉血",
            null, null,
            () => true, 6));
        _stateQueue.Enqueue(new TutorialState("继续搜刮\n寻找有用的物资",
            null, null,
            () => _thirdResourcePoitnSlots.activeInHierarchy, 0));
        _stateQueue.Enqueue(new TutorialState("武器具有伤害类型和基础伤害\n攻击不同部位伤害倍率不同",
            "UI/Sprite/Tutorial/WeaponExample", null,
            () => !_thirdResourcePoitnSlots.activeInHierarchy, 2));
        _stateQueue.Enqueue(new TutorialState("现在的材料可以合成纸质护甲了\n点击右键可装备护甲",
            "UI/Sprite/Tutorial/ArmorExample2", "UI/Sprite/Tutorial/EquipExample",
            () => BackpackManager.Instance.ItemList.Find(
                i => i.ItemData.ItemName == "纸质护甲")));
        _stateQueue.Enqueue(new TutorialState("护甲会减少特定类型的伤害\n其承伤值归零后会损坏",
            "UI/Sprite/Tutorial/ArmorExample2", null,
            () => UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("关闭背包\n回到场景中继续探索",
            "UI/Sprite/KeyboardSprite/Escape", null,
            () => !UIManager.Instance.BagPanel.activeSelf, 0.5f));
        _stateQueue.Enqueue(new TutorialState("右侧有玻璃,穿过会破碎发出声音\n可能会向附近玩家暴露你的位置",
            null, "UI/Sprite/Tutorial/GlassExample",
            () => _playerObject.transform.position.x > 20, 1));
        _stateQueue.Enqueue(new TutorialState("发现了一个敌人\n按住alt点击敌人,发起攻击",
            "UI/Sprite/KeyboardSprite/LeftAlt", "UI/Sprite/Tutorial/FightExample",
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("在自己的回合点击左键选取武器\n每回合最多可消耗1AP",
            null, null,
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("训练结束,战斗需实战学习\n即将退回主菜单",
            null, null,
            () => true, 6));
        _stateQueue.Enqueue(new TutorialState("",
            null, null,
            () => true));


        StartCoroutine(changePlayerHP());
    }

    private IEnumerator changePlayerHP()
    {
        while ((_playerObject = GameObject.FindWithTag("LocalPlayer")) == null)
        {
            yield return new WaitForSeconds(0.2f);
        }

        PlayerMove playerMove = _playerObject.GetComponent<PlayerMove>();
        Vector3 bornPos = Vector3.zero;
        var tilePosition = GridMoveController.Instance.GroundTilemap.WorldToCell(bornPos);
        playerMove.transform.position = bornPos + GridMoveController.Instance.GroundTilemap.cellSize * 0.5f;
        bornPos = playerMove.transform.position;
        playerMove.CmdSetPosition(bornPos, tilePosition, null);

        _playerActionPoint = _playerObject.GetComponent<PlayerActionPoint>();

        PlayerHealth playerHealth = _playerObject.GetComponent<PlayerHealth>();
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.MainBody, -5.5f);
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.Legs, -7.4f);
        yield return new WaitForSeconds(0.2f);
        GameObject.Find("SafeArea/WholeArea").SetActive(false);
    }

    private void Update()
    {
        if (!_isStateChanging && _currentState.IsTutorialFinished())
        {
            if (_stateQueue.TryDequeue(out var state))
            {
                StartCoroutine(setStateWithDelay(state));
            }
            else
            {
                NetworkManagerController.Instance.IsEnterTutorial = false;
                RoomManager.Instance.StopHost();
            }
        }
    }

    private bool _isStateChanging;

    private IEnumerator setStateWithDelay(TutorialState state)
    {
        _isStateChanging = true;
        yield return new WaitForSeconds(_currentState.DelayTime);
        setState(state);
        _isStateChanging = false;
    }

    private void setState(TutorialState state)
    {
        _currentState = state;
        _tutorialText.text = state.DescriptionText;
        _tutorialImage1.enabled = (state.Sprite1 != null);
        _tutorialImage1.sprite = state.Sprite1;
        _tutorialImage2.enabled = (state.Sprite2 != null);
        _tutorialImage2.sprite = state.Sprite2;
    }

    public void OnClickExitButton()
    {
        NetworkManagerController.Instance.IsEnterTutorial = false;
        RoomManager.Instance.StopHost();
    }
}