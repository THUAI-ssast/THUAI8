using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 物品槽右键菜单触发器。挂载在每一个slot上，用于触发右键菜单。
/// </summary>
public class SlotMenuTrigger : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// 右键菜单预制体
    /// </summary>
    [SerializeField] private GameObject _operationMenuPrefab;
    /// <summary>
    /// 物品描述面板预制体
    /// </summary>
    [SerializeField] private GameObject _itemDescriptionPanelPrefab;
    /// <summary>
    /// 定位背包面板ui
    /// </summary>
    private GameObject _bagPanel;
    /// <summary>
    /// 右键菜单
    /// </summary>
    private GameObject _operationMenu;
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
    /// 已存在的右键菜单，保证全局唯一右键菜单
    /// </summary>
    private GameObject _existingOperationMenu;
    /// <summary>
    /// 获取背包面板ui位置
    /// </summary>
    void Start()
    {
        _bagPanel = UIManager.Instance.BagPanel;
    }
    /// <summary>
    /// 设置物品
    /// </summary>
    /// <param name="item">该位置物品</param>
    public void SetItem(Item item)
    {
        _slotItem = item;
    }
    /// <summary>
    /// 监听鼠标右键点击事件。生成全局唯一菜单、加入菜单按钮点击事件。
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _slotItem != null)
        {
            if(_existingOperationMenu != null)
            {
                Destroy(_existingOperationMenu);
                return ;
            }
            if(UIManager.Instance.ExistingOperationMenu != null)
            {
                Destroy(UIManager.Instance.ExistingOperationMenu);
            }
            _operationMenu = Instantiate(_operationMenuPrefab, _bagPanel.transform.GetChild(2), false);
            _operationMenu.transform.position = gameObject.transform.position + new Vector3(60, -100, 0);
            UIManager.Instance.ExistingOperationMenu = _operationMenu;
            _existingOperationMenu = _operationMenu;
            _layout = _operationMenu.transform.GetChild(0);
            if(_slotItem != null)
            {
                _layout.GetChild(0).GetComponent<Button>().onClick.AddListener(() => 
                {
                    BackpackManager.Instance.UseItem(_slotItem);
                    Destroy(_operationMenu);
                });
                _layout.GetChild(1).GetComponent<Button>().onClick.AddListener(() => 
                {
                    BackpackManager.Instance.DropItem(_slotItem);
                    Destroy(_operationMenu);    
                });
            }
        }
    }
}
