using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI显示类，用于管理鼠标悬停在槽上时出现的UI，每个槽类型的GameObject都需要挂载（包括背包中的物品槽、资源点中的物品槽等）
/// </summary>
public class SlotHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 鼠标悬停显示UI
    /// </summary>
    private GameObject _mouseHoverPanel = null;

    /// <summary>
    /// UI跟随鼠标移动的协程
    /// </summary>
    private Coroutine _followMouseCoroutine = null;

    /// <summary>
    /// 槽中对应的Item类物品的信息
    /// </summary>
    private string _itemDesc;


    private void Start()
    {
        _mouseHoverPanel = UIManager.Instance.MainCanvas.transform.Find("SlotHoverPanel").gameObject;
    }

    private void OnDisable()
    {
        // 在槽被设置为非激活状态时，需要将悬停显示的UI也禁用
        if (_followMouseCoroutine != null)
        {
            StopCoroutine(_followMouseCoroutine);
        }
        if (_mouseHoverPanel != null)
        {
            _mouseHoverPanel.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _itemDesc = GetSlotItemDesc();
        if (_itemDesc == null)
        {
            return;
        }
        // 更新鼠标悬停UI的文本内容，并启动协程不断使UI跟随鼠标
        _mouseHoverPanel.GetComponentInChildren<TextMeshProUGUI>().text = _itemDesc;
        _followMouseCoroutine = StartCoroutine(FollowMousePosition());
        _mouseHoverPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_followMouseCoroutine != null)
        {
            StopCoroutine(_followMouseCoroutine);
        }
        _mouseHoverPanel.SetActive(false);
    }

    /// <summary>
    /// 不断使UI跟随鼠标移动
    /// </summary>
    /// <returns></returns>
    private IEnumerator FollowMousePosition()
    {
        while(true)
        {
            _mouseHoverPanel.transform.position = Input.mousePosition;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// 获取槽中的Item类物品（包括背包中的物品槽、资源点中的物品槽等）
    /// </summary>
    /// <returns>槽中的Item类物品</returns>
    private Item GetSlotItem()
    {
        Item item = null;
        if (GetComponent<SlotMenuTrigger>() != null)
        {
            item = GetComponent<SlotMenuTrigger>().GetItem();
        }
        else if (GetComponent<ArmorSlot>() != null)
        {
            item = GetComponent<ArmorSlot>().GetItem();
        }
        else if (GetComponent<RPSlot>() != null)
        {
            item =  GetComponent<RPSlot>().GetItem();
        }
        return item;
    }

    private string GetSlotItemDesc()
    {
        string itemDesc = null;
        
        Item item = GetSlotItem();
        if (item != null)
        {
            itemDesc = item.ItemData.ItemDesc;
        }
        else if (GetComponent<CraftWayUI>() != null)
        {
            itemDesc = "产物：" + GetComponent<CraftWayUI>().CraftWayData.ProductItem.ItemDesc;
        }

        return itemDesc;
    }
}
