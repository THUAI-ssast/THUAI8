using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家死亡界面，点击按钮断开房间连接，返回开始界面
/// </summary>
public class PlayerDead : MonoBehaviour
{
    public void OnButtonClick()
    {
        RoomManager.Instance.StopClient();
        SceneManager.LoadScene("StartScene");
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}