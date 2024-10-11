using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject playerPrefab; // ��ק������Ԥ���嵽�˴�
    public Vector3 areaMin;         // �������������λ�õ���С����
    public Vector3 areaMax;         // �������������λ�õ��������
    private List<GameObject> characters = new List<GameObject>();

    private void Start()
    {
        // �������е���ҽ�ɫλ�ò��䣬����7����ɫ�������
        for (int i = 0; i < 8; i++)
        {
            if (i == 0)
            {
                // ��һ����ɫ������ǰ���еĽ�ɫ��������
                characters.Add(GameObject.FindGameObjectWithTag("Player")); // �������еĽ�ɫ��"Player"��ǩ
            }
            else
            {
                // �����������7����ɫ
                Vector3 randomPosition = GetRandomPosition();
                GameObject newCharacter = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
                characters.Add(newCharacter);
            }
        }
    }

    /// <summary>
    /// ��ȡ���λ��
    /// </summary>
    /// <returns>����������ɵ�λ��</returns>
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(areaMin.x, areaMax.x);
        float y = Random.Range(areaMin.y, areaMax.y);
        float z = Random.Range(areaMin.z, areaMax.z);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// ��ȡ���н�ɫ��λ��
    /// </summary>
    public void GetAllCharacterPositions()
    {
        foreach (var character in characters)
        {
            Debug.Log(character.transform.position);
        }
    }
}
