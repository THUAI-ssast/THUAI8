using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _audioClipOnHitButton;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            Camera.main.gameObject.GetComponent<AudioSource>().PlayOneShot(_audioClipOnHitButton);
        });
    }
}