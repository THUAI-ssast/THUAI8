using UnityEngine;
using UnityEngine.UI;

public class ViewAround : MonoBehaviour
{
    private Button _button;
    private Button _backButton;
    private GameObject _battleUI;
    private Image _battleUIImage; // 用来控制面板的透明度
    GameObject _roundUI;
    GameObject _playerInfoUI;

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
        _backButton = transform.parent.Find("BackButton").GetComponent<Button>();
        _battleUI = transform.parent.gameObject; // 假设 _battleUI 是这个 UI 面板
        _battleUIImage = _battleUI.GetComponent<Image>(); // 获取面板的 Image 组件

        _backButton.gameObject.SetActive(false); // 初始时 _backButton 不可见
        _button.onClick.AddListener(onClickCheckButton);
        _playerInfoUI = GameObject.Find("Canvas").transform.Find("PlayerInfoPanel").gameObject;
        _roundUI = GameObject.Find("Canvas").transform.Find("Round").gameObject;
    }

    // 按钮点击后隐藏其他UI并显示 _backButton，设置面板为透明
    private void onClickCheckButton()
    {
        // 遍历 _battleUI 中的所有子物体
        foreach (Transform child in _battleUI.transform)
        {
            if(child.name == "InterruptedMessagePanel")
            {
                if(FightingProcessManager.Instance.transform.GetChild(0).GetComponent<FightingProcess>().FightingInterrupted)
                {
                    continue;
                }
            }
            if (child != _backButton.transform) // 排除 _backButton
            {
                child.gameObject.SetActive(false); // 隐藏其他 UI 元素
            }
        }

        _backButton.gameObject.SetActive(true); // 显示 _backButton
        _playerInfoUI.SetActive(false); // 隐藏玩家信息面板
        _roundUI.SetActive(false); // 隐藏回合信息面板

        // 设置 _battleUI 面板的透明度
        if (_battleUIImage != null)
        {
            Color tempColor = _battleUIImage.color;
            tempColor.a = 0f; // 设置透明度为 0 (完全透明)，原本为100
            _battleUIImage.color = tempColor;
        }
    }
}
