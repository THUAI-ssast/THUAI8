using UnityEngine;
using UnityEngine.UI;

public class CombatLogManager : MonoBehaviour
{
    public GameObject logTextPrefab;
    public Transform contentTransform;

    private float timer = 0f;
    private int logCounter = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0f;
            AddLog($"Log message {logCounter++}");
        }
    }

    void AddLog(string message)
    {
        // 实例化新的日志条目
        GameObject newLog = Instantiate(logTextPrefab, contentTransform);

        // 设置日志内容
        Text logText = newLog.GetComponent<Text>();
        if (logText != null)
        {
            logText.text = message;
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
