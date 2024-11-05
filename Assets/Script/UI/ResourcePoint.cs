using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourcePoint : NetworkBehaviour
{
    public GameObject resourceUIPanel;

    // Start is called before the first frame update
    void Start()
    {
        resourceUIPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            UIManager.Instance.allowTabOperation = true;
            UIManager.Instance.currentResourceUIPanel = resourceUIPanel;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            UIManager.Instance.allowTabOperation = false;
            UIManager.Instance.currentResourceUIPanel = null;
        }
    }
}
