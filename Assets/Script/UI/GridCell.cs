using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    private int _playerAmount;

    public int PlayerAmount
    {
        get => _playerAmount;
        set => _playerAmount = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
