using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : NetworkBehaviour
{
    public enum RoundState
    {
        NotReady,
        PreReady,
        Ready
    }
    public static RoundManager Instance;
    public bool IsReady = false;
    public RoundState State = RoundState.NotReady;
    [SerializeField] private GameObject _round;
    private float _roundDuration = 20f;
    private float _timer;
    [SyncVar] private string _timeText;
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
    void Start()
    {
        if(isServer)
        {
            _timer = _roundDuration;
            StartCoroutine(RoundTimer());
        }
    }

    public void Ready(uint playerID, bool isReady)
    {
        _readyPlayer.Add(playerID);
        IsReady = isReady;
    }
    IEnumerator RoundTimer()
    {
        Debug.Log("EnterGame");
        StartRound();
        yield return new WaitForSeconds(2);
        while (true)
        {
            // Debug.Log(_readyPlayer.Count);
            // Debug.Log(NetworkServer.connections.Count);
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
    void EndRoundOnServer()
    {
        EndRoundOnClient();
        _timer = _roundDuration;
        _readyPlayer.Clear();
    }
    void UpdateTimerText()
    {
        int second = Mathf.CeilToInt(_timer) % 60;
        int minute = Mathf.CeilToInt(_timer) / 60;
        string minuteText = minute < 10 ? "0" + minute.ToString() : minute.ToString();
        string secondText = second< 10 ? "0" + second.ToString() : second.ToString();
        _timeText = minuteText + ":" + secondText;
        UpdateTimeUI();   
    }
    [ClientRpc]
    void UpdateTimeUI()
    {
        if(State == RoundState.NotReady)
        {
            // Debug.Log(_timeText);
            _round.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = _timeText;
        }
    }
    [ClientRpc]
    void StartRound()
    {
        StartCoroutine(StartRoundUI());
    }
    IEnumerator StartRoundUI()
    {
        _round.transform.GetChild(1).gameObject.SetActive(true);
        Debug.Log("回合开始");
        yield return new WaitForSeconds(1);
        _round.transform.GetChild(1).gameObject.SetActive(false);
    }
    [ClientRpc]
    void EndRoundOnClient()
    {
        StartCoroutine(EndRoundUI());
        IsReady = false;
        State = RoundState.NotReady;
        _round.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color32(203, 255, 252, 255);// blue
        _round.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = null;
        _round.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().fontSize = 20;
        GameObject player = GameObject.FindWithTag("LocalPlayer");
        player.GetComponent<PlayerActionPoint>().IncreaseActionPoint(2);
    }
    IEnumerator EndRoundUI()
    {
        _round.transform.GetChild(2).gameObject.SetActive(true);
        Debug.Log("回合结束");
        yield return new WaitForSeconds(1);
        _round.transform.GetChild(2).gameObject.SetActive(false);
    }
}
