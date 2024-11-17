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
        // ʵ�����µ���־��Ŀ
        GameObject newLog = Instantiate(logTextPrefab, contentTransform);

        // ������־����
        Text logText = newLog.GetComponent<Text>();
        if (logText != null)
        {
            logText.text = message;
        }

        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        // ����� Scroll Rect�������ֶ���������������ײ�
        ScrollRect scrollRect = contentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
