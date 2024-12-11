using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// UI类，用于显示和更新玩家在BattleScene中的血条
/// </summary>
public class HealthGUI : MonoBehaviour
{
    /// <summary>
    /// 对应的玩家Transform
    /// </summary>
    public Transform TargetPlayer;

    /// <summary>
    /// 相对于玩家位置的偏移量，用于调整玩家血条的显示位置
    /// </summary>
    public Vector3 Offset;


    // Start is called before the first frame update
    void Start()
    {
        transform.position = TargetPlayer.transform.position + Offset;
    }


    /// <summary>
    /// 更新玩家血条的长度，当玩家血量更改时会自动调用
    /// </summary>
    /// <param name="totalHealth">血量上限</param>
    /// <param name="newHealth">当前血量</param>
    public void UpdateHealthGUILength(float totalHealth, float newHealth)
    {
        gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount = newHealth/totalHealth;
    }
}
