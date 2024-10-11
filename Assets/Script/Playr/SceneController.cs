using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject playerPrefab; // 拖拽你的玩家预制体到此处
    public Vector3 areaMin;         // 场景中随机生成位置的最小坐标
    public Vector3 areaMax;         // 场景中随机生成位置的最大坐标
    private List<GameObject> characters = new List<GameObject>();

    private void Start()
    {
        // 保持现有的玩家角色位置不变，其他7个角色随机生成
        for (int i = 0; i < 8; i++)
        {
            if (i == 0)
            {
                // 第一个角色保留当前已有的角色，不生成
                characters.Add(GameObject.FindGameObjectWithTag("Player")); // 假设现有的角色有"Player"标签
            }
            else
            {
                // 随机生成其他7个角色
                Vector3 randomPosition = GetRandomPosition();
                GameObject newCharacter = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
                characters.Add(newCharacter);
            }
        }
    }

    /// <summary>
    /// 获取随机位置
    /// </summary>
    /// <returns>返回随机生成的位置</returns>
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(areaMin.x, areaMax.x);
        float y = Random.Range(areaMin.y, areaMax.y);
        float z = Random.Range(areaMin.z, areaMax.z);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 获取所有角色的位置
    /// </summary>
    public void GetAllCharacterPositions()
    {
        foreach (var character in characters)
        {
            Debug.Log(character.transform.position);
        }
    }
}
