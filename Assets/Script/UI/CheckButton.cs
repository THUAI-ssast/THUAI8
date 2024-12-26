using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI行为类，在资源点UI内的搜刮按钮
/// </summary>
public class CheckButton : MonoBehaviour
{
    private string _newText = "已搜刮";
    private Button button;
    private TextMeshProUGUI buttonText;

    private GameObject _slots;
    private GameObject ResourcePoint;

    private static AudioClip _checkAudioClip;

    private Transform _searchIcon;

    void Start()
    {
        ResourcePoint = transform.parent.parent.parent.gameObject;
        if (_checkAudioClip == null)
        {
            _checkAudioClip = Resources.Load<AudioClip>("Sound/Action/翻找物品金属");
        }

        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = $"消耗 {ResourcePoint.GetComponent<ResourcePointController>().RequiredActionPoint} AP搜刮";
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
        if (GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerActionPoint>()
            .DecreaseActionPoint(ResourcePoint.GetComponent<ResourcePointController>().RequiredActionPoint))
        {
            buttonText.text = _newText;
            button.interactable = false;
            _searchIcon.gameObject.SetActive(true);
            AudioManager.Instance.CameraSource.PlayOneShot(_checkAudioClip);
            yield return new WaitForSeconds(1.3f);
            AudioManager.Instance.CameraSource.Stop();
            _searchIcon.gameObject.SetActive(false);
            _slots.SetActive(true);
        }
    }
}