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
    public void ChangeGridCellColor(int oldIndex, int newIndex)
    {
        if (oldIndex >= 0 && BornUIManager.Instance.GridCells[oldIndex] != null)
        {
            GameObject oldCell = BornUIManager.Instance.GridCells[oldIndex];
            GridCell oldCellComponent = oldCell.GetComponent<GridCell>();
            oldCellComponent.PlayerAmount -= 1;
            UpdateCellDisplay(oldCell, oldCellComponent.PlayerAmount);
        }

        if (BornUIManager.Instance.GridCells[newIndex] != null)
        {
            GameObject clickedCell = BornUIManager.Instance.GridCells[newIndex];
            GridCell gridCellComponent = clickedCell.GetComponent<GridCell>();

            // 更新 PlayerAmount
            gridCellComponent.PlayerAmount += 1;

            // 更新当前客户端的显示状态
            UpdateCellDisplay(clickedCell, gridCellComponent.PlayerAmount);
        }
    }

    private void UpdateCellDisplay(GameObject cell, int amount)
    {
        Image cellImage = cell.GetComponent<Image>();
        TextMeshProUGUI textComponent = cell.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (amount > 0)
        {
            // 显示红色背景和文本
            if (cell != BornUIManager.Instance.CurrentSelectedCell)
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
        if (BornUIManager.Instance.CurrentSelectedCell != null)
        {
            GameObject currentCell = BornUIManager.Instance.CurrentSelectedCell;
            Image oldCellImage = currentCell.GetComponent<Image>();
            TextMeshProUGUI oldTextComponent = currentCell.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (currentCell.GetComponent<GridCell>().PlayerAmount > 0)
                oldCellImage.color = Color.red;
            else
            {
                Color currentColor = Color.white;
                currentColor.a = 100f / 255f;
                oldCellImage.color = currentColor;
                oldTextComponent.gameObject.SetActive(false);
            }
        }

        if (index >= 0 && BornUIManager.Instance.GridCells[index] != null)
        {
            BornUIManager.Instance.CurrentSelectedCell = BornUIManager.Instance.GridCells[index];
            GameObject clickedCell = BornUIManager.Instance.CurrentSelectedCell;
            Image cellImage = clickedCell.GetComponent<Image>();
            cellImage.color = Color.blue;
        }
        else
        {
            Debug.LogWarning($"TargetChangeCurrentRedCell: GridCells[{index}] is null, skipping update.");
        }
    }
}
