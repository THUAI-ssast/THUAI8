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

    private static AudioClip _checkAudioClip;

    private Transform _searchIcon;

    void Start()
    {
        ResourcePoint = transform.parent.parent.parent.gameObject;
        if (_checkAudioClip==null)
        {
            _checkAudioClip = Resources.Load<AudioClip>("Sound/Action/∑≠’“ŒÔ∆∑Ω Ù");
        }
        
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = $"Cost {ResourcePoint.GetComponent<ResourcePointController>().RequiredActionPoint} AP to Check";
        _slots = transform.parent.GetChild(0).GetChild(0).GetChild(0).gameObject;
        _searchIcon = transform.parent.GetChild(2);
        _slots.SetActive(false);
        button.onClick.AddListener(onClickCheckButton);
    }

    private void onClickCheckButton()
    {
        StartCoroutine(ChangeTextAndDisable());
    }

    private IEnumerator ChangeTextAndDisable()
    {
        buttonText.text = newText;
        button.interactable = false;
        _searchIcon.gameObject.SetActive(true);
        AudioManager.Instance.CameraSource.PlayOneShot(_checkAudioClip);
        yield return new WaitForSeconds(1.3f);
        AudioManager.Instance.CameraSource.Stop();
        _searchIcon.gameObject.SetActive(false);
        _slots.SetActive(true);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerActionPoint>().DecreaseActionPoint(ResourcePoint.GetComponent<ResourcePointController>().RequiredActionPoint);
    }
}
