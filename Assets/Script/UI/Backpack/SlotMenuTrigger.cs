using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI行为类，物品槽右键菜单触发器。挂载在每一个slot上，用于触发右键菜单
/// </summary>
public class SlotMenuTrigger : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// 右键菜单预制体
    /// </summary>
    [SerializeField] private GameObject _operationMenuPrefab;
    /// <summary>
    /// 治疗身体部位菜单预制体
    /// </summary>
    [SerializeField] private GameObject _bodyPositionMenuPrefab;
    /// <summary>
    /// 物品描述面板预制体
    /// </summary>
    [SerializeField] private GameObject _itemDescriptionPanelPrefab;
    /// <summary>
    /// 定位背包面板ui
    /// </summary>
    private GameObject _bagPanel;
    /// <summary>
    /// 定位战斗面板ui
    /// </summary>
    private GameObject _battlePanel;
    /// <summary>
    /// 右键菜单
    /// </summary>
    private GameObject _operationMenu;
    /// <summary>
    /// 身体部位菜单
    /// </summary>
    private GameObject _bodyPositionMenu;
    /// <summary>
    /// 物品描述面板
    /// </summary>
    private GameObject _itemDescriptionPanel;
    /// <summary>
    /// 该位置物品
    /// </summary>
    private Item _slotItem = null;
    /// <summary>
    /// 右键菜单布局
    /// </summary>
    private Transform _layout;
    /// <summary>
    /// 右键菜单布局
    /// </summary>
    private Transform _bodyPositionLayout;
    /// <summary>
    /// 已存在的右键菜单，保证全局唯一右键菜单
    /// </summary>
    private GameObject _existingOperationMenu;
    /// <summary>
    /// 已存在的治疗身体部位菜单，保证全局唯一
    /// </summary>
    private GameObject _existingBodyPositionMenu;
    /// <summary>
    /// 临时的跟随鼠标的图片
    /// </summary>
    private static Image _followImage;

    public Image FollowImage
    {
        get => _followImage;
        set => _followImage = value;
    }

    /// <summary>
    /// 获取背包面板ui位置
    /// </summary>
    void Start()
    {
        _bagPanel = UIManager.Instance.BagPanel;
        _battlePanel = UIManager.Instance.BattlePanel;
    }
    /// <summary>
    /// 设置物品
    /// </summary>
    /// <param name="item">该位置物品</param>
    public void SetItem(Item item)
    {
        _slotItem = item;
    }
    public Item GetItem()
    {
        return _slotItem;
    }
    /// <summary>
    /// 监听鼠标左右键点击事件。生成全局唯一菜单、加入菜单按钮点击事件。
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 左键生成 followImage
        if (eventData.button == PointerEventData.InputButton.Left && gameObject.transform.IsChildOf(_battlePanel.transform) 
            && _slotItem != null)
        {
            if(GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().FightingState == FightingProcess.PlayerState.Defender)
            {
                UIManager.Instance.DisplayHoverStatusPanel("现在不是你的回合！");
                return ;
            }
            if(_slotItem.ItemData is not WeaponItemData)
            {
                UIManager.Instance.DisplayHoverStatusPanel("战斗中只能使用武器！");
                return ;
            }
            if(GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().QueryRemainingAP() < (_slotItem.ItemData as WeaponItemData).AttakAPCost)
            {
                UIManager.Instance.DisplayHoverStatusPanel("战斗回合内体力不足！");
                return ;
            }
            if(GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().FightingState == FightingProcess.PlayerState.Attacker)
            {
                if (_followImage != null)
                {
                    DestroyFollowImage();
                    UIManager.Instance.FollowImage = null;
                }
                UIManager.Instance.CurrentSlotMenuTrigger = this; // 注册当前实例

                Image itemImage = transform.GetChild(0).GetComponent<Image>(); // 获取当前slot中item的Image
                _followImage = Instantiate(itemImage, _battlePanel.transform);  // 在battlePanel上创建跟随Image
                _followImage.rectTransform.pivot = new Vector3(0.5f, 0.5f, 0); // 设置锚点为中心
                _followImage.raycastTarget = false; // 设置为不可交互
                UIManager.Instance.FollowImage = _slotItem;
            }
        }

        // 右键生成操作菜单
        if (eventData.button == PointerEventData.InputButton.Right && _slotItem != null)
        {
            if (_existingOperationMenu != null)
            {
                Destroy(_existingOperationMenu);
                return ;
            }
            if(UIManager.Instance.ExistingOperationMenu != null)
            {
                Destroy(UIManager.Instance.ExistingOperationMenu);
            }
            if (_existingBodyPositionMenu != null)
            {
                Destroy(_existingBodyPositionMenu);
                return;
            }
            if (UIManager.Instance.ExistingBodyPositionMenu != null)
            {
                Destroy(UIManager.Instance.ExistingBodyPositionMenu);
            }
            _operationMenu = Instantiate(_operationMenuPrefab, _bagPanel.transform.GetChild(2), false);
            _operationMenu.transform.position = gameObject.transform.position + new Vector3(60, -100, 0);
            UIManager.Instance.ExistingOperationMenu = _operationMenu;
            _existingOperationMenu = _operationMenu;
            _layout = _operationMenu.transform.GetChild(0);
            if(_slotItem != null)
            {
                if (_slotItem.ItemData is ArmorItemData armorData)
                {
                    var tmpText = _layout.GetChild(0).GetComponentInChildren<TMP_Text>();
                    tmpText.text = $"装备\n({PlayerHealth.BodyToChinese[armorData.EquipBodyPosition]})";
                    tmpText.fontSize = 11;
                }

                if (_slotItem.ItemData is not ArmorItemData&& _slotItem.ItemData is not MedicineItemData)
                {
                    _layout.GetChild(0).gameObject.SetActive(false);
                    _operationMenu.transform.localScale *= 0.7f;
                }
                _layout.GetChild(0).GetComponent<Button>().onClick.AddListener(() => 
                {
                    Destroy(_operationMenu);
                    if (_slotItem.ItemData is ArmorItemData)
                    {
                        BackpackManager.Instance.UseItem(_slotItem);
                    }
                    else if (_slotItem.ItemData is MedicineItemData medicineData)
                    {
                        string medicinename = _slotItem.ItemData.ItemName;
                        if (medicinename == "止痛药" || medicinename == "肾上腺素")
                        {
                            BackpackManager.Instance.UseItem(_slotItem);
                        }
                        else
                        {
                            _bodyPositionMenu = Instantiate(_bodyPositionMenuPrefab, _bagPanel.transform.GetChild(2), false);
                            _bodyPositionMenu.transform.position = gameObject.transform.position + new Vector3(60, -100, 0);
                            UIManager.Instance.ExistingBodyPositionMenu = _bodyPositionMenu;
                            _existingBodyPositionMenu = _bodyPositionMenu;
                            _bodyPositionLayout = _bodyPositionMenu.transform.GetChild(0);

                            if (!medicineData.BodyHealDictionary.ContainsKey(PlayerHealth.BodyPosition.Head))
                            {
                                _bodyPositionLayout.GetChild(0).GetComponent<Button>().interactable = false;
                            }
                            if (!medicineData.BodyHealDictionary.ContainsKey(PlayerHealth.BodyPosition.MainBody))
                            {
                                _bodyPositionLayout.GetChild(1).GetComponent<Button>().interactable = false;
                            }
                            if (!medicineData.BodyHealDictionary.ContainsKey(PlayerHealth.BodyPosition.Legs))
                            {
                                _bodyPositionLayout.GetChild(2).GetComponent<Button>().interactable = false;
                            }

                            _bodyPositionLayout.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                            {
                                BackpackManager.Instance.UseItem(_slotItem, false, true, false, false);
                                Destroy(_bodyPositionMenu);
                            });
                            _bodyPositionLayout.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
                            {
                                BackpackManager.Instance.UseItem(_slotItem, false, false, true, false);
                                Destroy(_bodyPositionMenu);
                            });
                            _bodyPositionLayout.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
                            {
                                BackpackManager.Instance.UseItem(_slotItem, false, false, false, true);
                                Destroy(_bodyPositionMenu);
                            });
                        }
                    }
                });
                _layout.GetChild(1).GetComponent<Button>().onClick.AddListener(() => 
                {
                    BackpackManager.Instance.DropItem(_slotItem);
                    Destroy(_operationMenu);    
                });
            }
        }
    }

    void Update()
    {
        if(Input.GetAxis("Mouse ScrollWheel") < 0f && _existingOperationMenu != null)
        {
            Destroy(_existingOperationMenu);
        }
        // 如果存在跟随图片，更新它的位置
        if (_followImage != null)
        {
            Vector2 mousePosition = Input.mousePosition;

            // bias
            Vector2 offset = new Vector2(28, -28);

            // 将屏幕空间的鼠标位置转换为UI空间的局部坐标
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _battlePanel.GetComponent<RectTransform>(),
                mousePosition,
                null,
                out localPosition
            );

            // 应用偏移量
            _followImage.rectTransform.localPosition = localPosition + offset;

            if (Input.GetMouseButtonDown(1))
            {
                DestroyFollowImage();
                UIManager.Instance.FollowImage = null;
            }
        }
    }

    public void DestroyFollowImage()
    {
        Destroy(_followImage.gameObject);
        _followImage = null;
    }
}
