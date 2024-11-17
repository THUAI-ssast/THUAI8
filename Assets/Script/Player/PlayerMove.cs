using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering.Universal;

public class PlayerMove : NetworkBehaviour
{
    private LineRenderer _pathLineRenderer;
    private bool _isMoving;
    private Transform _spriteDisplay;

    [SyncVar] private Vector3Int _tilePosition; // ����ͬ�����ֶ�

    private PlayerSound _playerSound;

    public Vector3Int TilePosition
    {
        get => _tilePosition;
        private set => _tilePosition = value;
    }


    private void Start()
    {
        _pathLineRenderer = GetComponent<LineRenderer>();
        _spriteDisplay = transform.Find("SpriteDisplay");

        _playerSound = GetComponent<PlayerSound>();
        if (this.isLocalPlayer)
        {
            GridMoveController.Instance.InitLocalPlayer(this);
            tag = "LocalPlayer";
        }
        else
        {
            GetComponentInChildren<Light2D>().enabled = false;
        }
    }


    public bool SetPosition(Vector3 worldPosition, Vector3Int tilePosition)
    {
        if (_isMoving)
            return false;
        _isMoving = true;
        var position = transform.position;
        float duration = (worldPosition - position).magnitude * 0.6f;
        StartCoroutine(drawPathLine(worldPosition, duration));
        Vector3 moveVector = worldPosition - position;
        float angle = (moveVector.y > 0 ? 1 : -1) * Vector3.Angle(Vector3.right, moveVector);
        transform.DORotate(new Vector3(0, 0, angle), 0.2f).SetEase(Ease.Linear);
        transform.DOMove(worldPosition, duration);
        TilePosition = tilePosition; // ����ͬ��λ��
        return true;
    }
    /// <summary>
    /// ��player���õ���Ӧλ���ϣ�����·���ƶ�����
    /// </summary>
    /// <param name="worldPosition">Ŀ��λ�õ���������</param>
    /// <param name="tilePosition">Ŀ��λ�õ�Tilemap����</param>
    /// <param name="path">�ƶ�·����ʹ���������꣬���������ƶ�����</param>
    [Command]
    public void SetPosition(Vector3 worldPosition, Vector3Int tilePosition, Vector3[] path)
    {
        if (_isMoving || path.Length < 2)
            return;
        _isMoving = true;
        float duration = (path.Length - 1) * 0.6f;
        StartCoroutine(drawPathLine(path, duration));
        Vector3 firstMoveVector = path[1] - path[0];
        float firstAngle = (firstMoveVector.y > 0 ? 1 : -1) * Vector3.Angle(Vector3.right, firstMoveVector);
        Vector3 endMoveVector = path[^1] - path[^2];
        float angle = (endMoveVector.y > 0 ? 1 : -1) * Vector3.Angle(Vector3.right, endMoveVector);
        setRotateAnimation(firstAngle,angle,duration);

        transform.DOPath(path, duration);
        SetTilePos(tilePosition);
        //_tilePosition = tilePosition; // ����ͬ��λ��
        return;
    }

    /// <summary>
    /// ͬ�������ת�Ķ���
    /// </summary>
    /// <param name="firstAngle">��һ����ת��</param>
    /// <param name="angle">�ڶ�����ת��</param>
    /// <param name="duration"></param>
    [ClientRpc]
    private void setRotateAnimation(float firstAngle,float angle,float duration)
    {
        _spriteDisplay.DORotate(new Vector3(0, 0, firstAngle), duration * 0.15f).SetEase(Ease.Linear).OnComplete(() =>
            _spriteDisplay.DORotate(new Vector3(0, 0, angle), duration * 0.35f).SetEase(Ease.Linear));
    }

    [ClientRpc]
    private void SetTilePos(Vector3Int tilePosition)
    {
        _tilePosition = tilePosition;
    }

    // ͬ������
    [Command]
    public void CmdBreakGlass(Vector3Int cellPosition)
    {
        RpcBreakGlass(cellPosition);
    }

    [ClientRpc]
    private void RpcBreakGlass(Vector3Int cellPosition)
    {
        GridMoveController.Instance.BreakGlass(cellPosition,_playerSound);
    }

    // ͬ����
    [Command]
    public void CmdRotateDoor(Vector3Int doorPosition)
    {
        RpcRotateDoor(doorPosition);
    }

    [ClientRpc]
    private void RpcRotateDoor(Vector3Int doorPosition)
    {
        GridMoveController.Instance.RotateDoor(doorPosition);
    }

    /// <summary>
    /// ��ʾ��ҵ�ǰλ�ú�Ŀ��λ�ü��ֱ��·��ָʾ��
    /// </summary>
    /// <param name="target">Ŀ��λ�ã�ʹ����������</param>
    /// <param name="duration">��ʾ����ʱ��</param>
    public IEnumerator drawPathLine(Vector3 target, float duration)
    {
        _pathLineRenderer.SetPositions(new[] { transform.position, target });
        _pathLineRenderer.enabled = true;
        yield return new WaitForSeconds(duration);
        _pathLineRenderer.enabled = false;
        _isMoving = false;
    }

    /// <summary>
    /// ��ʾ·��ָʾ��
    /// </summary>
    /// <param name="path">��ʾ��·����ʹ����������</param>
    /// <param name="duration">��ʾ����ʱ��</param>
    public IEnumerator drawPathLine(Vector3[] path, float duration)
    {
        _pathLineRenderer.positionCount = path.Length;
        _pathLineRenderer.SetPositions(path);
        _pathLineRenderer.enabled = true;
        yield return new WaitForSeconds(duration);
        _pathLineRenderer.enabled = false;
        _isMoving = false;
    }
}