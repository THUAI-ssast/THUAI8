using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// UI数据类，选择出生点时的跳伞地图方格
/// </summary>
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
}
