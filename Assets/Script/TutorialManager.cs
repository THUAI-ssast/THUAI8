using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Item _medicine;
    private void Start()
    {
        _medicine.Initialize(_medicine.ItemData, ItemOwner.World, 0);
        StartCoroutine(changePlayerHP());
    }

    private IEnumerator changePlayerHP()
    {
        GameObject player;
        while ((player = GameObject.FindWithTag("LocalPlayer")) == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.MainBody, -5.5f);
        playerHealth.ChangeHealth((int)PlayerHealth.BodyPosition.Legs, -7.4f);
    }
}
