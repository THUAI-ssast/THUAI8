using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
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

        public readonly boolFunc IsTutorialFinished;
    }

    [SerializeField] private Item _medicine;
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private GameObject _firstResourcePoitnUI;
    private TMP_Text _tutorialText;
    private Image _tutorialImage1;
    private Image _tutorialImage2;
    private Queue<TutorialState> _stateQueue = new Queue<TutorialState>();
    private TutorialState _currentState;
    private GameObject _playerObject;


    private void Start()
    {
        _medicine.Initialize(_medicine.ItemData, ItemOwner.World, 0);
        _tutorialText = _tutorialPanel.GetComponentInChildren<TMP_Text>();
        _tutorialImage1 = _tutorialPanel.transform.Find("Image1").GetComponent<Image>();
        _tutorialImage2 = _tutorialPanel.transform.Find("Image2").GetComponent<Image>();

        setState(new TutorialState("点击(C键)打开悬停显示\n将鼠标悬停在地块上可以查看信息",
            "UI/Sprite/KeyboardSprite/C", null, () => Input.GetKeyDown(KeyCode.C)));
        _stateQueue.Enqueue(new TutorialState("在地块上点击(鼠标左键)移动\n每移动1格会消耗0.2AP",
            "UI/Sprite/KeyboardSprite/Mouse0", null,
            () => _playerObject.transform.position.x > 3));
        _stateQueue.Enqueue(new TutorialState("移动到物品旁\n(鼠标右键)点击物品进行拾取",
            "UI/Sprite/Tutorial/ItemExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _medicine.GetComponent<SpriteRenderer>().enabled == false));
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
            "UI/Sprite/KeyboardSprite/E", "UI/Sprite/KeyboardSprite/Esc",
            () => !UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("移动到微弱发光的资源点地块旁\n(鼠标右键)点击资源点进行搜刮",
            "UI/Sprite/Tutorial/ResourcePointExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _firstResourcePoitnUI.activeSelf));


        StartCoroutine(changePlayerHP());
    }

    private IEnumerator changePlayerHP()
    {
        while ((_playerObject = GameObject.FindWithTag("LocalPlayer")) == null)
        {
            yield return new WaitForSeconds(0.2f);
        }

        PlayerHealth playerHealth = _playerObject.GetComponent<PlayerHealth>();
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.MainBody, -5.5f);
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.Legs, -7.4f);
    }

    private void Update()
    {
        if (!_isStateChanging && _currentState.IsTutorialFinished() && _stateQueue.TryDequeue(out var state))
            StartCoroutine(setStateWithDelay(state));
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
}