using UnityEngine;
/// <summary>
/// UI行为类，玩家战斗UI内结束按钮
/// </summary>
public class FinishRound : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdFinishRound();
    }
}