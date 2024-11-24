using UnityEngine;

public class FinishRound : MonoBehaviour
{
    public void OnButtonClick()
    {
        Debug.Log("FinishRound");
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdFinishRound();
    }
}