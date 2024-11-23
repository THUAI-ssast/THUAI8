using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 世界回合管理类。
/// </summary>
public class RoundManager : NetworkBehaviour
{
    /// <summary>
    /// 回合状态，未准备，准备中（准备确认状态），已准备。
    /// </summary>
    public enum RoundState
    {
        NotReady,
        PreReady,
        Ready
    }
    /// <summary>
    /// 单例模式
    /// </summary>
    public static RoundManager Instance;
    /// <summary>
    /// 回合状态
    /// </summary>
    public RoundState State = RoundState.NotReady;
    /// <summary>
    /// 定位回合UI
    /// </summary>
    [SerializeField] private GameObject _round;

    [SerializeField] private float _apIncrease;
    /// <summary>
    /// 回合持续时间
    /// </summary>
    [SerializeField]private float _roundDuration = 20f;
    /// <summary>
    /// 回合计时器
    /// </summary>
    private float _timer;
    /// <summary>
    /// 已准备玩家列表
    /// </summary>
    private List<uint> _readyPlayer = new List<uint>();
    /// <summary>
    /// 回合计时器文本
    /// </summary>
    [SyncVar] private string _timeText;
    /// <summary>
    /// 已准备玩家数量
    /// </summary>
    [SyncVar(hook = nameof(UpdateReadyButtonUIReady))]  private int _readyPlayerCount;
    /// <summary>
    /// 局内玩家数量
    /// </summary>
    [SyncVar(hook = nameof(UpdateReadyButtonUIAll))] private int _playerCount;

    /// <summary>
    /// 回合计数
    /// </summary>
    [SyncVar(hook = nameof(RoundCountChange))] private int _roundCount = 0;

    private void Awake()
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
    /// 初始化回合，仅在服务器上执行，开始世界回合循环。
    /// </summary>
    void Start()
    {
        InitRoundSetting();
        if(isServer)
        {
            _timer = _roundDuration;
            RoundCountIncreaseOnServer();
            StartCoroutine(RoundTimer());
        }
    }
    /// <summary>
    /// 回合初始化设置。
    /// </summary>
    void InitRoundSetting()
    {
        State = RoundState.NotReady;
        _round.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color32(203, 255, 252, 255);// blue
        _round.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().fontSize = 18;
        _round.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "结束\n回合";
        _round.transform.GetChild(3).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = null;
    }
    /// <summary>
    /// 玩家准备状态变化。从[Command]中调用，仅在服务器上执行。
    /// </summary>
    /// <param name="playerID">玩家id</param>
    /// <param name="isReady">玩家准备状态</param>
    public void Ready(uint playerID, bool isReady)
    {
        if(isReady == true) _readyPlayer.Add(playerID);
        else _readyPlayer.Remove(playerID);
        _readyPlayerCount = _readyPlayer.Count;
    }
    /// <summary>
    /// 世界回合循环流程。准备玩家数等于连接玩家数量或计时器结束时，结束回合。
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundTimer()
    {
        StartRound();
        yield return new WaitForSeconds(2);
        while (true)
        {
            _playerCount = NetworkServer.connections.Count;
            if (_readyPlayer.Count == _playerCount)
            {
                EndRoundOnServer();
                yield return new WaitForSeconds(2);
                StartRound();
                yield return new WaitForSeconds(2);
            }
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                UpdateTimerText();
            }
            else
            {
                EndRoundOnServer();
                yield return new WaitForSeconds(2);
                StartRound();
                yield return new WaitForSeconds(2);
            }
            yield return null;
        }
    }
    /// <summary>
    /// 结束回合时服务器操作，清空准备玩家列表、重置计时器。
    /// </summary>
    void EndRoundOnServer()
    {
        EndRoundOnClient();
        _timer = _roundDuration;
        RoundCountIncreaseOnServer();
        _readyPlayer.Clear();
    }
    /// <summary>
    /// 服务器更新回合计时器文本，每帧调用。
    /// </summary>
    void UpdateTimerText()
    {
        int second = Mathf.CeilToInt(_timer) % 60;
        int minute = Mathf.CeilToInt(_timer) / 60;
        string minuteText = minute < 10 ? "0" + minute.ToString() : minute.ToString();
        string secondText = second< 10 ? "0" + second.ToString() : second.ToString();
        string countText = $"第{_roundCount}回合 ";
        _timeText = countText + minuteText + ":" + secondText;
        UpdateTimeUI();   
    }
    /// <summary>
    /// 客户端更新回合计时器文本UI显示。
    /// </summary>
    [ClientRpc]
    void UpdateTimeUI()
    {
        _round.transform.GetChild(3).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = _timeText;
    }
    /// <summary>
    /// 客户端开始回合UI显示。
    /// </summary>
    [ClientRpc]
    void StartRound()
    {
        StartCoroutine(StartRoundUI());
    }
    /// <summary>
    /// 客户端开始回合UI显示。
    /// </summary>
    /// <returns></returns>
    IEnumerator StartRoundUI()
    {
        _round.transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        _round.transform.GetChild(1).gameObject.SetActive(false);
    }
    /// <summary>
    /// 客户端结束回合UI显示。
    /// </summary>
    [ClientRpc]
    void EndRoundOnClient()
    {
        StartCoroutine(EndRoundUI());
        InitRoundSetting();
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        player.GetComponent<PlayerActionPoint>().IncreaseActionPoint(_apIncrease);
    }
    /// <summary>
    /// 客户端结束回合UI显示。
    /// </summary>
    /// <returns></returns>
    IEnumerator EndRoundUI()
    {
        _round.transform.GetChild(2).gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        _round.transform.GetChild(2).gameObject.SetActive(false);
    }
    /// <summary>
    /// 准备玩家数量的hook函数，进行准备按钮的UI更新。
    /// </summary>
    /// <param name="oldReadyPlayer">要求格式</param>
    /// <param name="newReadyPlayer">要求格式</param>
    private void UpdateReadyButtonUIReady(int oldReadyPlayer, int newReadyPlayer)
    {
        GameObject readyButton = _round.transform.GetChild(0).gameObject;
        if(State == RoundState.Ready)
        {
            readyButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"等待\n其他玩家\n({newReadyPlayer}/{_playerCount})";
        }
    }
    /// <summary>
    /// 局内玩家总数的hook函数，进行准备按钮的UI更新。
    /// </summary>
    /// <param name="oldPlayerCount">要求格式</param>
    /// <param name="newPlayerCount">要求格式</param>
    private void UpdateReadyButtonUIAll(int oldPlayerCount, int newPlayerCount)
    {
        GameObject readyButton = _round.transform.GetChild(0).gameObject;
        if(State == RoundState.Ready)
        {
            readyButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"等待\n其他玩家\n({_readyPlayerCount}/{newPlayerCount})";
        }
    }

    private void RoundCountIncreaseOnServer()
    {
        _roundCount += 1;
        SafeAreaManager.Instance.UpdateSafeAreaOnServer(_roundCount);
    }

    private void RoundCountChange(int oldRoundCount, int newRoundCount)
    {
        if (newRoundCount > 1)
        {
            SafeAreaManager.Instance.DoSafeAreaDamageByRoundEnd(oldRoundCount);
        }
    }
}
