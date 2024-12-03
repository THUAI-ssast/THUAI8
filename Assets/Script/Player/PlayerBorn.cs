using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class PlayerBorn : NetworkBehaviour
{
    [Command]
    public void CmdHandleCellClick(int oldIndex, int newIndex, NetworkConnectionToClient conn = null)
    {
        ChangeGridCellColor(oldIndex, newIndex);
        TargetChangeCurrentRedCell(conn, newIndex);
    }

    [ClientRpc]
    public void ChangeGridCellColor(int oldIndex, int newIndex) // ����Ҫ�ѱ��˵ľɵ�currentredcell����ʾ��ͬ���޸ĵ������ǲ��޸���currentredcell
    {
        if (oldIndex >= 0)
        {
            GameObject oldCell = BornUIManager.Instance.GridCells[oldIndex];
            GridCell oldCellComponent = oldCell.GetComponent<GridCell>();
            oldCellComponent.PlayerAmount -= 1;
            UpdateCellDisplay(oldCell, oldCellComponent.PlayerAmount);
        }

        //if (BornUIManager.Instance.CurrentRedCell != null)
        //{
        //    GameObject oldCell = BornUIManager.Instance.CurrentRedCell;
        //    GridCell oldCellComponent = oldCell.GetComponent<GridCell>();

        //    oldCellComponent.PlayerAmount -= 1;

        //    UpdateCellDisplay(oldCell, oldCellComponent.PlayerAmount);
        //}

        GameObject clickedCell = BornUIManager.Instance.GridCells[newIndex];
        GridCell gridCellComponent = clickedCell.GetComponent<GridCell>();

        // ���� PlayerAmount
        gridCellComponent.PlayerAmount += 1;

        // ���µ�ǰ�ͻ��˵���ʾ״̬
        UpdateCellDisplay(clickedCell, gridCellComponent.PlayerAmount);
    }

    private void UpdateCellDisplay(GameObject cell, int amount)
    {
        Image cellImage = cell.GetComponent<Image>();
        TextMeshProUGUI textComponent = cell.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (amount > 0)
        {
            // ��ʾ��ɫ�������ı�
            cellImage.color = Color.red;
            textComponent.gameObject.SetActive(true);
            textComponent.text = amount.ToString();
        }
        else
        {
            // �ָ���ɫ�����������ı�
            Color currentColor = Color.white;
            currentColor.a = 100f / 255f;
            cellImage.color = currentColor;
            textComponent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ���ûص����������޸ľɵ�CurrentRedCell֮ǰ�����Ѿ���CurrentRedCell����
    /// </summary>
    [TargetRpc]
    public void TargetChangeCurrentRedCell(NetworkConnection target, int index)
    {
        BornUIManager.Instance.CurrentRedCell = BornUIManager.Instance.GridCells[index];
    }
}
