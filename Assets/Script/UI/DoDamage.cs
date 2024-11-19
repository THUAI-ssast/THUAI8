using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoDamage : MonoBehaviour
{
    private GameObject _enemyHead;
    private GameObject _enemyBody;
    private GameObject _enemyLegs;

    // Start is called before the first frame update
    void Start()
    {
        _enemyHead = gameObject.transform.GetChild(0).gameObject;
        _enemyBody = gameObject.transform.GetChild(1).gameObject;
        _enemyLegs = gameObject.transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
