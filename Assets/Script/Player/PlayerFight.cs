using System.Collections;
using Mirror;
using Mirror.Examples.Basic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家行为类，管理玩家战斗的发起和打断相关
/// </summary>
public class PlayerFight : NetworkBehaviour
{
    /// <summary>
    /// 是否正在战斗，服务端改变，向所有客户端同步。
    /// </summary>
    [SyncVar] public bool IsFighting;

    /// <summary>
    /// 回合战斗状态，仅在服务端改变，不向客户端同步。
    /// </summary>
    [SyncVar] public FightingProcess.PlayerState FightingState;
    /// <summary>
    /// 从玩家ID所属逃跑。服务端改变，向所有客户端同步。
    /// </summary>
    [SyncVar] public uint EscapeFromPlayerID;

    /// <summary>
    /// 定位BattlePanel。
    /// </summary>
    GameObject _battleUI;
    GameObject _mapUI;

    /// <summary>
    /// 定位攻击范围ui。
    /// </summary>
    GameObject _attackRange;

    /// <summary>
    /// 定位玩家被选中ui。
    /// </summary>
    GameObject _selectedUI;
    /// <summary>
    /// 存储战斗时的敌人，战斗结束后为上一次战斗的敌人。在战斗的客户端和服务端有存储，无同步。
    /// </summary>
    GameObject _enemy;
    public GameObject Enemy => _enemy;

    /// <summary>
    /// 存储开始战斗后生成的战斗流程GameObject，在战斗的客户端和服务端有存储，无同步。
    /// 战斗结束后，销毁。
    /// </summary>
    GameObject _fightingProcess;
    public GameObject FightingProcess => _fightingProcess;

    /// <summary>
    /// 定位打断战斗后等待战斗的UI。
    /// </summary>
    GameObject _waitingForFightUI;

    /// <summary>
    /// 是否在攻击范围内，仅在客户端改变，不向服务端同步，LocalPlayer始终为false。
    /// </summary>
    bool _inAttackRange;

    /// <summary>
    /// 打断战斗时被攻击的玩家
    /// </summary>
    NetworkIdentity _interruptedPlayerID;

    /// <summary>
    /// 初始化
    /// </summary>
    void Start()
    {
        EscapeFromPlayerID = 0;
        _interruptedPlayerID = null;
        IsFighting = false;
        _attackRange = gameObject.transform.Find("AttackRange").gameObject;
        _attackRange.SetActive(false);
        _inAttackRange = false;
        _selectedUI = gameObject.transform.Find("Selected").gameObject;
        _selectedUI.SetActive(false);
        _battleUI = GameObject.Find("Canvas").transform.Find("BattlePanel").gameObject;
        _waitingForFightUI = GameObject.Find("Canvas").transform.Find("WaitingForFight").gameObject;
        _mapUI = GameObject.Find("Canvas").transform.Find("MapPanel").gameObject;
    }

    /// <summary>
    /// 每帧更新，检测是否按下Alt键，显示攻击范围。当有UI被激活时，不显示攻击范围。
    /// </summary>
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

    /// <summary>
    /// 判断是否有UI界面被激活。
    /// </summary>
    /// <returns></returns>
    bool UIState()
    {
        if(_battleUI.activeSelf || _waitingForFightUI.activeSelf)
        {
            return true;
        }
        // if(UIManager.Instance.MainCanvas.transform.Find("PlayerVictory").gameObject.activeSelf)
        // {
        //     return true;
        // }
        if(UIManager.Instance.MainCanvas.transform.Find("PlayerDead").gameObject.activeSelf)
        {
            return true;
        }
        return false;
    }
 
    /// <summary>
    /// 碰撞检测。当该玩家物体进入LocalPlayer的攻击范围时（攻击范围是一个触发器），设置_inAttackRange为true。
    /// </summary>
    /// <param name="Other"></param>
    void OnTriggerEnter2D(Collider2D Other)
    {
        if(gameObject.CompareTag("Player") && Other.gameObject.CompareTag("LocalPlayerAttackRange") &&
            Other.gameObject.transform.parent.gameObject.GetComponent<PlayerFight>().EscapeFromPlayerID != gameObject.GetComponent<NetworkIdentity>().netId)
        {
            _inAttackRange = true;
        }
    }
    /// <summary>
    /// 碰撞检测。当该玩家物体离开LocalPlayer的攻击范围时（攻击范围是一个触发器），设置_inAttackRange为false。
    /// </summary>
    /// <param name="Other"></param>
    void OnTriggerExit2D(Collider2D Other)
    {
        if(gameObject.CompareTag("Player") && Other.gameObject.CompareTag("LocalPlayerAttackRange"))
        {
            _inAttackRange = false;
        }
    }

    /// <summary>
    /// 鼠标按下事件，当该玩家物体被点击时，如果在LocalPlayer攻击范围内，隐藏选中UI，调用CmdStartFighting。在客户端执行。
    /// </summary>
    void OnMouseDown()
    {
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

    /// <summary>
    /// 鼠标进入事件，在该玩家物体被鼠标悬浮时的第一帧，如果在LocalPlayer攻击范围内，禁用移动，显示选中UI。在客户端执行。
    /// </summary>
    void OnMouseEnter()
    {
        if(_inAttackRange)
        {
            GridMoveController.Instance.ToggleMovementState(false);
            _selectedUI.SetActive(true);
        }
    }

    /// <summary>
    /// 鼠标悬浮事件，在该玩家物体被鼠标悬浮时的每一帧，如果在LocalPlayer攻击范围内，禁用移动，显示选中UI；
    /// 如果不在LocalPlayer攻击范围内，无UI被激活时启用移动，隐藏选中UI。在客户端执行。
    /// </summary>
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

    /// <summary>
    /// 鼠标离开事件，在该玩家物体被鼠标离开时的第一帧，如果在LocalPlayer攻击范围内，启用移动，隐藏选中UI。在客户端执行。
    /// </summary>
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
        yield return new WaitForSeconds(3);
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

    /// <summary>
    /// 显示或隐藏打断战斗UI。显示UI时禁用移动，关闭战斗范围。在打断者的客户端执行。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="state">显示或隐藏ui。true为显示，false为隐藏</param>
    [TargetRpc]
    void TargetInterruptUI(NetworkConnection target, bool state)
    {
        GridMoveController.Instance.ToggleMovementState(!state);
        _waitingForFightUI.transform.GetChild(2).GetComponent<Button>().interactable = true;
        _waitingForFightUI.SetActive(state);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerFight>().SetAttackRangeFalse();
    }

    /// <summary>
    /// 更新打断战斗UI。在打断者的客户端执行。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="mode">模式1和2。2为即将开始战斗的界面。</param>
    /// <param name="roundDuration">战斗回合时长</param>
    /// <param name="timeLeft">战斗回合剩余时长</param>
    /// <param name="attackerName">打断者名字</param>
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

    /// <summary>
    /// 设置战斗状态。在服务端执行。
    /// </summary>
    /// <param name="state"></param>
    [Command]
    public void CmdSetFightingState(FightingProcess.PlayerState state)
    {
        FightingState = state;
    }

    /// <summary>
    /// 指向战斗流程GameObject。在服务端执行。
    /// </summary>
    /// <param name="processID">战斗流程NetworkIdentity</param>
    /// <param name="enemyPlayerID">对面玩家NetworkIdentity</param>
    public void DeploySet(NetworkIdentity processID, NetworkIdentity enemyPlayerID)
    {
        _fightingProcess = processID.gameObject;
        _enemy = enemyPlayerID.gameObject;
    }

    /// <summary>
    /// 开始战斗，生成战斗流程GameObject，调用战斗流程的StartFighting方法。在攻击者的服务端执行，从攻击者客户端调用。
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="defender">防守者</param>
    [Command]
    public void CmdStartFighting(GameObject attacker, GameObject defender)
    {
        FightingProcessManager.Instance.CreateProcess(attacker, defender);
        if(_fightingProcess == null)
        {
            return ;
        }
        _fightingProcess.GetComponent<FightingProcess>().StartFighting(attacker, defender);
        TargetStartFighting(attacker.GetComponent<NetworkIdentity>().connectionToClient, defender);
        defender.GetComponent<PlayerFight>().TargetStartFighting(defender.GetComponent<NetworkIdentity>().connectionToClient, attacker);
    }

    /// <summary>
    /// 双方客户端开始战斗。更新血量和护甲显示。关闭战斗范围。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="enemyPlayer">对面玩家</param>
    [TargetRpc]
    void TargetStartFighting(NetworkConnection target, GameObject enemyPlayer)
    {
        CmdSetIsFighting();
        _fightingProcess = FightingProcessManager.Instance.transform.GetChild(0).gameObject;
        _enemy = enemyPlayer;
        HealthPanelEnemy.Instance.SetEnemy(enemyPlayer);
        GridMoveController.Instance.ToggleMovementState(false);
        OpenBattleUI();
        BackpackManager.Instance.RefreshArmorBattleDisplay(enemyPlayer);
        RefreshPositionHealth(enemyPlayer);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerFight>().SetAttackRangeFalse();
    }

    /// <summary>
    /// 更新对面玩家的血量显示。在客户端执行。
    /// </summary>
    /// <param name="enemyPlayer"></param>
    void RefreshPositionHealth(GameObject enemyPlayer)
    {
        _battleUI.transform.Find("HealthPanel_enemy/Head").GetChild(0).GetComponent<TMP_Text>().text = 
            $"{enemyPlayer.GetComponent<PlayerHealth>().HeadHealth}/{enemyPlayer.GetComponent<PlayerHealth>().HeadMaxHealth}";
        _battleUI.transform.Find("HealthPanel_enemy/Body").GetChild(0).GetComponent<TMP_Text>().text = 
            $"{enemyPlayer.GetComponent<PlayerHealth>().BodyHealth}/{enemyPlayer.GetComponent<PlayerHealth>().BodyMaxHealth}";
        _battleUI.transform.Find("HealthPanel_enemy/Legs").GetChild(0).GetComponent<TMP_Text>().text = 
            $"{enemyPlayer.GetComponent<PlayerHealth>().LegHealth}/{enemyPlayer.GetComponent<PlayerHealth>().LegMaxHealth}";
    }

    void OpenBattleUI()
    {
        _battleUI.SetActive(true);
        if (!HealthPanelEnemy.Instance.IfStart)
        {
            PlayerActionPoint playerAP = HealthPanelEnemy.Instance.LocalPlayer.GetComponent<PlayerActionPoint>();
            float currentAP = playerAP.CurrentActionPoint;
            float maxAP = playerAP.MaxActionPoint;
            HealthPanelEnemy.Instance.UpdateActionPoint(currentAP, maxAP);
        }

        _mapUI.SetActive(false);
    }

    /// <summary>
    /// 设置IsFighting为true。在服务端执行。
    /// </summary>
    [Command]
    void CmdSetIsFighting()
    {
        IsFighting = true;
    }

    /// <summary>
    /// 玩家逃跑，调用DeployPlayerEscape。在服务端执行。
    /// </summary>
    [Command]
    public void CmdEscape()
    {
        StartCoroutine(FightingCDAfterEscape());
        _fightingProcess.GetComponent<FightingProcess>().DeployPlayerEscape(gameObject);
        gameObject.GetComponent<PlayerActionPoint>().DecreaseActionPoint(2);
    }

    /// <summary>
    /// 玩家逃跑后，5s内不能和同一玩家再次战斗。在服务端执行。
    /// </summary>
    /// <returns></returns>
    IEnumerator FightingCDAfterEscape()
    {
        EscapeFromPlayerID = _enemy.GetComponent<NetworkIdentity>().netId;
        yield return new WaitForSeconds(5);
        EscapeFromPlayerID = 0;
    }

    /// <summary>
    /// 玩家结束回合，调用DeployFinishRound。在服务端执行。
    /// </summary>
    [Command]
    public void CmdFinishRound()
    {
        _fightingProcess.GetComponent<FightingProcess>().DeployFinishRound();
        if(gameObject.GetComponent<PlayerActionPoint>().CurrentActionPoint < 1 && 
            _fightingProcess.GetComponent<FightingProcess>().RoundAPRemaining == _fightingProcess.GetComponent<FightingProcess>().RoundAPLimit)
        {
            gameObject.GetComponent<PlayerActionPoint>().IncreaseActionPoint(1);
        }
    }

    /// <summary>
    /// 玩家死亡，调用DeployPlayerDead。在服务端执行。
    /// </summary>
    [Command]
    public void CmdDead()
    {
        _fightingProcess.GetComponent<FightingProcess>().DeployPlayerDead(gameObject);
    }

    /// <summary>
    /// 玩家取消打断。在打断者服务端执行。
    /// </summary>
    [Command]
    public void CmdCancelInterrupt()
    {
        TargetInterruptUI(gameObject.GetComponent<NetworkIdentity>().connectionToClient, false);
        _interruptedPlayerID.gameObject.GetComponent<PlayerFight>().DeployCancelInterrupt();
    }

    /// <summary>
    /// 玩家取消打断。在被打断者的服务端执行。
    /// </summary>
    void DeployCancelInterrupt()
    {
        _fightingProcess.GetComponent<FightingProcess>().InterruptFighting(false, null, null);
    }

    /// <summary>
    /// 双方玩家确认结束回合。在双方服务端执行。
    /// </summary>
    [Command]
    public void CmdConfirmOver()
    {
        _fightingProcess.GetComponent<FightingProcess>().DeployConfirmOver(FightingState);
    }

    /// <summary>
    /// 关闭玩家战斗范围。
    /// </summary>
    public void SetAttackRangeFalse()
    {
        _attackRange.SetActive(false);
    }

    /// <summary>
    /// 在战斗界面将武器拖到对方身上后被调用，表示检测到攻击，执行增加战斗日志，记录耗费AP及刷新护甲显示的流程。在服务端执行。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="costAP"></param>
    [Command]
    public void CmdAttackHappened(string message, float costAP)
    {
        _fightingProcess.GetComponent<FightingProcess>().DeployAddLog(message);
        _fightingProcess.GetComponent<FightingProcess>().DeployPlayAttackSound();
        _fightingProcess.GetComponent<FightingProcess>().DeployConsumeAP(costAP);
        _fightingProcess.GetComponent<FightingProcess>().DeployRefreshArmorDisplay();   
    }

    /// <summary>
    /// 查询剩余AP，在客户端执行，用于判断是否能使用武器。
    /// </summary>
    /// <returns></returns>
    public float QueryRemainingAP()
    {
        float ret = _fightingProcess.GetComponent<FightingProcess>().RoundAPRemaining;
        return ret;
    }
}