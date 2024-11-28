using UnityEngine;
using UnityEngine.UI;

public class ViewAround : MonoBehaviour
{
    private Button _button;
    private Button _backButton;
    private GameObject _battleUI;
    private Image _battleUIImage; // ������������͸����
    GameObject _roundUI;
    GameObject _playerInfoUI;

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
        _backButton = transform.parent.Find("BackButton").GetComponent<Button>();
        _battleUI = transform.parent.gameObject; // ���� _battleUI ����� UI ���
        _battleUIImage = _battleUI.GetComponent<Image>(); // ��ȡ���� Image ���

        _backButton.gameObject.SetActive(false); // ��ʼʱ _backButton ���ɼ�
        _button.onClick.AddListener(onClickCheckButton);
        _playerInfoUI = GameObject.Find("Canvas").transform.Find("PlayerInfoPanel").gameObject;
        _roundUI = GameObject.Find("Canvas").transform.Find("Round").gameObject;
    }

    // ��ť�������������UI����ʾ _backButton���������Ϊ͸��
    private void onClickCheckButton()
    {
        // ���� _battleUI �е�����������
        foreach (Transform child in _battleUI.transform)
        {
            if(child.name == "InterruptedMessagePanel")
            {
                if(FightingProcessManager.Instance.transform.GetChild(0).GetComponent<FightingProcess>().FightingInterrupted)
                {
                    continue;
                }
            }
            if (child != _backButton.transform) // �ų� _backButton
            {
                child.gameObject.SetActive(false); // �������� UI Ԫ��
            }
        }

        _backButton.gameObject.SetActive(true); // ��ʾ _backButton
        _playerInfoUI.SetActive(false); // ���������Ϣ���
        _roundUI.SetActive(false); // ���ػغ���Ϣ���

        // ���� _battleUI ����͸����
        if (_battleUIImage != null)
        {
            Color tempColor = _battleUIImage.color;
            tempColor.a = 0f; // ����͸����Ϊ 0 (��ȫ͸��)��ԭ��Ϊ100
            _battleUIImage.color = tempColor;
        }
    }
}
