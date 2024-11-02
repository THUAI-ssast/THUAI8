using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotMenuTrigger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject _operationMenuPrefab;
    [SerializeField] private GameObject _itemDescriptionPanelPrefab;
    private GameObject _bagPanel;
    private GameObject _operationMenu;
    private GameObject _itemDescriptionPanel;
    private Item _slotItem = null;
    private Transform _layout;
    void Start()
    {
        _bagPanel = UIManager.Instance.BagPanel;
    }
    public void SetItem(Item item)
    {
        _slotItem = item;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _slotItem != null)
        {
            Debug.Log("Right Clicked");
            if(UIManager.Instance.MenuActivating == true)
            {
                UIManager.Instance.MenuActivating = false;
                Destroy(UIManager.Instance.ExistingOperationMenu);
                return ;
            }
            UIManager.Instance.MenuActivating = true;
            _operationMenu = Instantiate(_operationMenuPrefab);
            _operationMenu.transform.SetParent(_bagPanel.transform.GetChild(2));
            _operationMenu.transform.position = gameObject.transform.position + new Vector3(40, -50, 0);    
            UIManager.Instance.ExistingOperationMenu = _operationMenu;
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
//     public void OnPointerEnter(PointerEventData eventData)
//     {
//         if(_slotItem != null)
//         {
//             itemDescriptionPanel = Instantiate(_itemDescriptionPanelPrefab);
//             itemDescriptionPanel.transform.SetParent(_bagPanel.transform);
//             itemDescriptionPanel.transform.position = gameObject.transform.position + new Vector3(40, -50, 0);
//             itemDescriptionPanel.transform.GetChild(0).GetComponent<Text>().text = _slotItem.ItemData.ItemDesc;
//         }   
//     }
    
//     public void OnPointerExit(PointerEventData eventData)
//     {
//         if(itemDescriptionPanel != null)
//         {
//             Destroy(itemDescriptionPanel);
//         }
//     }
}
