using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI行为类，玩家死亡界面返回大厅按钮，点击按钮断开房间连接，返回开始界面
/// </summary>
public class PlayerDeadUI : MonoBehaviour
{
    public void OnButtonClick()
    {
        NetworkManagerController.Instance.IsEnterRoom = false;
        RoomManager.Instance.StopClient();
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}