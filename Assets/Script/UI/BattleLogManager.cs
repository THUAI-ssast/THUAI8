using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleLogManager : MonoBehaviour
{
    public static BattleLogManager Instance;

    public GameObject logTextPrefab;
    public Transform contentTransform;

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

    public void AddLog(string message)
    {
        // 实例化新的日志条目
        GameObject newLog = Instantiate(logTextPrefab, contentTransform);

        // 设置日志内容
        TMP_Text logText = newLog.GetComponent<TMP_Text>();
        if (logText != null)
        {
            logText.text = message;
            Debug.Log(message);
        }

        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        // 如果有 Scroll Rect，可以手动将滚动条拉到最底部
        ScrollRect scrollRect = contentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
