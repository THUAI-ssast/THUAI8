using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new CraftWay", menuName = "ItemSystem/Craft way")]
public class CraftWayData : ScriptableObject
{
    public List<ItemData> CostItems;
    public List<ItemData> CatalystItems;
    public ItemData ProductItem;
}
