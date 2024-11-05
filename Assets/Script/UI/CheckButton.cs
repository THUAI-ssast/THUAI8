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

    private GameObject[] resourceItems;

    void Start()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        resourceItems = GameObject.FindGameObjectsWithTag("ResourceUIItem");

        SetResourceUIItemsActive(false);

        button.onClick.AddListener(ChangeTextAndDisable);
    }

    void ChangeTextAndDisable()
    {
        buttonText.text = newText;
        button.interactable = false;

        SetResourceUIItemsActive(true);
    }

    void SetResourceUIItemsActive(bool isActive)
    {
        foreach (var item in resourceItems)
        {
            item.SetActive(isActive);
        }
    }
}
