using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Mirror.Examples.Basic;

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
    /// 回合AP上限。
    /// </summary>
    public float RoundAPLimit => _roundAPLimit;
    float _roundAPLimit;

    /// <summary>
    /// 回合AP剩余。
    /// </summary>
    public float RoundAPRemaining => _roundAPRemaining;
    [SyncVar] float _roundAPRemaining;

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

    /// <summary>
    /// 战斗将被打断标志。
    /// </summary>
    [SyncVar] bool _fightingInterrupted;
    public bool FightingInterrupted => _fightingInterrupted;

    /// <summary>
    /// 打断战斗的玩家NetId
    /// </summary>
    uint _interruptedPlayerNetId;

    /// <summary>
    /// 打断战斗时显示的文本
    /// </summary>
    string _interruptText;

    /// <summary>
    /// 逃跑的玩家。
    /// </summary>
    GameObject _escapePlayer;

    /// <summary>
    /// 死亡的玩家。
    /// </summary>
    GameObject _deadPlayer;

    /// <summary>
    /// 定位BattleUI。
    /// </summary>
    GameObject _battleUI;

    /// <summary>
    /// 定位FinishRoundButton。
    /// </summary>
    GameObject _finishRoundButton;
    
    /// <summary>
    /// 定位EscapeButton。
    /// </summary>
    GameObject _escapeButton;

    /// <summary>
    /// 定位BattleRoundUI。
    /// </summary>
    GameObject _roundUI;

    /// <summary>
    /// 回合计时器。
    /// </summary>
    [SyncVar] float _timer;
    public float Timer => _timer;

    /// <summary>
    /// 回合计数。
    /// </summary>
    [SyncVar] int _roundCount;

    /// <summary>
    /// 玩家战斗状态，攻击或防守。
    /// </summary>
    public enum PlayerState
    {
        Attacker,
        Defender
    }

    /// <summary>
    /// 初始化战斗流程。
    /// </summary>
    void Awake()
    {
        _attackerCmdComfirmFightOver = false;
        _defenderCmdComfirmFightOver = false;
        _fightingInterrupted = false;
        _escapePlayer = null;
        _deadPlayer = null;
        _roundCount = 1;
        _roundDuration = 20f;
        _roundAPLimit = 1;
        _roundAPRemaining = _roundAPLimit;
        _finishRound = false;
        _battleUI = GameObject.Find("Canvas").transform.Find("BattlePanel").gameObject; 
        _roundUI = _battleUI.transform.Find("RoundUI").gameObject;
        _finishRoundButton = _battleUI.transform.Find("FinishRoundButton").gameObject;
        _escapeButton = _battleUI.transform.Find("EscapeButton").gameObject;
        _finishRoundButton.GetComponent<Button>().interactable = false;
        _escapeButton.GetComponent<Button>().interactable = false;
        _battleUI.transform.Find("InterruptedMessagePanel").gameObject.SetActive(false);
    }

    /// <summary>
    /// 获取进攻方名字。
    /// </summary>
    /// <returns></returns>
    public string GetAttackerName()
    {
        return _attacker.GetComponent<PlayerHealth>().Name;
    }

    /// <summary>
    /// 开始战斗。
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
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

    /// <summary>
    /// 战斗主流程。
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundTimer()
    {
        StartRoundOnServer();
        yield return new WaitForSeconds(2);
        while (!IsFightOver())
        {
            if (_timer > 0 && !_finishRound && _roundAPRemaining > 0)
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

    /// <summary>
    /// 战斗结束流程。
    /// </summary>
    /// <returns></returns>
    IEnumerator FightOver()
    {
        _attacker.GetComponent<PlayerFight>().IsFighting = false;
        _defender.GetComponent<PlayerFight>().IsFighting = false;
        LogManager.Instance.TargetDestroyAllLogDisplay(_attacker.GetComponent<NetworkIdentity>().connectionToClient);
        LogManager.Instance.TargetDestroyAllLogDisplay(_defender.GetComponent<NetworkIdentity>().connectionToClient);
        if(_escapePlayer != null)
        {
            // 玩家逃跑
            TargetPlayerEscape(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
            TargetPlayerEscape(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        }
        else if(_deadPlayer != null)
        {
            // 玩家死亡
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
            // 战斗打断
            TargetPlayerInterrupt(_attacker.GetComponent<NetworkIdentity>().connectionToClient, _interruptText);
            TargetPlayerInterrupt(_defender.GetComponent<NetworkIdentity>().connectionToClient, _interruptText); 
        }
        yield return new WaitForSeconds(2);
        while(_attackerCmdComfirmFightOver == false || _defenderCmdComfirmFightOver == false)
        {
            // 确认攻击方和防守方都结束战斗后销毁战斗流程
            yield return null;
        }
        FightingProcessManager.Instance.DestroyProcess(gameObject);
    }

    /// <summary>
    /// 打断战斗。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="text">战斗被打断时输出的文字</param>
    [TargetRpc]
    void TargetPlayerInterrupt(NetworkConnection target, string text)
    {
        StartCoroutine(Interrupt(text));
    }

    /// <summary>
    /// 玩家逃跑。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="playerState"></param>
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

    /// <summary>
    /// 玩家死亡。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isDead"></param>
    [TargetRpc]
    void TargetPlayerDead(NetworkConnection target, bool isDead)
    {
        if(isDead)
        {
            _battleUI.SetActive(false);
            // GameObject.Find("Canvas").transform.Find("PlayerDead").gameObject.SetActive(true);
            GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
        }
        else
        {
            StartCoroutine(Dead());
        }
    }

    /// <summary>
    /// 打断战斗流程，在双方客户端执行。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 玩家死亡流程，在非死亡方客户端执行。
    /// </summary>
    /// <returns></returns>
    IEnumerator Dead()
    {
        _roundUI.transform.Find("PlayerDead").gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find("PlayerDead").gameObject.SetActive(false);
        GridMoveController.Instance.ToggleMovementState(true);
        _battleUI.SetActive(false);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
    }

    /// <summary>
    /// 玩家逃跑流程，在防守方客户端执行。
    /// </summary>
    /// <returns></returns>
    IEnumerator Escape()
    {
        _roundUI.transform.Find("PlayerEscape").gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find("PlayerEscape").gameObject.SetActive(false);
        GridMoveController.Instance.ToggleMovementState(true);
        _battleUI.SetActive(false);
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdConfirmOver();
    }

    /// <summary>
    /// 确认战斗结束，在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="playerState">玩家战斗状态</param>
    public void DeployConfirmOver(PlayerState playerState)
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

    /// <summary>
    /// 在服务端开始回合。
    /// </summary>
    void StartRoundOnServer()
    {
        _timer = _roundDuration;
        _finishRound = false;
        _roundAPRemaining = _roundAPLimit;

        // 开始当前进攻方的回合
        TargetStartRound(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        TargetStartRound(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
        _roundCount++;
        // 战斗日志输出
        DeployAddLog($"{_attacker.GetComponent<PlayerHealth>().Name}的回合开始");
    }

    /// <summary>
    /// 在服务端结束回合。
    /// </summary>
    void EndRoundOnServer()
    {
        // 结束当前进攻方的回合
        TargetEndRound(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        TargetEndRound(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
        
        // 战斗日志输出
        DeployAddLog($"{_attacker.GetComponent<PlayerHealth>().Name}的回合结束");

        // 切换进攻方防守方
        GameObject temp = _attacker;
        _attacker = _defender;
        _defender = temp;
    }

    /// <summary>
    /// 在双方客户端结束回合。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="playerState">玩家战斗状态</param>
    [TargetRpc]
    void TargetEndRound(NetworkConnection target, PlayerState playerState)
    {
        if(playerState == PlayerState.Attacker)
        {
            if(UIManager.Instance.FollowImage)
            {
                UIManager.Instance.DestroyCurrentFollowImage();
            }
            StartCoroutine(SetRoundUI("YourRoundEnd"));
            // 进攻方按钮变灰 不可点
            _finishRoundButton.GetComponent<Button>().interactable = false;
            _escapeButton.GetComponent<Button>().interactable = false;
            
        }
        if(playerState == PlayerState.Defender)
        {
            StartCoroutine(SetRoundUI("EnemyRoundEnd"));
        }
    }

    /// <summary>
    /// 在双方客户端开始回合。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="playerState">玩家战斗状态</param>
    [TargetRpc]
    void TargetStartRound(NetworkConnection target, PlayerState playerState)
    {
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
            _battleUI.transform.Find("FinishRoundButton").GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = $"(0AP/{_roundAPLimit}AP)";
        }
        if(playerState == PlayerState.Defender)
        {
            StartCoroutine(SetRoundUI("EnemyRoundStart"));
            currentRound.GetComponent<TMPro.TextMeshProUGUI>().text = $"第{_roundCount}回合\n对方回合";
            _battleUI.transform.Find("FinishRoundButton").GetChild(1).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = $"(0AP/{_roundAPLimit}AP)";
        }
    }

    /// <summary>
    /// 显示BattleRoundUI中的子对象，在客户端执行。
    /// </summary>
    /// <param name="childName">子对象名称</param>
    /// <returns></returns>
    IEnumerator SetRoundUI(string childName)
    {
        _roundUI.transform.Find(childName).gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        _roundUI.transform.Find(childName).gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新计时器文本。在服务端被调用，服务端执行。
    /// </summary>
    void UpdateTimerText()
    {
        TargetUpdateTimeUI(_attacker.GetComponent<NetworkIdentity>().connectionToClient);   
        TargetUpdateTimeUI(_defender.GetComponent<NetworkIdentity>().connectionToClient);
    }

    /// <summary>
    /// 在双方客户端更新计时器文本。
    /// </summary>
    /// <param name="target"></param>
    [TargetRpc]
    void TargetUpdateTimeUI(NetworkConnection target)
    {
        string secondText = Mathf.CeilToInt(_timer).ToString();
        GameObject timer = _battleUI.transform.Find("CurrentRoundPanel").GetChild(1).gameObject;
        timer.GetComponent<TMPro.TextMeshProUGUI>().text= $"{secondText}s/{_roundDuration}s";
    }

    /// <summary>
    /// 判断战斗是否结束。
    /// </summary>
    /// <returns></returns>
    bool IsFightOver()
    {
        // 玩家逃跑、死亡的情况
        if(_deadPlayer != null || _escapePlayer != null)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 攻击方逃跑时调用。在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="player">逃跑的玩家</param>
    public void DeployPlayerEscape(GameObject player)
    {
        _escapePlayer = player;
    }

    /// <summary>
    /// 有一方死亡时调用。在服务端被调用，在服务端执行。
    /// </summary>
    /// <param name="player">死亡的玩家</param>
    public void DeployPlayerDead(GameObject player)
    {
        if(_deadPlayer.GetComponent<PlayerFight>().FightingState == PlayerState.Attacker)
        {
            _defender.GetComponent<PlayerLog>().DeployAddEliminationCount();
        }
        else
        {
            _attacker.GetComponent<PlayerLog>().DeployAddEliminationCount();
        }
        _deadPlayer = player;
    }

    /// <summary>
    /// 攻击方点击结束回合时调用。在服务端被调用，在服务端执行。
    /// </summary>
    public void DeployFinishRound()
    {
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

    /// <summary>
    /// 在双方客户端显示或隐藏打断战斗的消息框。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="state">显示或隐藏打断战斗的消息框</param>
    /// <param name="text">打断战斗的消息框中的文字</param>
    [TargetRpc]
    void TargetInterruptMessage(NetworkConnection target, bool state, string text)
    {
        _battleUI.transform.Find("InterruptedMessagePanel").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = text;
        _battleUI.transform.Find("InterruptedMessagePanel").gameObject.SetActive(state);
    }

    /// <summary>
    /// 在服务端添加战斗日志。
    /// </summary>
    /// <param name="message">一条战斗日志的文字</param>
    public void DeployAddLog(string message)
    {
        LogManager.Instance.TargetAddLog(_attacker.GetComponent<NetworkIdentity>().connectionToClient, message);
        LogManager.Instance.TargetAddLog(_defender.GetComponent<NetworkIdentity>().connectionToClient, message);
    }

    /// <summary>
    /// 在服务端改变玩家消耗的AP。
    /// </summary>
    /// <param name="costAP">新消耗的AP</param>
    public void DeployConsumeAP(float costAP)
    {
        _roundAPRemaining -= costAP;
        TargetAPChange(_attacker.GetComponent<NetworkIdentity>().connectionToClient, _roundAPLimit - _roundAPRemaining);
    }

    /// <summary>
    /// 在客户端改变玩家回合中消耗AP的显示。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="consumedAP">回合已消耗的AP</param>
    [TargetRpc]
    void TargetAPChange(NetworkConnection target, float consumedAP)
    {
        GameObject ConsumeAPText = _battleUI.transform.Find("FinishRoundButton").GetChild(1).gameObject;
        ConsumeAPText.GetComponent<TMPro.TextMeshProUGUI>().text = $"({consumedAP}AP/{_roundAPLimit}AP)";
    }

    /// <summary>
    /// 在服务端更新玩家护甲的显示。
    /// </summary>
    public void DeployRefreshArmorDisplay()
    {
        TargetRefreshArmorBattleDisplay(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker, _defender.GetComponent<NetworkIdentity>());
        TargetRefreshArmorBattleDisplay(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender, _attacker.GetComponent<NetworkIdentity>());
    }

    /// <summary>
    /// 在双方客户端更新玩家护甲的显示。
    /// </summary>
    /// <param name="target"></param>
    /// <param name="playerState">玩家战斗状态</param>
    [TargetRpc]
    void TargetRefreshArmorBattleDisplay(NetworkConnection target, PlayerState playerState, NetworkIdentity enemyPlayerID)
    {
        if(playerState == PlayerState.Attacker)
        {
            BackpackManager.Instance.RefreshArmorBattleDisplay(enemyPlayerID.gameObject);
        }
        else
        {
            BackpackManager.Instance.RefreshArmorBattleDisplay(enemyPlayerID.gameObject);
        }
    }
}
// TODO: 玩家断联 打断过程战斗结束有时候会有奇怪bug