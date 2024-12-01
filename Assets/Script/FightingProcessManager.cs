using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// 战斗流程管理类。负责创建和销毁战斗流程。
/// </summary>
public class FightingProcessManager : NetworkBehaviour
{
    /// <summary>
    /// 单例模式
    /// </summary>
    public static FightingProcessManager Instance;
    /// <summary>
    /// 战斗流程的预制体
    /// </summary>
    [SerializeField] GameObject _fightingProcess;
    void Awake()
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
    /// <summary>
    /// 创建战斗流程，在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    public void CreateProcess(GameObject attacker, GameObject defender)
    {
        if(isServer)
        {
            GameObject process = Instantiate(_fightingProcess);
            process.transform.SetParent(gameObject.transform);
            attacker.GetComponent<PlayerFight>().DeploySetFightingProcess(process.GetComponent<NetworkIdentity>());
            defender.GetComponent<PlayerFight>().DeploySetFightingProcess(process.GetComponent<NetworkIdentity>());
            NetworkConnection attacker_conn = attacker.GetComponent<NetworkIdentity>().connectionToClient;
            NetworkConnection defender_conn = defender.GetComponent<NetworkIdentity>().connectionToClient;
            NetworkServer.Spawn(process, attacker_conn);
            NetworkServer.Spawn(process, defender_conn);
            TargetSetParent(attacker_conn, process.GetComponent<NetworkIdentity>());
            TargetSetParent(defender_conn, process.GetComponent<NetworkIdentity>());
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
    /// <summary>
    /// 销毁战斗流程，在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="process">要被销毁的战斗流程</param>
    public void DestroyProcess(GameObject process)
    {
        if(isServer)
        {
            NetworkServer.Destroy(process);
        }
    }
}