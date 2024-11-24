using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 战斗流程。在两个玩家发生战斗时动态生成，客户端完成所有流程后由战斗流程管理器销毁。所有非ui变量均在服务端发生改变。
/// </summary>
public class FightingProcess : NetworkBehaviour
{
    /// <summary>
    /// 回合进攻方。
    /// </summary>
    private GameObject _attacker;
    /// <summary>
    /// 回合防守方。
    /// </summary>
    private GameObject _defender;
    /// <summary>
    /// 回合持续时间。
    /// </summary>
    private float _roundDuration;
    public float RoundDuration => _roundDuration;
    /// <summary>
    /// 回合结束标志。
    /// </summary>
    bool _finishRound;
    /// <summary>
    /// 进攻方服务端确认回合结束。
    /// </summary>
    bool _attackerCmdComfirmFightOver;
    /// <summary>
    /// 防守方服务端确认回合结束。
    /// </summary>
    bool _defenderCmdComfirmFightOver;
    bool _fightingInterrupted;
    public bool FightingInterrupted => _fightingInterrupted;
    uint _interruptedPlayerNetId;
    string _interruptText;
    GameObject _escapePlayer;
    GameObject _deadPlayer;
    GameObject _battleUI;
    GameObject _finishRoundButton;
    GameObject _escapeButton;
    GameObject _roundUI;
    [SyncVar] float _timer;
    public float Timer => _timer;
    [SyncVar] int _roundCount;
    public enum PlayerState
    {
        Attacker,
        Defender
    }
    void Awake()
    {
        _attackerCmdComfirmFightOver = false;
        _defenderCmdComfirmFightOver = false;
        _fightingInterrupted = false;
        _escapePlayer = null;
        _deadPlayer = null;
        _roundCount = 1;
        _roundDuration = 20f;
        _finishRound = false;
        _battleUI = GameObject.Find("Canvas").transform.Find("BattlePanel").gameObject; 
        _roundUI = _battleUI.transform.Find("RoundUI").gameObject;
        _finishRoundButton = _battleUI.transform.Find("FinishRoundButton").gameObject;
        _escapeButton = _battleUI.transform.Find("EscapeButton").gameObject;
        _finishRoundButton.GetComponent<Button>().interactable = false;
        _escapeButton.GetComponent<Button>().interactable = false;
        _battleUI.transform.Find("IntertruptedMessagePanel").gameObject.SetActive(false);
        Debug.Log("FightingProcess Awake");
    }
    public string GetAttackerName()
    {
        return _attacker.GetComponent<PlayerHealth>().Name;
    }
    public void StartFighting(GameObject attacker, GameObject defender)
    {
        if(isServer)
        {
            _attacker = attacker;
            _defender = defender;
            _timer = _roundDuration;
            StartCoroutine(RoundTimer());
        }
    }
    IEnumerator RoundTimer()
    {
        StartRoundOnServer();
        yield return new WaitForSeconds(2);
        while (!IsFightOver())
        {
            if (_timer > 0 && !_finishRound)
            {
                _timer -= Time.deltaTime;
                UpdateTimerText();
            }
            else
            {
                _timer = 0;
                EndRoundOnServer();
                yield return new WaitForSeconds(2);
                if(_fightingInterrupted && 
                    _defender.GetComponent<NetworkIdentity>().netId == _interruptedPlayerNetId)
                {
                    break;
                }
                StartRoundOnServer();
                yield return new WaitForSeconds(2);
            }
            yield return null;
        }
        StartCoroutine(FightOver());
    }
    IEnumerator FightOver()
    {
        // 战斗结束
        // 战斗日志输出
        _attacker.GetComponent<PlayerFight>().IsFighting = false;
        _defender.GetComponent<PlayerFight>().IsFighting = false;
        if(_escapePlayer != null)
        {
            TargetPlayerEscape(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
            TargetPlayerEscape(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        }
        else if(_deadPlayer != null)
        {
            TargetPlayerDead(_deadPlayer.GetComponent<NetworkIdentity>().connectionToClient, true);
            if(_deadPlayer.GetComponent<PlayerFight>().FightingState == PlayerState.Attacker)
            {
                TargetPlayerDead(_defender.GetComponent<NetworkIdentity>().connectionToClient, false);
            }
            else
            {
                TargetPlayerDead(_attacker.GetComponent<NetworkIdentity>().connectionToClient, false);
            }
        }
        else if(_fightingInterrupted)
        {
            TargetPlayerInterrupt(_attacker.GetComponent<NetworkIdentity>().connectionToClient);
            TargetPlayerInterrupt(_defender.GetComponent<NetworkIdentity>().connectionToClient); 
        }
        yield return new WaitForSeconds(2);
        while(_attackerCmdComfirmFightOver == false || _defenderCmdComfirmFightOver == false)
        {
            // Debug.Log("Unready attacker: " + _attackerCmdComfirmFightOver + " defender: " + _defenderCmdComfirmFightOver);
            yield return null;
        }
        // Debug.Log("ReadyToDestroy");
        FightingProcessManager.Instance.DestroyProcess(gameObject);
    }
    [TargetRpc]
    void TargetPlayerInterrupt(NetworkConnection target)
    {
        StartCoroutine(Interrupt(_interruptText));
    }
    [TargetRpc]
    void TargetPlayerEscape(NetworkConnection target, PlayerState playerState)
    {
        if(playerState == PlayerState.Attacker)
        {
            GridMoveController.Instance.ToggleMovementState(true);
            _battleUI.SetActive(false);
            GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
        }
        if(playerState == PlayerState.Defender)
        {
            StartCoroutine(Escape());
        }
    }
    [TargetRpc]
    void TargetPlayerDead(NetworkConnection target, bool isDead)
    {
        if(isDead)
        {
            _battleUI.SetActive(false);
            GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
        }
        else
        {
            StartCoroutine(Dead());
        }
    }
    IEnumerator Interrupt(string text)
    {
        _roundUI.transform.Find("FightingInterrupted").GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = text;
        _roundUI.transform.Find("FightingInterrupted").gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find("FightingInterrupted").gameObject.SetActive(false);
        GridMoveController.Instance.ToggleMovementState(true);
        _battleUI.SetActive(false);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
    }
    IEnumerator Dead()
    {
        _roundUI.transform.Find("PlayerDead").gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find("PlayerDead").gameObject.SetActive(false);
        GridMoveController.Instance.ToggleMovementState(true);
        _battleUI.SetActive(false);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
    }
    IEnumerator Escape()
    {
        _roundUI.transform.Find("PlayerEscape").gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find("PlayerEscape").gameObject.SetActive(false);
        GridMoveController.Instance.ToggleMovementState(true);
        _battleUI.SetActive(false);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
    }
    public void ConfirmOver(PlayerState playerState)
    {
        if(playerState == PlayerState.Attacker)
        {
            _attackerCmdComfirmFightOver = true;
        }
        if(playerState == PlayerState.Defender)
        {
            _defenderCmdComfirmFightOver = true;
        }
    }
    void StartRoundOnServer()
    {
        _timer = _roundDuration;
        _finishRound = false;
        // 开始当前进攻方的回合
        TargetStartRound(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        TargetStartRound(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
        _roundCount++;
        // 战斗日志输出
    }
    void EndRoundOnServer()
    {
        // 结束当前进攻方的回合
        TargetEndRound(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        TargetEndRound(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
        
        // 战斗日志输出
        // 切换进攻方防守方
        GameObject temp = _attacker;
        _attacker = _defender;
        _defender = temp;
    }
    [TargetRpc]
    void TargetEndRound(NetworkConnection target, PlayerState playerState)
    {
        Debug.Log(playerState + "EndRound");
        if(playerState == PlayerState.Attacker)
        {
            StartCoroutine(SetRoundUI("YourRoundEnd"));
            // 进攻方按钮变灰 不可点
            _finishRoundButton.GetComponent<Button>().interactable = false;
            _escapeButton.GetComponent<Button>().interactable = false;
            // 进攻方不可拖动
        }
        if(playerState == PlayerState.Defender)
        {
            StartCoroutine(SetRoundUI("EnemyRoundEnd"));
        }
    }
    [TargetRpc]
    void TargetStartRound(NetworkConnection target, PlayerState playerState)
    {
        Debug.Log(playerState + "StartRound");
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdSetFightingState(playerState);
        GameObject currentRound = _battleUI.transform.Find("CurrentRoundPanel").GetChild(0).gameObject;
        GameObject timer = _battleUI.transform.Find("CurrentRoundPanel").GetChild(1).gameObject;
        timer.GetComponent<TMPro.TextMeshProUGUI>().text = $"{_roundDuration}s/{_roundDuration}s";
        if(playerState == PlayerState.Attacker)
        {
            StartCoroutine(SetRoundUI("YourRoundStart"));
            currentRound.GetComponent<TMPro.TextMeshProUGUI>().text = $"第{_roundCount}回合\n你的回合";
            // 进攻方按钮变亮 可点
            _finishRoundButton.GetComponent<Button>().interactable = true;
            _escapeButton.GetComponent<Button>().interactable = true;
            // 进攻方可拖动
        }
        if(playerState == PlayerState.Defender)
        {
            StartCoroutine(SetRoundUI("EnemyRoundStart"));
            currentRound.GetComponent<TMPro.TextMeshProUGUI>().text = $"第{_roundCount}回合\n对方回合";
        }
    }
    IEnumerator SetRoundUI(string childName)
    {
        _roundUI.transform.Find(childName).gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find(childName).gameObject.SetActive(false);
    }
    void UpdateTimerText()
    {
        TargetUpdateTimeUI(_attacker.GetComponent<NetworkIdentity>().connectionToClient);   
        TargetUpdateTimeUI(_defender.GetComponent<NetworkIdentity>().connectionToClient);
    }
    [TargetRpc]
    void TargetUpdateTimeUI(NetworkConnection target)
    {
        string secondText = Mathf.CeilToInt(_timer).ToString();
        GameObject timer = _battleUI.transform.Find("CurrentRoundPanel").GetChild(1).gameObject;
        timer.GetComponent<TMPro.TextMeshProUGUI>().text= $"{secondText}s/{_roundDuration}s";
    }
    bool IsFightOver()
    {
        // 玩家逃跑、死亡或被打断的情况
        if(_deadPlayer != null || _escapePlayer != null)
        {
            return true;
        }
        return false;
    }
    public void PlayerEscape(GameObject player)
    {
        _escapePlayer = player;
    }
    public void PlayerDead(GameObject player)
    {
        _deadPlayer = player;
    }
    public void FinishRound()
    {
        Debug.Log("CmdFinishRound");
        _finishRound = true;
    }
    /// <summary>
    /// 打断战斗。
    /// </summary>
    /// <param name="isInterrupted">为1时，打断战斗；为0时，取消打断</param>
    /// <param name="attackerPlayerID">打断时，为攻击者Identity；取消打断时，为null</param>
    /// <param name="defenderPlayerID">打断时，为被攻击者Identity；取消打断时，为null</param>
    public void InterruptFighting(bool isInterrupted, NetworkIdentity attackerPlayerID, NetworkIdentity defenderPlayerID)
    {
        _fightingInterrupted = isInterrupted;
        _interruptedPlayerNetId = 0;

        string text = "";
        if(isInterrupted)
        { 
            _interruptedPlayerNetId = defenderPlayerID.netId;
            string attackerName = attackerPlayerID.GetComponent<PlayerHealth>().Name;
            string defenderName = defenderPlayerID.GetComponent<PlayerHealth>().Name;
            _interruptText = $"{attackerName}向{defenderName}发起攻击…";
            text = $"战斗打断：{attackerName}意图向{defenderName}发起攻击…";
        }
        TargetInterruptMessage(_attacker.GetComponent<NetworkIdentity>().connectionToClient, isInterrupted, text);
        TargetInterruptMessage(_defender.GetComponent<NetworkIdentity>().connectionToClient, isInterrupted, text);
    }
    [TargetRpc]
    void TargetInterruptMessage(NetworkConnection target, bool state, string text)
    {
        _battleUI.transform.Find("IntertruptedMessagePanel").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = text;
        _battleUI.transform.Find("IntertruptedMessagePanel").gameObject.SetActive(state);
    }
}
// TODO: 玩家断联 不可拖动 FightLog 阻塞ui 打断过程战斗结束 SeletedUI