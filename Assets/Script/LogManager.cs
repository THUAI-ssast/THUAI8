using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LogManager : NetworkBehaviour
{
    public static LogManager Instance;

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
    
    [TargetRpc]
    public void TargetAddLog(NetworkConnection conn, string message)
    {
        GameObject newLog = Instantiate(logTextPrefab, _logPanelContent.transform);
        newLog.GetComponent<TMPro.TextMeshProUGUI>().text = message;
        ScrollToBottom();  
    }

    void ScrollToBottom()
    {
        ScrollRect scrollRect = _logPanelContent.transform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    [TargetRpc]
    public void TargetDestroyAllLogDisplay(NetworkConnection conn)
    {
        foreach (Transform child in _logPanelContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
