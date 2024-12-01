using UnityEngine;

public class CancelInterrupt : MonoBehaviour
{
    public void OnButtonClick()
    {
        GameObject.FindWithTag("LocalPlayer").GetComponent<PlayerFight>().CmdCancelInterrupt();
    }
}