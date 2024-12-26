using UnityEngine;

/// <summary>
/// UI行为类，玩家战斗UI内逃跑按钮
/// </summary>
public class Escape : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdEscape();
    }
}