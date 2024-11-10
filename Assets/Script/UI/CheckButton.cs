using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckButton : MonoBehaviour
{
    private string newText = "Resources Checkable";
    private Button button;
    private TextMeshProUGUI buttonText;

    private GameObject _slots;
    private GameObject ResourcePoint;

    void Start()
    {
        ResourcePoint = transform.parent.parent.parent.gameObject;
        
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = $"Cost {ResourcePoint.GetComponent<ResourcePointController>().RequiredActionPoint} AP to Check";
        _slots = transform.parent.GetChild(0).GetChild(0).GetChild(0).gameObject;
        _slots.SetActive(false);
        button.onClick.AddListener(ChangeTextAndDisable);
    }

    void ChangeTextAndDisable()
    {
        buttonText.text = newText;
        button.interactable = false;

        _slots.SetActive(true);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerActionPoint>().DecreaseActionPoint(ResourcePoint.GetComponent<ResourcePointController>().RequiredActionPoint);
    }
}
