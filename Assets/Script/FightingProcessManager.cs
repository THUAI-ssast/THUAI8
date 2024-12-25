using DG.Tweening;
using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 单例Manager，战斗流程管理类。负责创建和销毁战斗流程
/// </summary>
public class FightingProcessManager : NetworkBehaviour
{
    /// <summary>
    /// 单例模式
    /// </summary>
    public static FightingProcessManager Instance;

    public AudioClip AttackAudioClip;
    /// <summary>
    /// 战斗流程的预制体
    /// </summary>
    [SerializeField] GameObject _fightingProcess;
    [SerializeField] GameObject _fightingIcon;
    Dictionary<GameObject, GameObject> processToIconMapping;
    void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            AttackAudioClip = Resources.Load<AudioClip>("Sound/Action/Attack");
        }
    }
    void Start()
    {
        processToIconMapping = new Dictionary<GameObject, GameObject>();
    }
    /// <summary>
    /// 创建战斗流程，在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    public void CreateProcess(GameObject attacker, GameObject defender)
    {
        if(isServer)
        {
            // foreach (var existedProcess in processToIconMapping)
            // {
            //     if(existedProcess.Key.GetComponent<FightingProcess>().GetAttackerId() == attacker.GetComponent<NetworkIdentity>().netId && 
            //         existedProcess.Key.GetComponent<FightingProcess>().GetDefenderId() == defender.GetComponent<NetworkIdentity>().netId)
            //     {
            //         return ;
            //     }
            // }
            GameObject process = Instantiate(_fightingProcess);
            GameObject icon = Instantiate(_fightingIcon);
            processToIconMapping.Add(process, icon);
            process.transform.SetParent(gameObject.transform);
            icon.transform.SetParent(GameObject.Find("GridParent").transform.GetChild(0).Find("ItemTilemap").transform);
            // Debug.LogError(attacker.transform.position + " " + defender.transform.position);
            icon.transform.position = (attacker.transform.position + defender.transform.position)/2;
            attacker.GetComponent<PlayerFight>().DeploySet(process.GetComponent<NetworkIdentity>(), defender.GetComponent<NetworkIdentity>());
            defender.GetComponent<PlayerFight>().DeploySet(process.GetComponent<NetworkIdentity>(), attacker.GetComponent<NetworkIdentity>());
            NetworkConnection attacker_conn = attacker.GetComponent<NetworkIdentity>().connectionToClient;
            NetworkConnection defender_conn = defender.GetComponent<NetworkIdentity>().connectionToClient;
            NetworkServer.Spawn(process, attacker_conn);
            NetworkServer.Spawn(process, defender_conn);
            NetworkServer.Spawn(icon);
            TargetSetParent(attacker_conn, process.GetComponent<NetworkIdentity>());
            TargetSetParent(defender_conn, process.GetComponent<NetworkIdentity>());
            RpcSetParent(icon.GetComponent<NetworkIdentity>());
        }
    }
    /// <summary>
    /// 在战斗双方客户端设置战斗流程的父物体
    /// </summary>
    /// <param name="target"></param>
    /// <param name="childId">战斗流程NetworkIdentity</param>
    [TargetRpc]
    void TargetSetParent(NetworkConnection target, NetworkIdentity childId)
    {
        GameObject child = childId.gameObject;
        GameObject parent = gameObject;

        if (child != null && parent != null)
        {
            child.transform.SetParent(parent.transform);
        }
    }
    [ClientRpc]
    void RpcSetParent(NetworkIdentity iconId)
    {
        GameObject icon = iconId.gameObject;
        GameObject parent = GameObject.Find("GridParent").transform.GetChild(0).Find("ItemTilemap").gameObject;

        if (icon != null && parent != null)
        {
            icon.transform.SetParent(parent.transform);
        }
        // Debug.LogError(icon.transform.position);
        // Debug.LogError(icon.transform.rotation);
        // Debug.LogError("icon路径: " + GetTransformPath(icon.transform));
    }
    string GetTransformPath(Transform target)
    {
        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }
    /// <summary>
    /// 销毁战斗流程，在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="process">要被销毁的战斗流程</param>
    public void DestroyProcess(GameObject process)
    {
        if(isServer)
        {
            GameObject icon = processToIconMapping[process];
            processToIconMapping.Remove(process);
            NetworkServer.Destroy(process);
            NetworkServer.Destroy(icon);
        }
    }
}