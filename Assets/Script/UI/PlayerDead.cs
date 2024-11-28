using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDead : MonoBehaviour
{
    public void OnButtonClick()
    {
        RoomManager.Instance.StopClient();
        SceneManager.LoadScene("StartScene");
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}