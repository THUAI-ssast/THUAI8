using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
//�����������¼�ʱ��ʹ��Camera��Դ���Ŷ�Ӧ��Ƶ
/// </summary>
public class GameObjectAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _onEnableAudioClip;
    [SerializeField] private AudioClip _onDisableAudioClip;
    [SerializeField] private AudioClip _onDestroyAudioClip;

    private void OnEnable()
    {
        if (AudioManager.Instance.CameraSource&& _onEnableAudioClip)
        {
            AudioManager.Instance.CameraSource.PlayOneShot(_onEnableAudioClip);
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance.CameraSource&& _onDisableAudioClip)
        {
            AudioManager.Instance.CameraSource.PlayOneShot(_onDisableAudioClip);
        }
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance.CameraSource && _onDestroyAudioClip)
        {
            AudioManager.Instance.CameraSource.PlayOneShot(_onDestroyAudioClip);
        }
    }
}
