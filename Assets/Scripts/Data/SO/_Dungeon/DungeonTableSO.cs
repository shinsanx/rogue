using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonTableSO", menuName = "Dungeon/DungeonTableSO")]
public class DungeonTableSO : ScriptableObject
{
    public List<FloorTableSO> Floors;    
}
