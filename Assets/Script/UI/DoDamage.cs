using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoDamage : MonoBehaviour
{
    private GameObject _localPlayer;
    private GameObject _enemyPlayer;

    private SlotMenuTrigger _slotMenuTrigger;

    // Start is called before the first frame update
    void Start()
    {
        _localPlayer = GameObject.FindWithTag("LocalPlayer");
        _enemyPlayer = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickHead()
    {
        if (UIManager.Instance.FollowImage != null)
        {
            _localPlayer.GetComponent<PlayerHealth>().CmdAttack(_localPlayer, _enemyPlayer, (int)PlayerHealth.BodyPosition.Head, UIManager.Instance.FollowImage.gameObject);
            BackpackManager.Instance.BattleHeadHealthEnemyPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_enemyPlayer.GetComponent<PlayerHealth>().HeadHealth}/{_enemyPlayer.GetComponent<PlayerHealth>().HeadMaxHealth}";

            UIManager.Instance.DestroyCurrentFollowImage();

            //if (SlotMenuTrigger.Instance.FollowImage != null)
            //{
            //    Destroy(SlotMenuTrigger.Instance.FollowImage.gameObject);
            //    SlotMenuTrigger.Instance.FollowImage = null;
            //}
        }
    }

    public void OnClickBody()
    {
        if (UIManager.Instance.FollowImage != null)
        {
            _localPlayer.GetComponent<PlayerHealth>().CmdAttack(_localPlayer, _enemyPlayer, (int)PlayerHealth.BodyPosition.MainBody, UIManager.Instance.FollowImage.gameObject);
            BackpackManager.Instance.BattleHeadHealthEnemyPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_enemyPlayer.GetComponent<PlayerHealth>().BodyHealth}/{_enemyPlayer.GetComponent<PlayerHealth>().BodyMaxHealth}";

            UIManager.Instance.DestroyCurrentFollowImage();

            //if (SlotMenuTrigger.Instance.FollowImage != null)
            //{
            //    Destroy(SlotMenuTrigger.Instance.FollowImage.gameObject);
            //    SlotMenuTrigger.Instance.FollowImage = null;
            //}
        }
    }

    public void OnClickLegs()
    {
        if (UIManager.Instance.FollowImage != null)
        {
            _localPlayer.GetComponent<PlayerHealth>().CmdAttack(_localPlayer, _enemyPlayer, (int)PlayerHealth.BodyPosition.Legs, UIManager.Instance.FollowImage.gameObject);
            BackpackManager.Instance.BattleHeadHealthEnemyPanel.GetChild(0).GetComponent<TMP_Text>().text =
                $"{_enemyPlayer.GetComponent<PlayerHealth>().LegHealth}/{_enemyPlayer.GetComponent<PlayerHealth>().LegMaxHealth}";

            UIManager.Instance.DestroyCurrentFollowImage();

            //if (SlotMenuTrigger.Instance.FollowImage != null)
            //{
            //    Destroy(SlotMenuTrigger.Instance.FollowImage.gameObject);
            //    SlotMenuTrigger.Instance.FollowImage = null;
            //}
        }
    }
}
