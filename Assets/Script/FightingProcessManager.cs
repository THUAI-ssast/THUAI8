using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FightingProcessManager : NetworkBehaviour
{
    public static FightingProcessManager Instance;
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
    public void CreateProcess(GameObject attacker, GameObject defender)
    {
        if(isServer)
        {
            GameObject process = Instantiate(_fightingProcess);
            process.transform.SetParent(gameObject.transform);
            attacker.GetComponent<PlayerFight>().SetFightingProcess(process.GetComponent<NetworkIdentity>());
            defender.GetComponent<PlayerFight>().SetFightingProcess(process.GetComponent<NetworkIdentity>());
            NetworkConnection attacker_conn = attacker.GetComponent<NetworkIdentity>().connectionToClient;
            NetworkConnection defender_conn = defender.GetComponent<NetworkIdentity>().connectionToClient;
            NetworkServer.Spawn(process, attacker_conn);
            NetworkServer.Spawn(process, defender_conn);
            TargetSetParent(attacker_conn, process.GetComponent<NetworkIdentity>());
            TargetSetParent(defender_conn, process.GetComponent<NetworkIdentity>());
        }
    }
    [TargetRpc]
    void TargetSetParent(NetworkConnection target, NetworkIdentity childId)
    {
        GameObject child = childId.gameObject;
        GameObject parent = FightingProcessManager.Instance.gameObject;

        if (child != null && parent != null)
        {
            child.transform.SetParent(parent.transform);
        }
    }
    public void DestroyProcess(GameObject process)
    {
        if(isServer)
        {
            NetworkServer.Destroy(process);
        }
    }
}