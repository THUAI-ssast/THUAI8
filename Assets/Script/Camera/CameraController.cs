using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

/// <summary>
/// 摄像机控制类，查找LocalPlayer并将其绑定为跟随目标
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;


    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(findLocalPlayer());
    }

    private IEnumerator findLocalPlayer()
    {
        while (!_camera.Follow)
        {
            var obj = GameObject.FindWithTag("LocalPlayer");
            _camera.Follow = obj ? obj.transform : null;
            yield return new WaitForSeconds(0.1f);
        }
    }
}