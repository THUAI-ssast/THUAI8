using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 工具类，
/// 需要一个isTrigger的Collider2D，所有target tag物体需要collider2D和rigidbody2D。
/// 当具有target Tags的物体进入时，将所有target Components的enable设置为enableStateWhileInside；
/// 当退出时，则反之，设为!activeWhenEnter。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AreaTagTrigger : MonoBehaviour
{
    /// <summary>
    /// 目标tag，以字符串形式传入，所有物体需要collider2D和rigidbody2D
    /// </summary>
    [SerializeField] private List<string> _targetTags;
    /// <summary>
    /// 需要被打开/关闭的所有Behaviour列表
    /// </summary>
    [SerializeField] private List<Behaviour> _targetComponents;
    /// <summary>
    /// 当目标物体进入后，target Component.enable的目标值
    /// </summary>
    [SerializeField] private bool _enableStateWhileInside = true;
    /// <summary>
    /// 退出时是否需要把target Component.enable设为!activeWhenEnter
    /// </summary>
    [SerializeField] private bool _exitTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_targetTags.Contains(other.tag))
        {
            _targetComponents.ForEach(c=>c.enabled = _enableStateWhileInside);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_exitTrigger&& _targetTags.Contains(other.tag))
        {
            _targetComponents.ForEach(c => c.enabled = !_enableStateWhileInside);
        }
    }
}
