using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{
    private LineRenderer _pathLineRenderer;

    private bool _isMoving;
    public Vector3Int TilePosition { get; private set; }

    private void Start()
    {
        _pathLineRenderer = GetComponent<LineRenderer>();
    }


    public bool SetPosition(Vector3 worldPosition, Vector3Int tilePosition)
    {
        if (_isMoving)
            return false;
        _isMoving = true;
        var position = transform.position;
        float duration = (worldPosition - position).magnitude * 0.6f;
        drawPathLine(worldPosition,duration);
        Vector3 moveVector = worldPosition - position;
        float angle = (moveVector.y > 0 ? 1 : -1) * Vector3.Angle(Vector3.right, moveVector);
        transform.DORotate(new Vector3(0, 0, angle),0.2f).SetEase(Ease.Linear);
        transform.DOMove(worldPosition, duration);
        TilePosition = tilePosition;
        return true;
    }
    public bool SetPosition(Vector3 worldPosition, Vector3Int tilePosition, Vector3[] path)
    {
        if (_isMoving||path.Length<2)
            return false;
        _isMoving = true;
        var position = transform.position;
        float duration = (path.Length - 1) * 0.6f;
        drawPathLine(path, duration);
        Vector3 moveVector = path[^1] - path[^2];
        float angle = (moveVector.y > 0 ? 1 : -1) * Vector3.Angle(Vector3.right, moveVector);
        transform.DORotate(new Vector3(0, 0, angle), duration*0.5f).SetEase(Ease.Linear);
        transform.DOPath(path, duration);
        TilePosition = tilePosition;
        return true;
    }

    private async void drawPathLine(Vector3 target, float duration)
    {
        _pathLineRenderer.SetPositions(new []{transform.position,target});
        _pathLineRenderer.enabled = true;
        await Task.Delay((int)duration*1000);
        _pathLineRenderer.enabled = false;
        _isMoving = false;
    }
    private async void drawPathLine(Vector3[] path, float duration)
    {
        _pathLineRenderer.positionCount = path.Length;
        _pathLineRenderer.SetPositions(path);
        _pathLineRenderer.enabled = true;
        await Task.Delay((int)duration * 1000);
        _pathLineRenderer.enabled = false;
        _isMoving = false;
    }
}
