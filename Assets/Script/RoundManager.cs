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
    /// <summary>
    /// 回合持续时间
    /// </summary>
    private float _roundDuration = 20f;
    /// <summary>
    /// 回合计时器
    /// </summary>
    private float _timer;
    /// <summary>
    /// 回合计时器文本
    /// </summary>
    [SyncVar] private string _timeText;
    /// <summary>
    /// 已准备玩家列表
    /// </summary>
    [SyncVar] private List<uint> _readyPlayer = new List<uint>();
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
            StartCoroutine(RoundTimer());
        }
    }
    void InitRoundSetting()
    {
        State = RoundState.NotReady;
        _round.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color32(203, 255, 252, 255);// blue
        _round.transform.GetChild(0).GetComponent<VerticalLayoutGroup>().padding.top = 0;
        _round.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        _round.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = null;
        _round.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().fontSize = 20;
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
        Debug.Log("ReadyPlayer: " + _readyPlayer.Count);
    }
    /// <summary>
    /// 世界回合循环流程。准备玩家数等于连接玩家数量或计时器结束时，结束回合。
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundTimer()
    {
        Debug.Log("EnterGame");
        StartRound();
        yield return new WaitForSeconds(2);
        while (true)
        {
            if (_readyPlayer.Count == NetworkServer.connections.Count)
            {
                Debug.Log("Ready to end");
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
        _timeText = minuteText + ":" + secondText;
        UpdateTimeUI();   
    }
    /// <summary>
    /// 客户端更新回合计时器文本UI显示。
    /// </summary>
    [ClientRpc]
    void UpdateTimeUI()
    {
        _round.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = _timeText;
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
        Debug.Log("回合开始");
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
        player.GetComponent<PlayerActionPoint>().IncreaseActionPoint(2);
    }
    /// <summary>
    /// 客户端结束回合UI显示。
    /// </summary>
    /// <returns></returns>
    IEnumerator EndRoundUI()
    {
        _round.transform.GetChild(2).gameObject.SetActive(true);
        Debug.Log("回合结束");
        yield return new WaitForSeconds(1);
        _round.transform.GetChild(2).gameObject.SetActive(false);
    }
}
