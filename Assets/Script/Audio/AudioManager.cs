using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例Manager，管理本地音频(主要是UI音效)的播放
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// 类的单例
    /// </summary>
    public static AudioManager Instance;
    /// <summary>
    /// 摄像机声源，绑定在MainCamera上
    /// </summary>
    public AudioSource CameraSource;

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        CameraSource = GameObject.FindWithTag("MainCamera").GetComponent<AudioSource>();
    }
}
