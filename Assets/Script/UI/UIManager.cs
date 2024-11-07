using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理战斗场景内所有UI的展示、交互行为
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public bool IsUIActivating => _activeUIList.Count > 0;
    public GameObject ExistingOperationMenu;

    /// <summary>
    /// 背包UI界面
    /// </summary>
    public GameObject BagPanel{ get=>_bagPanel; private set=>_bagPanel=value; }
    [SerializeField]private GameObject _bagPanel;
    private GameObject _craftPanel;
    private Transform _craftContent;
    private GameObject _craftWayUIPrefab;


    private List<GameObject> _activeUIList = new List<GameObject>();

    /// <summary>
    /// BattleScene中的Canvas，供其他GameObject直接使用
    /// </summary>
    [SerializeField] public GameObject MainCanvas;

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
        _craftPanel.SetActive(true);
        CraftWayUI.ClearItemList();
        foreach (CraftWayData craftWayData in Resources.LoadAll<CraftWayData>("ScriptableObject/CraftWay"))
        {
            Instantiate(_craftWayUIPrefab,_craftContent).GetComponent<CraftWayUI>().CraftWayData = craftWayData;
        }
        _craftPanel.SetActive(false);
        _bagPanel.SetActive(false);

        _bagPanel.transform.Find("BackButton").GetComponent<Button>().onClick
            .AddListener(() =>
            {
                setUIActive(_bagPanel, false);
                setUIActive(_craftPanel, false);
            });
        _bagPanel.transform.Find("CraftButton").GetComponent<Button>().onClick
            .AddListener(() => setUIActive(_craftPanel, true));

        _craftPanel.transform.Find("BackButton").GetComponent<Button>().onClick
            .AddListener(() => reverseUIActive(_craftPanel));
        _craftPanel.transform.Find("ApplyButton").GetComponent<Button>().onClick
            .AddListener(() => BackpackManager.Instance.DeployCraft(CraftWayUI.SelectedCraftWay));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ReverseBagPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_activeUIList.Count > 0)
            {
                reverseUIActive(_activeUIList[^1]);
            }
        }

        // if(Input.GetMouseButtonDown(0))
        // {
        //     if(ExistingOperationMenu != null)
        //     {
        //         Destroy(ExistingOperationMenu);
        //     }
        // }
    }

    private void reverseUIActive(GameObject ui)
    {
        if (ui.activeSelf)
        {
            ui.SetActive(false);
            _activeUIList.Remove(ui);
        }
        else
        {
            ui.SetActive(true);
            _activeUIList.Add(ui);
        }
    }

    public void setUIActive(GameObject ui, bool active)
    {
        if (ui.activeSelf == active)
            return;
        reverseUIActive(ui);
    }    

    public void ReverseBagPanel()
    {
        reverseUIActive(_bagPanel);
        if (_bagPanel.activeSelf == false && ExistingOperationMenu != null)
        {
            Destroy(ExistingOperationMenu);
        }
    }
}