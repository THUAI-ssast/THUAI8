using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

/// <summary>
/// 单例Manager，管理战斗日志显示
/// </summary>
public class BattleLogManager : NetworkBehaviour
{
    /// <summary>
    /// 类的单例
    /// </summary>
    public static BattleLogManager Instance;

    [SerializeField] GameObject logTextPrefab;
    [SerializeField] GameObject _logPanelContent;

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
    /// TargetRPC函数，增加对应客户端的战斗日志
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="message"></param>
    [TargetRpc]
    public void TargetAddLog(NetworkConnection conn, string message)
    {
        GameObject newLog = Instantiate(logTextPrefab, _logPanelContent.transform);
        newLog.GetComponent<TMPro.TextMeshProUGUI>().text = message;
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        ScrollRect scrollRect = _logPanelContent.transform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
    /// <summary>
    /// TargetRPC函数，清空对应客户端的战斗日志
    /// </summary>
    /// <param name="conn"></param>
    [TargetRpc]
    public void TargetDestroyAllLog(NetworkConnection conn)
    {
        foreach (Transform child in _logPanelContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
