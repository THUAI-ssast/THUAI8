using UnityEngine;
using UnityEngine.UI;

public class BattleLogManager : MonoBehaviour
{
    public GameObject logTextPrefab; // ���� Text ��Ԥ�Ƽ�
    public Transform contentTransform; // Content �� Transform

    private float timer = 0f; // ���ڿ���ÿ�����
    private int logCounter = 0; // ������������������־����

    void Update()
    {
        // ÿ�����һ����־
        timer += Time.deltaTime;
        if (timer >= 1f) // ÿ��ִ��һ��
        {
            timer = 0f; // ���ü�ʱ��
            AddLog($"Log message {logCounter++}"); // �������־
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

        // �Զ�������������־
        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        // ����� Scroll Rect�������ֶ���������������ײ�
        ScrollRect scrollRect = contentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases(); // ȷ�����ָ������
            scrollRect.verticalNormalizedPosition = 0f; // �������ײ�
        }
    }
}
