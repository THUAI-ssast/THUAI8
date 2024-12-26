using UnityEngine;
/// <summary>
/// UI行为类，玩家战斗UI内取消打断按钮
/// </summary>
public class CancelInterrupt : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdCancelInterrupt();
    }
}