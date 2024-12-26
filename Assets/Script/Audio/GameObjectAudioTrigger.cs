using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工具component，根据所处的GameObject的状态播放音频
/// </summary>
public class GameObjectAudioTrigger : MonoBehaviour
{
    /// <summary>
    /// 当此component被enable时播放的音频
    /// </summary>
    [SerializeField] private AudioClip _onEnableAudioClip;
    /// <summary>
    /// 当此component被disable时播放的音频
    /// </summary>
    [SerializeField] private AudioClip _onDisableAudioClip;
    /// <summary>
    /// 当此gameObject被destroy时播放的音频
    /// </summary>
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
