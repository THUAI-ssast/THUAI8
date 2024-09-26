using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private LineRenderer _pathLineRenderer;

    private bool _isMoving;
    private Vector3Int _tilePosition;

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
        transform.DORotate(new Vector3(0, 0,Vector3.Angle(Vector3.right,worldPosition-position)),0.1f).SetEase(Ease.Linear);
        transform.DOMove(worldPosition, duration);
        _tilePosition = tilePosition;
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
}
