using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FightManager : NetworkBehaviour
{
    public static FightManager Instance;
    private GameObject _attacker;
    private GameObject _defender;
    private float _roundDuration;
    GameObject _runPlayer = null;
    GameObject _deadPlayer = null;
    GameObject _battleUI;
    GameObject _finishRoundButton;
    GameObject _escapeButton;
    [SyncVar] float _timer;
    [SyncVar] int _roundCount;
    enum PlayerState
    {
        Attacker,
        Defender
    }
    void Awake()
    {
        if(Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        _roundCount = 1;
        _roundDuration = 20f;
        _battleUI = GameObject.Find("Canvas").transform.GetChild(4).gameObject;
        _finishRoundButton = _battleUI.transform.GetChild(8).gameObject;
        _escapeButton = _battleUI.transform.GetChild(7).gameObject;
        _finishRoundButton.GetComponent<Button>().interactable = false;
        _escapeButton.GetComponent<Button>().interactable = false;
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
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                UpdateTimerText();
            }
            else
            {
                EndRoundOnServer();
                yield return new WaitForSeconds(2);
                StartRoundOnServer();
                yield return new WaitForSeconds(2);
            }
            yield return null;
        }
        FightOver();
    }
    void FightOver()
    {
        // 战斗结束
        // 战斗日志输出
        // 双方战斗界面关闭
        TargetFightOver(_attacker.GetComponent<NetworkIdentity>().connectionToClient);
        TargetFightOver(_defender.GetComponent<NetworkIdentity>().connectionToClient);
    }
    [TargetRpc]
    void TargetFightOver(NetworkConnection target)
    {
        _battleUI.SetActive(false);
    }
    void StartRoundOnServer()
    {
        _timer = _roundDuration;
        // 开始当前进攻方的回合
        // 进攻方开始回合
        TargetStartRound(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        TargetStartRound(_defender.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Defender);
        _roundCount++;
        // 战斗日志输出
    }
    void EndRoundOnServer()
    {
        // 结束当前进攻方的回合
        // 进攻方结束回合
        TargetEndRound(_attacker.GetComponent<NetworkIdentity>().connectionToClient, PlayerState.Attacker);
        
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
        // 进攻方按钮变灰 不可点
        _finishRoundButton.GetComponent<Button>().interactable = false;
        _escapeButton.GetComponent<Button>().interactable = false;
        // 进攻方不可拖动
    }
    [TargetRpc]
    void TargetStartRound(NetworkConnection target, PlayerState playerState)
    {
        Debug.Log(playerState + "StartRound");
        GameObject currentRound = _battleUI.transform.Find("CurrentRoundPanel").GetChild(0).gameObject;
        GameObject timer = _battleUI.transform.Find("CurrentRoundPanel").GetChild(1).gameObject;
        timer.GetComponent<TMPro.TextMeshProUGUI>().text = $"{_roundDuration}s/{_roundDuration}s";
        if(playerState == PlayerState.Attacker)
        {
            
            currentRound.GetComponent<TMPro.TextMeshProUGUI>().text = $"第{_roundCount}回合\n你的回合";
            // 进攻方按钮变亮 可点
            _finishRoundButton.GetComponent<Button>().interactable = true;
            _escapeButton.GetComponent<Button>().interactable = true;
            // 进攻方可拖动
        }
        if(playerState == PlayerState.Defender)
        {
            currentRound.GetComponent<TMPro.TextMeshProUGUI>().text = $"第{_roundCount}回合\n对方回合";
        }
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
        if(_deadPlayer != null)
        {
            TargetDefenderPlayerDead(_defender.GetComponent<NetworkIdentity>().connectionToClient);
            TargetAttackerPlayerDead(_attacker.GetComponent<NetworkIdentity>().connectionToClient);
            return true;
        }
        if(_runPlayer != null)
        {
            TargetDefenderPlayerRun(_defender.GetComponent<NetworkIdentity>().connectionToClient);
            TargetAttackerPlayerRun(_attacker.GetComponent<NetworkIdentity>().connectionToClient);
            return true;
        }
        return false;
    }
    [TargetRpc]
    void TargetDefenderPlayerDead(NetworkConnection target)
    {
        // 防守方ui显示
    }
    [TargetRpc]
    void TargetAttackerPlayerDead(NetworkConnection target)
    {
        // 进攻方ui显示
    }
    [TargetRpc]
    void TargetDefenderPlayerRun(NetworkConnection target)
    {
        // 防守方ui显示
    }
    [TargetRpc]
    void TargetAttackerPlayerRun(NetworkConnection target)
    {
        // 进攻方ui显示
    }
}