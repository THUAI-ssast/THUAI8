using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerFight : NetworkBehaviour
{
    [SerializeField] GameObject _battleUI;
    GameObject _attackRange;
    GameObject _selectedPlayer;
    bool _inAttackRange;
    void Start()
    {
        _attackRange = gameObject.transform.GetChild(4).gameObject;
        _attackRange.SetActive(false);
        _inAttackRange = false;
        _selectedPlayer = gameObject.transform.GetChild(5).gameObject;
        _selectedPlayer.SetActive(false);
        _battleUI = GameObject.Find("Canvas").transform.GetChild(4).gameObject;
    }
    void Update()
    {
        if(isLocalPlayer)
        {
            if(Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                _attackRange.SetActive(true);
                _attackRange.transform.localScale = new Vector3(3, 3, 1);
                float x = gameObject.transform.position.x;      
                float y = gameObject.transform.position.y;
                _attackRange.transform.position = new Vector3(x, y, gameObject.transform.position.z - 1);
                // _attackRange.transform.position = gameObject.transform.position;
            }
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                float x = gameObject.transform.position.x;      
                float y = gameObject.transform.position.y;
                _attackRange.transform.position = new Vector3(x, y, gameObject.transform.position.z - 1);
                // _attackRange.transform.position = gameObject.transform.position;
            }
            if(Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                _attackRange.SetActive(false);
            }
        }
    }
    // public void ToggleAttackRange(bool inAttackRange)
    // {
    //     _inAttackRange = inAttackRange;
    //     Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " ToggleInAttackRange:" + inAttackRange);
    // }
    void OnTriggerEnter2D(Collider2D Other)
    {
        if(gameObject.CompareTag("Player") && Other.gameObject.CompareTag("LocalPlayerAttackRange"))
        {
            Debug.Log("Enter");
            _inAttackRange = true;
            Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " ToggleInAttackRange:" + _inAttackRange);
        }
    }
    void OnTriggerExit2D(Collider2D Other)
    {
        if(gameObject.CompareTag("Player") && Other.gameObject.CompareTag("LocalPlayerAttackRange"))
        {
            Debug.Log("Exit");
            _inAttackRange = false;
            Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " ToggleInAttackRange:" + _inAttackRange);
        }
    }
    void OnMouseDown()
    {
        Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " Down inAttackRange:" + _inAttackRange);
        if(_inAttackRange)
        {
            GameObject localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
            localPlayer.GetComponent<PlayerFight>().StartFighting(localPlayer, gameObject);
        }
    }
    void OnMouseEnter()
    {
        Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " Enter inAttackRange:" + _inAttackRange);
        if(_inAttackRange)
        {
            _selectedPlayer.SetActive(true);
        }
    }
    void OnMouseExit()
    {
        Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " Exit inAttackRange:" + _inAttackRange);
        if(_inAttackRange)
        {
            _selectedPlayer.SetActive(false);
        }
    }

    [Command]
    public void StartFighting(GameObject attacker, GameObject defender)
    {
        Debug.Log("StartFighting");
        FightManager.Instance.StartFighting(attacker, defender);
        TargetStartFighting(attacker.GetComponent<NetworkIdentity>().connectionToClient, attacker);
        TargetStartFighting(defender.GetComponent<NetworkIdentity>().connectionToClient, defender);
    }
    [TargetRpc]
    void TargetStartFighting(NetworkConnection target, GameObject player)
    {
        _battleUI.SetActive(true);
    }
}