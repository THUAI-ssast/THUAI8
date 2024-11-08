using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourcePoint : NetworkBehaviour
{
    public GameObject resourceUIPanel;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        resourceUIPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        Debug.Log("----------------------------");
        player = GameObject.FindWithTag("LocalPlayer");
        if (player != null)
        {
            CheckPlayerProximity();
        }
    }

    private void CheckPlayerProximity()
    {
        Vector3 resourcePosition = transform.position;
        Vector3 playerPosition = player.transform.position;

        float epsilon = 0.1f;

        bool isAdjacentInX = Mathf.Abs(playerPosition.x - resourcePosition.x) <= 1.01f &&
                             Mathf.Abs(playerPosition.y - resourcePosition.y) <= epsilon;

        bool isAdjacentInY = Mathf.Abs(playerPosition.y - resourcePosition.y) <= 1.01f &&
                             Mathf.Abs(playerPosition.x - resourcePosition.x) <= epsilon;

        if (isAdjacentInX || isAdjacentInY)
        {
            UIManager.Instance.allowTabOperation = true;
            UIManager.Instance.currentResourceUIPanel = resourceUIPanel;
        }
        else
        {
            UIManager.Instance.allowTabOperation = false;
            UIManager.Instance.currentResourceUIPanel = null;
        }
    }
}
