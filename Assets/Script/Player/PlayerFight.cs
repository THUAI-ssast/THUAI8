using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerFight : NetworkBehaviour
{
    /// <summary>
    /// 是否正在战斗，服务端改变，向客户端同步。
    /// </summary>
    [SyncVar] public bool IsFighting;
    /// <summary>
    /// 回合战斗状态，仅在服务端改变，不向客户端同步。
    /// </summary>
    public FightingProcess.PlayerState FightingState;
    /// <summary>
    /// 定位BattlePanel。
    /// </summary>
    GameObject _battleUI;
    /// <summary>
    /// 定位攻击范围ui。
    /// </summary>
    GameObject _attackRange;
    /// <summary>
    /// 定位玩家被选中ui。
    /// </summary>
    GameObject _selectedUI;
    /// <summary>
    /// 存储开始战斗后生成的战斗流程GameObject，在客户端和服务端都有改变，但无同步。
    /// </summary>
    GameObject _fightingProcess;
    GameObject _waitingForFightUI;
    /// <summary>
    /// 是否在攻击范围内，仅在客户端改变，不向服务端同步，LocalPlayer始终为false。
    /// </summary>
    bool _inAttackRange;
    NetworkIdentity _interruptedPlayerID;
    void Start()
    {
        _interruptedPlayerID = null;
        IsFighting = false;
        _attackRange = gameObject.transform.Find("AttackRange").gameObject;
        _attackRange.SetActive(false);
        _inAttackRange = false;
        _selectedUI = gameObject.transform.Find("Selected").gameObject;
        _selectedUI.SetActive(false);
        _battleUI = GameObject.Find("Canvas").transform.Find("BattlePanel").gameObject;
        _waitingForFightUI = GameObject.Find("Canvas").transform.Find("WaitingForFight").gameObject;
    }
    void Update()
    {
        if(isLocalPlayer && !UIState())
        {
            if(Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                _attackRange.SetActive(true);
                _attackRange.transform.localScale = new Vector3(3, 3, 1);
                float x = gameObject.transform.position.x;      
                float y = gameObject.transform.position.y;
                _attackRange.transform.position = new Vector3(x, y, gameObject.transform.position.z - 1);
            }
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                float x = gameObject.transform.position.x;      
                float y = gameObject.transform.position.y;
                _attackRange.transform.position = new Vector3(x, y, gameObject.transform.position.z - 1);
            }
            if(Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                _attackRange.SetActive(false);
            }
        }
    }
    bool UIState()
    {
        return _battleUI.activeSelf || _waitingForFightUI.activeSelf;
    }
    void OnTriggerEnter2D(Collider2D Other)
    {
        // Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " " + gameObject.tag + " " + IsFighting);
        if(gameObject.CompareTag("Player") && Other.gameObject.CompareTag("LocalPlayerAttackRange"))
        {
            // Debug.Log("Enter");
            _inAttackRange = true;
            // Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " ToggleInAttackRange:" + _inAttackRange);
        }
    }
    void OnTriggerExit2D(Collider2D Other)
    {
        if(gameObject.CompareTag("Player") && Other.gameObject.CompareTag("LocalPlayerAttackRange"))
        {
            // Debug.Log("Exit");
            _inAttackRange = false;
            // Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " ToggleInAttackRange:" + _inAttackRange);
        }
    }
    void OnMouseDown()
    {
        // Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + " Down inAttackRange:" + _inAttackRange);
        if(_inAttackRange)
        {
            _selectedUI.SetActive(false);
            GameObject localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
            if(IsFighting)
            {
                // 开始打断流程
                localPlayer.GetComponent<PlayerFight>().CmdInterruptFighting(gameObject.GetComponent<NetworkIdentity>());
                return;
            }
            localPlayer.GetComponent<PlayerFight>().CmdStartFighting(localPlayer, gameObject);
        }
    }
    /// <summary>
    /// 打断战斗流程，调用被打断者的打断战斗流程。在打断者的服务端执行。
    /// </summary>
    /// <param name="playerID">被打断者NetworkIdentity</param>
    [Command]
    void CmdInterruptFighting(NetworkIdentity playerID)
    {
        _interruptedPlayerID = playerID;
        playerID.gameObject.GetComponent<PlayerFight>().DeployInterruptFighting(gameObject.GetComponent<NetworkIdentity>());
    }
    /// <summary>
    /// 打断战斗流程。在被打断者的服务端执行。
    /// </summary>
    /// <param name="playerID">打断者NetworkIdentity</param>
    void DeployInterruptFighting(NetworkIdentity playerID) 
    {
        _fightingProcess.GetComponent<FightingProcess>().InterruptFighting(true, playerID, gameObject.GetComponent<NetworkIdentity>());
        TargetInterruptUI(playerID.connectionToClient, true);
        StartCoroutine(InterruptFighting(playerID));
    }
    void OnMouseEnter()
    {
        if(_inAttackRange)
        {
            GridMoveController.Instance.ToggleMovementState(false);
            _selectedUI.SetActive(true);
        }
    }
    void OnMouseOver()
    {
        if(_inAttackRange)
        {
            GridMoveController.Instance.ToggleMovementState(false);
            _selectedUI.SetActive(true);
        }
        else
        {
            if(!UIState()) GridMoveController.Instance.ToggleMovementState(true);
            _selectedUI.SetActive(false);
        }
    }
    void OnMouseExit()
    {
        if(_inAttackRange)
        {
            GridMoveController.Instance.ToggleMovementState(true);
            _selectedUI.SetActive(false);
        }
    }
    /// <summary>
    /// 打断战斗流程，处理打断者监听FightingProcess。在被打断者的服务端执行。
    /// </summary>
    /// <param name="playerID">打断者NetworkIdentity</param>
    /// <returns></returns>
    IEnumerator InterruptFighting(NetworkIdentity playerID)
    {
        float roundDuration = _fightingProcess.GetComponent<FightingProcess>().RoundDuration;
        float timer = _fightingProcess.GetComponent<FightingProcess>().Timer;
        string attackerName = _fightingProcess.GetComponent<FightingProcess>().GetAttackerName();
        while(IsFighting)
        {
            if(_fightingProcess.GetComponent<FightingProcess>().FightingInterrupted == false)
            {
                // TargetInterruptUI(playerID.connectionToClient, false);
                yield break;
            }
            TargetUpdateInterruptUI(playerID.connectionToClient, 0, roundDuration, timer, attackerName);
            timer = _fightingProcess.GetComponent<FightingProcess>().Timer;
            attackerName = _fightingProcess.GetComponent<FightingProcess>().GetAttackerName();
            yield return null;
        }
        TargetUpdateInterruptUI(playerID.connectionToClient, 1, roundDuration, timer, attackerName);
        yield return new WaitForSeconds(2);
        TargetInterruptUI(playerID.connectionToClient, false);
        playerID.GetComponent<PlayerFight>().TargetEnterStartFighting(playerID.connectionToClient, gameObject.GetComponent<NetworkIdentity>());
    }
    /// <summary>
    /// 开始战斗，调用CmdStartFighting。在打断者的客户端执行。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="playerID">被打断者NetworkIdentity</param>
    [TargetRpc]
    void TargetEnterStartFighting(NetworkConnection target, NetworkIdentity playerID)
    {
        CmdStartFighting(gameObject, playerID.gameObject);
    }
    [TargetRpc]
    void TargetInterruptUI(NetworkConnection target, bool state)
    {
        GridMoveController.Instance.ToggleMovementState(!state);
        _waitingForFightUI.transform.GetChild(2).GetComponent<Button>().interactable = true;
        _waitingForFightUI.SetActive(state);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerFight>().SetAttackRangeFalse();
    }
    [TargetRpc]
    void TargetUpdateInterruptUI(NetworkConnection target, int mode, float roundDuration, float timeLeft, string attackerName)
    {
        if(mode == 0)
        {
            string timeLeftStr = Mathf.CeilToInt(timeLeft).ToString();
            string text = $"{attackerName}的回合中({timeLeftStr}s/{roundDuration}s)…";
            _waitingForFightUI.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = text;
            return ;
        }
        if(mode == 1)
        {
            string text = "战斗即将开始…";
            _waitingForFightUI.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = text;
            _waitingForFightUI.transform.GetChild(2).GetComponent<Button>().interactable = false;
            return ;
        }
    }
    [Command]
    public void CmdSetFightingState(FightingProcess.PlayerState state)
    {
        FightingState = state;
    }
    public void SetFightingProcess(NetworkIdentity processID)
    {
        _fightingProcess = processID.gameObject;
    }
    /// <summary>
    /// 开始战斗，生成战斗流程GameObject，调用战斗流程的StartFighting方法。在攻击者的服务端执行，从攻击者客户端调用。
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    [Command]
    public void CmdStartFighting(GameObject attacker, GameObject defender)
    {
        Debug.Log("StartFighting");
        FightingProcessManager.Instance.CreateProcess(attacker, defender);
        _fightingProcess.GetComponent<FightingProcess>().StartFighting(attacker, defender);
        TargetStartFighting(attacker.GetComponent<NetworkIdentity>().connectionToClient, attacker);
        defender.GetComponent<PlayerFight>().TargetStartFighting(defender.GetComponent<NetworkIdentity>().connectionToClient, defender);
    }
    [TargetRpc]
    void TargetStartFighting(NetworkConnection target, GameObject player)
    {
        Debug.Log("TargetSetIsFighting " + gameObject.GetComponent<NetworkIdentity>().netId);
        CmdSetIsFighting();
        _fightingProcess = FightingProcessManager.Instance.transform.GetChild(0).gameObject;
        GridMoveController.Instance.ToggleMovementState(false);
        _battleUI.SetActive(true);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerFight>().SetAttackRangeFalse();
    }
    [Command]
    void CmdSetIsFighting()
    {
        IsFighting = true;
        Debug.Log("SetIsFighting" + gameObject.GetComponent<NetworkIdentity>().netId);
    }
    [Command]
    public void CmdEscape()
    {
        _fightingProcess.GetComponent<FightingProcess>().PlayerEscape(gameObject);
    }
    [Command]
    public void CmdFinishRound()
    {
        if(_fightingProcess == null)
        {
            Debug.Log("Cmd_fightingProcess is null. Please check the reference!");
        }
        _fightingProcess.GetComponent<FightingProcess>().FinishRound();
    }
    [Command]
    public void CmdDead()
    {
        _fightingProcess.GetComponent<FightingProcess>().PlayerDead(gameObject);
    }
    [Command]
    public void CmdCancelInterrupt()
    {
        TargetInterruptUI(gameObject.GetComponent<NetworkIdentity>().connectionToClient, false);
        _interruptedPlayerID.gameObject.GetComponent<PlayerFight>().DeployCancelInterrupt();
    }
    void DeployCancelInterrupt()
    {
        _fightingProcess.GetComponent<FightingProcess>().InterruptFighting(false, null, null);
    }
    [Command]
    public void CmdConfirmOver()
    {
        if(FightingState == FightingProcess.PlayerState.Attacker)
        {
            Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + "Attacker" + " ConfirmOver");
        }
        else
        {
            Debug.Log(gameObject.GetComponent<NetworkIdentity>().netId + "Defender" + " ConfirmOver");
        }
        _fightingProcess.GetComponent<FightingProcess>().ConfirmOver(FightingState);
    }
    public void SetAttackRangeFalse()
    {
        _attackRange.SetActive(false);
    }
}