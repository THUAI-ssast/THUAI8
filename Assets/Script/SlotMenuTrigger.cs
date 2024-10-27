using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotMenuTrigger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject _operationMenuPrefab;
    private GameObject _bagPanel;
    private GameObject operationMenu;
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
            if(UIManager.Instance.MenuActivating == false)
            {
                UIManager.Instance.MenuActivating = true;
            }
            else
            {
                Destroy(UIManager.Instance.ExistingOperationMenu);
            }
            operationMenu = Instantiate(_operationMenuPrefab);
            operationMenu.transform.SetParent(_bagPanel.transform);
            operationMenu.transform.position = gameObject.transform.position + new Vector3(40, -50, 0);    
            UIManager.Instance.ExistingOperationMenu = operationMenu;
            _layout = operationMenu.transform.GetChild(0);
            if(_slotItem != null)
            {
                _layout.GetChild(0).GetComponent<Button>().onClick.AddListener(() => 
                {
                    BackpackManager.Instance.UseItem(_slotItem);
                    Destroy(operationMenu);
                });
                _layout.GetChild(1).GetComponent<Button>().onClick.AddListener(() => 
                {
                    BackpackManager.Instance.RemoveItem(_slotItem);
                    Destroy(operationMenu);
                });
            }
        }
    }
}
