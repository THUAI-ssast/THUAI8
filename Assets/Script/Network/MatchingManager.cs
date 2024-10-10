using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MatchingManager : NetworkManager
{
    public static MatchingManager Instance;

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            MatchingManager.Instance = this;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update(); 
    }
}
