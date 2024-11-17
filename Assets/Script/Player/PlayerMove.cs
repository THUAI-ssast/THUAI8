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

    [SyncVar] private Vector3Int _tilePosition; // 用于同步的字段

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
        TilePosition = tilePosition; // 更新同步位置
        return true;
    }
    /// <summary>
    /// 将player设置到对应位置上，生成路径移动动画
    /// </summary>
    /// <param name="worldPosition">目标位置的世界坐标</param>
    /// <param name="tilePosition">目标位置的Tilemap坐标</param>
    /// <param name="path">移动路径，使用世界坐标，用于生成移动动画</param>
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
        //_tilePosition = tilePosition; // 更新同步位置
        return;
    }

    /// <summary>
    /// 同步玩家旋转的动画
    /// </summary>
    /// <param name="firstAngle">第一个旋转角</param>
    /// <param name="angle">第二个旋转角</param>
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

    // 同步玻璃
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

    // 同步门
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
    /// 显示玩家当前位置和目标位置间的直线路径指示器
    /// </summary>
    /// <param name="target">目标位置，使用世界坐标</param>
    /// <param name="duration">显示持续时间</param>
    public IEnumerator drawPathLine(Vector3 target, float duration)
    {
        _pathLineRenderer.SetPositions(new[] { transform.position, target });
        _pathLineRenderer.enabled = true;
        yield return new WaitForSeconds(duration);
        _pathLineRenderer.enabled = false;
        _isMoving = false;
    }

    /// <summary>
    /// 显示路线指示器
    /// </summary>
    /// <param name="path">显示的路径，使用世界坐标</param>
    /// <param name="duration">显示持续时间</param>
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