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
    private GameObject _existingOperationMenu;
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
