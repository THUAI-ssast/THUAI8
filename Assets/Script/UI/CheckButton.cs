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

    void Start()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(ChangeTextAndDisable);
    }

    void ChangeTextAndDisable()
    {
        buttonText.text = newText;
        button.interactable = false;
    }
}
