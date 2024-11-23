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
    /// ��ȫ�������ߵĵ���
    /// </summary>
    public static SafeAreaManager Instance;

    /// <summary>
    /// ��ȫ���Ĵ�С�ı���ԣ��ܹ���ʾ��ȫ����ĳһ���غϵĴ�С��
    /// key������غϼ�����value����ȫ���ı߳�����ȫ��ʼ���������Σ���
    /// ����keyָ��������غϣ���ȫ���ı߳��͸���Ϊ��Ӧ��value
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
    /// ��ȫ���ı߳�
    /// </summary>
    [SyncVar] private int _safeAreaLength;

    /// <summary>
    /// ��ȫ�����½����ڵ�cellPosition
    /// </summary>
    [SyncVar] private Vector2Int _safeAreaOrigin ;

    /// <summary>
    /// ��һ�ΰ�ȫ���ı߳�
    /// </summary>
    [SyncVar] private int _nextSafeAreaLength = 168;

    /// <summary>
    /// ��һ�ΰ�ȫ�������½����꣬��ʼ��ʱ��Ҫ����Ϊȫͼ�����½�
    /// </summary>
    [SyncVar] private Vector2Int _nextSafeAreaOrigin = new(-84,-84);

    /// <summary>
    /// ������ʾ��ȫ��������ȫͼ
    /// </summary>
    private Transform _wholeArea;
    /// <summary>
    /// ������ʾ��ȫ�������ǰ�ȫ������
    /// </summary>
    private Transform _safeAreaMask;
    /// <summary>
    /// ������ʾ��ȫ����Ϊ��ȫ�����
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
    /// ���°�ȫ���������߼��ϵĸ��º���ʾ�ϵĸ���
    /// </summary>
    /// <param name="currentRoundCount">��ǰ������غϼ���</param>
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
            // ȷ���±߳�
            _safeAreaLength = _nextSafeAreaLength;
            // ȷ�����½ǵ�������
            _safeAreaOrigin = _nextSafeAreaOrigin;
            
            // ���㲢�洢��һ�ΰ�ȫ���ı߳������½�����
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

            // ʹ���пͻ�����ʾ��ȫ��
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
    /// ���ݵ�ǰ��ȫ���ı߳������½�������ư�ȫ������ʾ
    /// </summary>
    [ClientRpc]
    private void RpcDisplaySafeArea()
    {
        // ���ư�ȫ����ȷ��positionΪ�����ε��������꣬scale���ڱ߳�
        float halfLength = _safeAreaLength / 2.0f;
        _safeAreaMask.position = new Vector3(_safeAreaOrigin.x, _safeAreaOrigin.y) + new Vector3(halfLength, halfLength);
        _safeAreaMask.localScale = new(_safeAreaLength, _safeAreaLength);
        // ���ư�ȫ������
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
        // ��ȡö�����͵�����ֵ
        var bodyPositions = Enum.GetValues(typeof(PlayerHealth.BodyPosition));
        // ���ѡ��һ��ö��ֵ
        var randomBody = (PlayerHealth.BodyPosition)bodyPositions.GetValue(UnityEngine.Random.Range(0, bodyPositions.Length));
        return randomBody;
    }
}
