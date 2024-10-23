using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// ��Ҫһ��isTrigger��Collider2D������target tag������Ҫcollider2D��rigidbody2D��
/// ������target Tags���������ʱ��������target Components��enable����ΪenableStateWhileInside��
/// ���˳�ʱ����֮����Ϊ!activeWhenEnter��
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AreaTagTrigger : MonoBehaviour
{
    /// <summary>
    /// Ŀ��tag�����ַ�����ʽ���룬����������Ҫcollider2D��rigidbody2D
    /// </summary>
    [SerializeField] private List<string> _targetTags;
    /// <summary>
    /// ��Ҫ����/�رյ�����Behaviour�б�
    /// </summary>
    [SerializeField] private List<Behaviour> _targetComponents;
    /// <summary>
    /// ��Ŀ����������target Component.enable��Ŀ��ֵ
    /// </summary>
    [SerializeField] private bool _enableStateWhileInside = true;
    /// <summary>
    /// �˳�ʱ�Ƿ���Ҫ��target Component.enable��Ϊ!activeWhenEnter
    /// </summary>
    [SerializeField] private bool _exitTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.tag);
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
