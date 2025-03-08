using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTableSO", menuName = "Dungeon/ItemTableSO")]
public class ItemTableSO : ScriptableObject
{
    public List<ItemSO> Items;
}
