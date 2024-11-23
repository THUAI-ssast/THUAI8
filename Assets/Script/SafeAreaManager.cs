using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SafeAreaManager : NetworkBehaviour
{
    /// <summary>
    /// 安全区管理者的单例
    /// </summary>
    public static SafeAreaManager Instance;

    /// <summary>
    /// 安全区的大小改变策略，能够表示安全区在某一个回合的大小。
    /// key：世界回合计数；value：安全区的边长（安全区始终是正方形）。
    /// 到了key指定的世界回合，安全区的边长就更新为对应的value
    /// </summary>
    [Mirror.ReadOnly]
    private Dictionary<int, int> _safeAreaChangePlan = new Dictionary<int, int>
    {
        {1,168},
        {3,110},
        {5,91},
        {7,64},
        {9,45},
        {11,32},
        {13,16},
        {15,8},
        {16,-1}
    };

    /// <summary>
    /// 安全区的边长
    /// </summary>
    [SyncVar] private int _safeAreaLength;

    /// <summary>
    /// 安全区左下角所在的cellPosition
    /// </summary>
    [SyncVar] private Vector2Int _safeAreaOrigin ;

    /// <summary>
    /// 下一次安全区的边长
    /// </summary>
    [SyncVar] private int _nextSafeAreaLength = 168;

    /// <summary>
    /// 下一次安全区的左下角坐标，初始化时需要设置为全图的左下角
    /// </summary>
    [SyncVar] private Vector2Int _nextSafeAreaOrigin = new(-84,-84);

    /// <summary>
    /// 用于显示安全区，覆盖全图
    /// </summary>
    private Transform _wholeArea;
    /// <summary>
    /// 用于显示安全区，覆盖安全区部分
    /// </summary>
    private Transform _safeAreaMask;
    /// <summary>
    /// 用于显示安全区，为安全区描边
    /// </summary>
    private Transform _safeAreaOutline;


    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _wholeArea = transform.Find("WholeArea");
        _safeAreaMask = transform.Find("SafeAreaMask");
        _safeAreaOutline = transform.Find("SafeAreaOutline");

        _wholeArea.transform.localScale = new(168,168,0);
        _wholeArea.gameObject.SetActive(true);
        _safeAreaMask.gameObject.SetActive(true);
        _safeAreaOutline.gameObject.SetActive(true);
    }


    /// <summary>
    /// 更新安全区，包括逻辑上的更新和显示上的更新
    /// </summary>
    /// <param name="currentRoundCount">当前的世界回合计数</param>
    public void UpdateSafeAreaOnServer(int currentRoundCount)
    {
        int newLength;
        if (_safeAreaChangePlan.TryGetValue(currentRoundCount, out newLength))
        {
            if (newLength == -1)
                return;
            if (newLength != _nextSafeAreaLength)
            {
                Debug.Log("SafeArea has error");
                return;
            }
            // 确定新边长
            _safeAreaLength = _nextSafeAreaLength;
            // 确定左下角的新坐标
            _safeAreaOrigin = _nextSafeAreaOrigin;
            
            // 计算并存储下一次安全区的边长和左下角坐标
            for (int i = 1; i < newLength; i++)
            {
                int nextLength;
                if(_safeAreaChangePlan.TryGetValue(currentRoundCount+i, out nextLength))
                {
                    if (nextLength == -1) 
                        break;
                    _nextSafeAreaLength = nextLength;
                    _nextSafeAreaOrigin += ComputeOriginDelta(newLength, nextLength);
                    break;
                }
            }

            // 使所有客户端显示安全区
            RpcDisplaySafeArea();
        }
    }

    public void DoSafeAreaDamageByActionPoint(float decreaseActionPoint)
    {
        if(JudgeIsInSafeArea(GridMoveController.Instance.Player.TilePosition))
        {
            return;
        }
        PlayerHealth localPlayerHealth = GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerHealth>();
        localPlayerHealth.ChangeHealth((int)RandomSelectBodyPosition(), -decreaseActionPoint);
    }

    public void DoSafeAreaDamageByRoundEnd(int roundCount)
    {
        if (JudgeIsInSafeArea(GridMoveController.Instance.Player.TilePosition))
        {
            return;
        }
        PlayerHealth localPlayerHealth = GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerHealth>();
        for (int i = 0; i < roundCount; i++)
        {
            localPlayerHealth.ChangeHealth((int)RandomSelectBodyPosition(), -1);
        }
    }

    /// <summary>
    /// 根据当前安全区的边长和左下角坐标绘制安全区的显示
    /// </summary>
    [ClientRpc]
    private void RpcDisplaySafeArea()
    {
        // 绘制安全区，确定position为正方形的中心坐标，scale等于边长
        float halfLength = _safeAreaLength / 2.0f;
        _safeAreaMask.position = new Vector3(_safeAreaOrigin.x, _safeAreaOrigin.y) + new Vector3(halfLength, halfLength);
        _safeAreaMask.localScale = new(_safeAreaLength, _safeAreaLength);
        // 绘制安全区轮廓
        _safeAreaOutline.position = _safeAreaMask.position;
        _safeAreaOutline.localScale = _safeAreaMask.localScale + new Vector3(0.1f,0.1f);
    }

    private Vector2Int ComputeOriginDelta(int oldLength, int newLength)
    {
        int margin = (int)((oldLength - newLength) / 5);
        int upBound = oldLength - newLength - margin;
        int xDelta = UnityEngine.Random.Range(margin, upBound+1);
        int yDetla = UnityEngine.Random.Range(margin, upBound+1);
        return new Vector2Int(xDelta, yDetla);
    }

    private bool JudgeIsInSafeArea(Vector3Int playerPosition)
    {
        return (_safeAreaOrigin.x <= playerPosition.x && playerPosition.x < (_safeAreaOrigin.x + _safeAreaLength))
            && (_safeAreaOrigin.y <= playerPosition.y && playerPosition.y < (_safeAreaOrigin.y + _safeAreaLength));
    }

    private PlayerHealth.BodyPosition RandomSelectBodyPosition()
    {
        // 获取枚举类型的所有值
        var bodyPositions = Enum.GetValues(typeof(PlayerHealth.BodyPosition));
        // 随机选择一个枚举值
        var randomBody = (PlayerHealth.BodyPosition)bodyPositions.GetValue(UnityEngine.Random.Range(0, bodyPositions.Length));
        return randomBody;
    }
}
