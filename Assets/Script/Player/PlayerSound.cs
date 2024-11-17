using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public AudioSource Source { get; private set; }

    private List<AudioClip> _glassBreakClips;
    // Start is called before the first frame update
    void Start()
    {
        Source = GetComponent<AudioSource>();
        _glassBreakClips = Resources.LoadAll<AudioClip>("Sound/Glass").ToList();
    }

    public void PlayGlassBreakSound()
    {
        Source.PlayOneShot(_glassBreakClips[Random.Range(0,_glassBreakClips.Count)]);
    }
}
