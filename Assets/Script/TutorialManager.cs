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

        setState(new TutorialState("���(C��)����ͣ��ʾ\n�������ͣ�ڵؿ��Ͽ��Բ鿴��Ϣ",
            "UI/Sprite/KeyboardSprite/C", null, () => Input.GetKeyDown(KeyCode.C)));
        _stateQueue.Enqueue(new TutorialState("�ڵؿ��ϵ��(������)�ƶ�\nÿ�ƶ�1�������0.2AP",
            "UI/Sprite/KeyboardSprite/Mouse0", null,
            () => _playerObject.transform.position.x > 3));
        _stateQueue.Enqueue(new TutorialState("�ƶ�����Ʒ��\n(����Ҽ�)�����Ʒ����ʰȡ",
            "UI/Sprite/Tutorial/ItemExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _medicine.GetComponent<SpriteRenderer>().enabled == false));
        _stateQueue.Enqueue(new TutorialState("�������ͼ���(E��)�򿪱���\n�ٴΰ�(E��)��(Esc��)�˳�",
            "UI/Sprite/Tutorial/BagExample", "UI/Sprite/KeyboardSprite/E",
            () => UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("�Ҳ���������Ʒ,��ͣ����ʾ��Ϣ\nʹ��(������)�ɻ�����ʾ����",
            null, "UI/Sprite/KeyboardSprite/Mouse2",
            () => Input.GetAxis("Mouse ScrollWheel") < 0f, 3f));
        _stateQueue.Enqueue(new TutorialState("�����״̬��,��Ϊͷ�����ɡ���\nÿ����λ�ж�����Ѫ����װ��",
            "UI/Sprite/Tutorial/HealthExample", "UI/Sprite/Tutorial/ArmorExample",
            () => true, 7f));
        _stateQueue.Enqueue(new TutorialState("ͷ��������Ѫ���������ɫ����\nѪ������ͨ��ʹ��ҩƷ�ظ�",
            null, "UI/Sprite/Tutorial/MedicineExample",
            () => true, 7f));
        _stateQueue.Enqueue(new TutorialState("���ɺ��Ȳ�������,��Ҫ����\n(����Ҽ�)�����������Ʒʹ��",
            "UI/Sprite/Tutorial/MedicineExample", "UI/Sprite/Tutorial/UseExample",
            () => !_medicine.gameObject.activeSelf));
        _stateQueue.Enqueue(new TutorialState("ֻ��һ������������ȫ����\n�رձ���,��Ѱ����ҩƷ",
            "UI/Sprite/KeyboardSprite/E", "UI/Sprite/KeyboardSprite/Esc",
            () => !UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("�ƶ���΢���������Դ��ؿ���\n(����Ҽ�)�����Դ������ѹ�",
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