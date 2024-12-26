using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 玩家行为类，管理以玩家为声源的音频播放
/// </summary>
public class PlayerSound : MonoBehaviour
{
    /// <summary>
    /// 玩家声源，会根据玩家位置移动
    /// </summary>
    public AudioSource Source { get; private set; }

    private List<AudioClip> _glassBreakClips;
    // Start is called before the first frame update
    void Start()
    {
        Source = GetComponent<AudioSource>();
        _glassBreakClips = Resources.LoadAll<AudioClip>("Sound/Glass").ToList();
    }
    /// <summary>
    /// 在玩家位置播放玻璃破碎声
    /// </summary>
    public void PlayGlassBreakSound()
    {
        Source.PlayOneShot(_glassBreakClips[Random.Range(0,_glassBreakClips.Count)]);
    }
}
