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
    public void ChangeGridCellColor(int oldIndex, int newIndex) // 还需要把别人的旧的currentredcell的显示给同步修改掉，但是不修改其currentredcell
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

        // 更新 PlayerAmount
        gridCellComponent.PlayerAmount += 1;

        // 更新当前客户端的显示状态
        UpdateCellDisplay(clickedCell, gridCellComponent.PlayerAmount);
    }

    private void UpdateCellDisplay(GameObject cell, int amount)
    {
        Image cellImage = cell.GetComponent<Image>();
        TextMeshProUGUI textComponent = cell.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (amount > 0)
        {
            // 显示红色背景和文本
            cellImage.color = Color.red;
            textComponent.gameObject.SetActive(true);
            textComponent.text = amount.ToString();
        }
        else
        {
            // 恢复白色背景并隐藏文本
            Color currentColor = Color.white;
            currentColor.a = 100f / 255f;
            cellImage.color = currentColor;
            textComponent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 采用回调，避免在修改旧的CurrentRedCell之前，就已经把CurrentRedCell更新
    /// </summary>
    [TargetRpc]
    public void TargetChangeCurrentRedCell(NetworkConnection target, int index)
    {
        BornUIManager.Instance.CurrentRedCell = BornUIManager.Instance.GridCells[index];
    }
}
