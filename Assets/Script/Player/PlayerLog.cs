using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// 在服务端存储玩家受伤简要日志和淘汰数量，用于玩家死亡展示
/// </summary>
public class PlayerLog : NetworkBehaviour
{
    public List<LogInfo> LogList => _logList;
    List<LogInfo> _logList = new List<LogInfo>();
    public int EliminationCount => _eliminationCount;
    int _eliminationCount;
    public bool CheckFlag;
    
    void Start()
    {
        CheckFlag = true;
        _eliminationCount = 0;
    }
    [Command]
    public void CmdAddLog(GameObject player ,string message, LogInfo.DamageType damageType)
    {
        player.GetComponent<PlayerLog>().DeployAddLog(message, damageType);
    }
    public void DeployAddLog(string message, LogInfo.DamageType damageType)
    {
        _logList.Add(new LogInfo(message, LogInfo.DamageType.fight));
    }
    
    public void DeployAddEliminationCount()
    {
        _eliminationCount++;
        CheckFlag = true;
    }
}

public class LogInfo
{
    public enum DamageType
    {
        fight,
        poison,
        // trap,
        // fall,
        // drown,
        // hunger,
        // thirst,
        // cold,
        // heat,
        // infection,
        // radiation,
        // explosion,
        other
    }
    public string Message;
    // string _time;
    // string _location;
    // string _damageSource;
    // string _weapon;
    // string _damage;
    public DamageType Type;
    public LogInfo(){}
    public LogInfo(string message, DamageType damageType)
    {
        Message = message;
        Type = damageType;
    }
}
