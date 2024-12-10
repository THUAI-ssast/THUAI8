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

        setState(new TutorialState("���(C��)����ͣ��ʾ\n�������ͣ�ڵؿ��Ͽ��Բ鿴��Ϣ",
            "UI/Sprite/KeyboardSprite/C", null, () => Input.GetKeyDown(KeyCode.C)));
        _stateQueue.Enqueue(new TutorialState("�ڵؿ��ϵ��(������)�ƶ�\nÿ�ƶ�1�������0.2AP",
    "UI/Sprite/KeyboardSprite/Mouse0", null,
    () => _playerObject.transform.position.x > 3));
        _stateQueue.Enqueue(new TutorialState("�ƶ�����Ʒ��\n(����Ҽ�)�����Ʒ����ʰȡ",
            "UI/Sprite/Tutorial/ItemExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _medicine.GetComponent<SpriteRenderer>().enabled == false, 0.8f));
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
            "UI/Sprite/KeyboardSprite/Escape", "UI/Sprite/KeyboardSprite/Esc",
            () => !UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("�ƶ���΢���������Դ��ؿ���\n(����Ҽ�)�����Դ������ѹ�",
            "UI/Sprite/Tutorial/ResourcePointExample", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _firstResourcePoitnUI.activeSelf));
        _stateQueue.Enqueue(new TutorialState("����·���ť����AP�ѹ�\n(������)�����Ʒ������뱳��",
            null, "UI/Sprite/KeyboardSprite/Mouse0",
            () => _firstRPSlots.activeSelf, 2));
        _stateQueue.Enqueue(new TutorialState("�ѵ���ҩƷ,��(Esc)�˳���Դ��\n�򿪱��������Լ�",
            "UI/Sprite/KeyboardSprite/Escape", "UI/Sprite/Tutorial/BagExample",
            () => UIManager.Instance.BagPanel.activeSelf, 2));
        _stateQueue.Enqueue(new TutorialState("�رձ���,�����ѹ�\n�ߵ����Ե��(����Ҽ�)��/����",
            "UI/Sprite/KeyboardSprite/Escape", "UI/Sprite/KeyboardSprite/Mouse1",
            () => _secondResourcePoitnSlots.activeInHierarchy));
        _stateQueue.Enqueue(new TutorialState("�ý�����ľ����������а�\n�򿪱������кϳ�",
            "UI/Sprite/Tutorial/BagExample", "UI/Sprite/Tutorial/CraftExample",
            () => _craftPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("�ܹ��ϳɵ��䷽���Ͻ�Ϊ��ɫ\n(���)ѡ�к����ϳɰ�ť",
            null, "UI/Sprite/Tutorial/CraftDeployExample",
            () => BackpackManager.Instance.ItemList.Find(i => i.ItemData.ItemName == "�а�")));
        _stateQueue.Enqueue(new TutorialState("�رձ���,�����ѹ�\n",
            "UI/Sprite/KeyboardSprite/Escape", null,
            () => !UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("�ƶ����ѹζ�������AP��\n��ʾ������ɫ���",
            "UI/Sprite/Tutorial/PlayerInfoExample", null,
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("��Ļ�Ϸ���ʾ�˵�ǰ����غ�\nÿ������غϿ�ʼʱ����12AP",
            "UI/Sprite/Tutorial/TurnDisplayExample", null,
            () => true, 10));
        _stateQueue.Enqueue(new TutorialState("������½ǰ�ť����ͶƱ�����غ�\n������Ҿ�ͶƱ�������һ�غ�",
            "UI/Sprite/Tutorial/EndTurnExample", "UI/Sprite/Tutorial/ConfirmEndExample",
            () => _playerActionPoint.CurrentActionPoint >= 12, 2));
        _stateQueue.Enqueue(new TutorialState("���Ͻ���ʾ��С��ͼ\n��(M��)�򿪴��ͼ",
            "UI/Sprite/Tutorial/MiniMapExample", "UI/Sprite/KeyboardSprite/M",
            () => Input.GetKeyDown(KeyCode.M), 1));
        _stateQueue.Enqueue(new TutorialState("��ͼ����ʾ�˵�ǰ��ȫ��\n�׿�������һ����ȫ��",
            null, null,
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("ÿ2������غϻ���С��ȫ��\n��ȫ�������Ѫ",
            null, null,
            () => true, 6));
        _stateQueue.Enqueue(new TutorialState("�����ѹ�\nѰ�����õ�����",
            null, null,
            () => _thirdResourcePoitnSlots.activeInHierarchy, 0));
        _stateQueue.Enqueue(new TutorialState("���������˺����ͺͻ����˺�\n������ͬ��λ�˺����ʲ�ͬ",
            "UI/Sprite/Tutorial/WeaponExample", null,
            () => !_thirdResourcePoitnSlots.activeInHierarchy, 2));
        _stateQueue.Enqueue(new TutorialState("���ڵĲ��Ͽ��Ժϳ�ֽ�ʻ�����\n����Ҽ���װ������",
            "UI/Sprite/Tutorial/ArmorExample2", "UI/Sprite/Tutorial/EquipExample",
            () => BackpackManager.Instance.ItemList.Find(
                i => i.ItemData.ItemName == "ֽ�ʻ���")));
        _stateQueue.Enqueue(new TutorialState("���׻�����ض����͵��˺�\n�����ֵ��������",
            "UI/Sprite/Tutorial/ArmorExample2", null,
            () => UIManager.Instance.BagPanel.activeSelf));
        _stateQueue.Enqueue(new TutorialState("�رձ���\n�ص������м���̽��",
            "UI/Sprite/KeyboardSprite/Escape", null,
            () => !UIManager.Instance.BagPanel.activeSelf, 0.5f));
        _stateQueue.Enqueue(new TutorialState("�Ҳ��в���,���������鷢������\n���ܻ��򸽽���ұ�¶���λ��",
            null, "UI/Sprite/Tutorial/GlassExample",
            () => _playerObject.transform.position.x > 20, 1));
        _stateQueue.Enqueue(new TutorialState("������һ������\n��סalt�������,���𹥻�",
            null, "UI/Sprite/Tutorial/FightExample",
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("���Լ��Ļغϵ�����ѡȡ����\nÿ�غ���������1AP",
            null, null,
            () => true, 5));
        _stateQueue.Enqueue(new TutorialState("ѵ������,ս����ʵսѧϰ\n�����˻����˵�",
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

        _playerActionPoint = _playerObject.GetComponent<PlayerActionPoint>();

        PlayerHealth playerHealth = _playerObject.GetComponent<PlayerHealth>();
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.MainBody, -5.5f);
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.Legs, -7.4f);
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
}