using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_SO", menuName ="Item/ItemSO", order =0)]
[System.Serializable]
public abstract class ItemSO : ScriptableObject
{
    public int id;
    public string itemName;
    [TextArea] public string description;
    public int purchasePrice;
    public int sellingPrice;
    public Sprite icon;
}