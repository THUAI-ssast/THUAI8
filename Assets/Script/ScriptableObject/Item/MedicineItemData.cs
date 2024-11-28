using SerializableDictionary.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ItemSystem/Item/MedicineItemData")]
public class MedicineItemData : ItemData
{
    public int Durability;

    public SerializableDictionary<PlayerHealth.BodyPosition, float> BodyHealDictionary =
        new SerializableDictionary<PlayerHealth.BodyPosition, float>();
}
