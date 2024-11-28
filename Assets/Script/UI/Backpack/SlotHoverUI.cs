using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ���ڹ��������ͣ�ڲ���ʱ���ֵ�UI��ÿ�������͵�GameObject����Ҫ���أ����������е���Ʒ�ۡ���Դ���е���Ʒ�۵ȣ�
/// </summary>
public class SlotHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// �����ͣ��ʾUI
    /// </summary>
    private GameObject _mouseHoverPanel = null;

    /// <summary>
    /// UI��������ƶ���Э��
    /// </summary>
    private Coroutine _followMouseCoroutine = null;

    /// <summary>
    /// UI������λ�õ�ƫ��
    /// </summary>
    private Vector3 _panelBias;

    /// <summary>
    /// ���ж�Ӧ��Item����Ʒ
    /// </summary>
    private Item _item;


    private void Start()
    {
        _mouseHoverPanel = UIManager.Instance.MainCanvas.transform.Find("SlotHoverPanel").gameObject;
    }

    private void OnDisable()
    {
        // �ڲ۱�����Ϊ�Ǽ���״̬ʱ����Ҫ����ͣ��ʾ��UIҲ����
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
        _item = GetSlotItem();
        if (_item == null)
            return;
        // ���������ͣUI���ı����ݣ�������Э�̲���ʹUI�������
        _mouseHoverPanel.GetComponentInChildren<TextMeshProUGUI>().text = _item.ItemData.ItemDesc;
        _panelBias = (new Vector2(40,30) + _mouseHoverPanel.GetComponent<RectTransform>().rect.size * new Vector2(1.2f,1.2f)) * new Vector2(1,-1);
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
    /// ����ʹUI��������ƶ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator FollowMousePosition()
    {
        while(true)
        {
            _mouseHoverPanel.transform.position = Input.mousePosition + _panelBias;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// ��ȡ���е�Item����Ʒ�����������е���Ʒ�ۡ���Դ���е���Ʒ�۵ȣ�
    /// </summary>
    /// <returns>���е�Item����Ʒ</returns>
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
}
