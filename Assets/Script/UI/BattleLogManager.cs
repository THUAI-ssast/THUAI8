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
        // ʵ�����µ���־��Ŀ
        GameObject newLog = Instantiate(logTextPrefab, contentTransform);

        // ������־����
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
        // ����� Scroll Rect�������ֶ���������������ײ�
        ScrollRect scrollRect = contentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
