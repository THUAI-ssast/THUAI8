using UnityEngine;

public class FinishRound : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdFinishRound();
    }
}