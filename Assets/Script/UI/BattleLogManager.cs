using UnityEngine;
using UnityEngine.UI;

public class BattleLogManager : MonoBehaviour
{
    public GameObject logTextPrefab; // 引用 Text 的预制件
    public Transform contentTransform; // Content 的 Transform

    private float timer = 0f; // 用于控制每秒添加
    private int logCounter = 0; // 计数器，用于生成日志内容

    void Update()
    {
        // 每秒添加一个日志
        timer += Time.deltaTime;
        if (timer >= 1f) // 每秒执行一次
        {
            timer = 0f; // 重置计时器
            AddLog($"Log message {logCounter++}"); // 添加新日志
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

        // 自动滚动到最新日志
        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        // 如果有 Scroll Rect，可以手动将滚动条拉到最底部
        ScrollRect scrollRect = contentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases(); // 确保布局更新完成
            scrollRect.verticalNormalizedPosition = 0f; // 滚动到底部
        }
    }
}
