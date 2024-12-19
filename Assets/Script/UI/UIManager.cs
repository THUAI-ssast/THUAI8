using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单例Manager，UI类，管理BattleScene内所有需要阻塞的UI的展示、交互行为
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public bool IsUIActivating => _activeUIList.Count > 0;
    public GameObject ExistingOperationMenu;
    public GameObject ExistingBodyPositionMenu;

    /// <summary>
    /// 背包UI界面
    /// </summary>
    public GameObject BagPanel
    {
        get => _bagPanel;
        private set => _bagPanel = value;
    }

    public GameObject BattlePanel
    {
        get => _battlePanel;
        private set => _battlePanel = value;
    }

    public Item FollowImage;

    public SlotMenuTrigger CurrentSlotMenuTrigger { get; set; } // here

    [SerializeField] private GameObject _bagPanel;
    [SerializeField] private GameObject _battlePanel;
    private GameObject _craftPanel;
    private Transform _craftContent;
    private GameObject _craftWayUIPrefab;
    private GameObject _bigMapPanel;

    private bool _isOnBattle = false;

    private Button _backButton;
    private Image _battleUIImage;

    private List<GameObject> _activeUIList = new List<GameObject>();

    public List<GameObject> ActiveUIList
    {
        get => _activeUIList;
        set => _activeUIList = value;
    }

    /// <summary>
    /// BattleScene中的Canvas，供其他GameObject直接使用
    /// </summary>
    [SerializeField] public GameObject MainCanvas;

    /// <summary>
    /// 悬停在屏幕上方的显示UI的列表，仅供DisplayHoverStatusPanel相关的过程使用
    /// </summary>
    private List<GameObject> _hoverStatusPanelList = new List<GameObject>();

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
        _craftWayUIPrefab = Resources.Load<GameObject>("UI/CraftWayUI");
        _craftPanel = _bagPanel.transform.Find("CraftPanel").gameObject;
        _craftContent = _craftPanel.transform.Find("Scroll View/Viewport/Content");


        //初始化craft way ui 需要其父物体active
        _bagPanel.SetActive(true);
        _battlePanel.SetActive(true);
        _craftPanel.SetActive(true);
        CraftWayUI.ClearItemList();
        foreach (CraftWayData craftWayData in Resources.LoadAll<CraftWayData>("ScriptableObject/CraftWay"))
        {
            Instantiate(_craftWayUIPrefab, _craftContent).GetComponent<CraftWayUI>().CraftWayData = craftWayData;
        }

        _bagPanel.SetActive(false);
        _battlePanel.SetActive(false);
        _craftPanel.SetActive(false);

        _bagPanel.transform.Find("BackButton").GetComponent<Button>().onClick
            .AddListener(() =>
            {
                setUIActive(_bagPanel, false);
                setUIActive(_craftPanel, false);
            });
        _bagPanel.transform.Find("CraftButton").GetComponent<Button>().onClick
            .AddListener(() => setUIActive(_craftPanel, true));

        _battlePanel.transform.Find("BackButton").GetComponent<Button>().onClick
            .AddListener(() =>
            {
                _backButton = _battlePanel.transform.Find("BackButton").GetComponent<Button>();
                _battleUIImage = _battlePanel.GetComponent<Image>();
                // 遍历 _battleUI 中的所有子物体
                foreach (Transform child in _battlePanel.transform)
                {
                    if(child.name == "InterruptedMessagePanel")
                    {
                        if(!FightingProcessManager.Instance.transform.GetChild(0).GetComponent<FightingProcess>().FightingInterrupted)
                        {
                            continue;
                        }
                    }
                    if (child != _backButton.transform) // 排除 _backButton
                    {
                        child.gameObject.SetActive(true); // 显示其他 UI 元素
                    }
                }
                _backButton.gameObject.SetActive(false);
                GameObject.Find("Canvas").transform.Find("PlayerInfoPanel").gameObject.SetActive(true); // 显示玩家信息面板
                GameObject.Find("Canvas").transform.Find("Round").gameObject.SetActive(true); // 显示回合信息面板

                if (_battleUIImage != null)
                {
                    Color tempColor = _battleUIImage.color;
                    tempColor.a = 100f; // 设置透明度为 100 (完全透明)，原本为 0
                    _battleUIImage.color = tempColor;
                }
            });

        _craftPanel.transform.Find("BackButton").GetComponent<Button>().onClick
            .AddListener(() => reverseUIActive(_craftPanel));
        _craftPanel.transform.Find("ApplyButton").GetComponent<Button>().onClick
            .AddListener(() => BackpackManager.Instance.DeployCraft(CraftWayUI.SelectedCraftWay));

        _bigMapPanel = MainCanvas.transform.Find("MapPanel/SmallMapMask/BigMapImage").gameObject;
    }

    void Update()
    {
        GameObject localPlayer = GameObject.FindWithTag("LocalPlayer");
        if (Input.GetKeyDown(KeyCode.E) && !_isOnBattle && localPlayer.GetComponent<PlayerHealth>().IsAlive)
        {
            ReversePanel(_bagPanel);
        }
        
        if ((Input.GetKeyDown(KeyCode.M)||Input.GetKeyDown(KeyCode.Tab)) && localPlayer.GetComponent<PlayerHealth>().IsAlive)
        {
            reverseMapPanel();
        }

        // if (Input.GetKeyDown(KeyCode.B))
        // {
        //     ReverseBattlePanel();
        // }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_activeUIList.Count > 0)
            {
                if (_activeUIList[^1] == MapUIManager.Instance.BigMapPanel && MapUIManager.Instance.IsDisplayBigMap)
                {
                    MapUIManager.Instance.DisplaySmallMap();
                }
                else
                {
                    reverseUIActive(_activeUIList[^1]);
                }
            }
        }
    }

    private void reverseUIActive(GameObject ui)
    {
        if (ui.activeSelf)
        {
            ui.SetActive(false);
            foreach (var activeUI in _activeUIList.FindAll(x=>x.transform.parent == ui.transform))
            {
                activeUI.SetActive(false);
                _activeUIList.Remove(activeUI);
            }
            _activeUIList.Remove(ui);
        }
        else
        {
            ui.SetActive(true);
            _activeUIList.Add(ui);
        }
    }

    public void AddActiveUI(GameObject ui)
    {
        if (!_activeUIList.Contains(ui))
        {
            _activeUIList.Add(ui);
        }
    }

    public void RemoveActiveUI(GameObject ui)
    {
        if (_activeUIList.Contains(ui))
            _activeUIList.Remove(ui);
    }


    public void setUIActive(GameObject ui, bool active)
    {
        if (ui.activeSelf == active)
            return;
        reverseUIActive(ui);
    }

    public void ReversePanel(GameObject panel)
    {
        reverseUIActive(panel);
        if (panel.activeSelf == false && ExistingOperationMenu != null)
        {
            Destroy(ExistingOperationMenu);
        }
        if (_bagPanel.activeSelf == false && ExistingBodyPositionMenu != null)
        {
            Destroy(ExistingBodyPositionMenu);
        }
    }

    public void ReverseBagPanel()
    {
        ReversePanel(_bagPanel);
    }

    private void reverseMapPanel()
    {
        if (MapUIManager.Instance.IsDisplayBigMap)
        {
            MapUIManager.Instance.DisplaySmallMap();
        }
        else
        {
            MapUIManager.Instance.DisplayBigMap();
        }
    }

    // public void ReverseBattlePanel()
    // {
    //     reverseUIActive(_battlePanel);

    //     if (_battlePanel.activeSelf)
    //     {
    //         _isOnBattle = true;
    //     }
    //     else
    //     {
    //         _isOnBattle = false;
    //     }

    //     if (_battlePanel.activeSelf == false && ExistingOperationMenu != null)
    //     {
    //         Destroy(ExistingOperationMenu);
    //     }

    //     BackpackManager.Instance.RefreshArmorDisplay();
    // }

    public void DestroyCurrentFollowImage()
    {
        UIManager.Instance.FollowImage = null;
        if (CurrentSlotMenuTrigger != null)
        {
            CurrentSlotMenuTrigger.DestroyFollowImage();
            CurrentSlotMenuTrigger = null; // 清空引用
        }
    }

    public int GetActiveUINumber => _activeUIList.Count;

    /// <summary>
    /// 用于在屏幕上方显示提醒玩家的UI，该UI会在1秒后逐渐淡化至不可见
    /// </summary>
    /// <param name="text">提醒玩家的文本内容</param>
    public void DisplayHoverStatusPanel(string text)
    {
        foreach (var panel in _hoverStatusPanelList)
        {
            panel.SetActive(false);
        }
        GameObject hoverStatusPanel = Instantiate(Resources.Load<GameObject>("UI/HoverStatusPanel"), MainCanvas.transform, false);
        hoverStatusPanel.GetComponentInChildren<TextMeshProUGUI>().text = text;
        _hoverStatusPanelList.Add(hoverStatusPanel);
        StartCoroutine(DisplayHoverStatusPanelCoroutine(hoverStatusPanel));
    }

    /// <summary>
    /// 在屏幕上方显示提醒玩家的UI
    /// </summary>
    /// <param name="hoverStatusPanel">在屏幕上方显示提醒玩家的UI对象</param>
    /// <returns></returns>
    private IEnumerator DisplayHoverStatusPanelCoroutine(GameObject hoverStatusPanel)
    {
        hoverStatusPanel.GetComponent<CanvasGroup>().alpha = 1f;
        hoverStatusPanel.SetActive(true);
        yield return new WaitForSeconds(1);
        hoverStatusPanel.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        yield return new WaitForSeconds(1);
        hoverStatusPanel.SetActive(false);
        _hoverStatusPanelList.Remove(hoverStatusPanel);
        Destroy(hoverStatusPanel);
    }

    public void AddKillLog(LogInfo.DamageType damageType, string victimName, string killerName = null)
    {
        GameObject KillLogPanel = MainCanvas.transform.Find("KillLogPanel").gameObject;
        GameObject newKillLog = Instantiate(Resources.Load<GameObject>("UI/KillLog"), KillLogPanel.transform);
        switch(damageType)
        {
            case LogInfo.DamageType.fight:
                newKillLog.transform.GetChild(0).GetComponent<TMP_Text>().text = killerName;
                newKillLog.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/Sprite/KillLog/Murder");
                newKillLog.transform.GetChild(2).GetComponent<TMP_Text>().text = victimName;
                break;
            case LogInfo.DamageType.poison:
                newKillLog.transform.GetChild(0).GetComponent<TMP_Text>().text = victimName;
                newKillLog.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/Sprite/KillLog/Death");
                newKillLog.transform.GetChild(2).gameObject.SetActive(false);
                break;
            case LogInfo.DamageType.other:
                break;
        }
        newKillLog.transform.SetAsFirstSibling();
        StartCoroutine(DestroyAfterSeconds(2, newKillLog));
    }
    
    IEnumerator DestroyAfterSeconds(int seconds, GameObject log)
    {
        yield return new WaitForSeconds(seconds);
        byte transparent = 255;
        Color32 color = log.transform.GetChild(0).GetComponent<TMP_Text>().color;
        while(transparent > 0)
        {
            log.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color32(color.r, color.g, color.b, transparent);
            log.transform.GetChild(1).GetComponent<Image>().color = new Color32(color.r, color.g, color.b, transparent);
            log.transform.GetChild(2).GetComponent<TMP_Text>().color = new Color32(color.r, color.g, color.b, transparent);
            transparent -= 1;
            yield return null;
        }
        Destroy(log);
    }
    public void UpdateAlivePlayersNumUI(int alivePlayerNum)
    {
        MainCanvas.transform.Find("AlivePlayersNum").GetChild(1).GetComponent<TMP_Text>().text = alivePlayerNum.ToString();
    }
}