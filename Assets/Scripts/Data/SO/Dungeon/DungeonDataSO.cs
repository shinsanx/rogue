using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonDataSO", menuName = "Dungeon/DungeonDataSO")]
public class DungeonDataSO : ScriptableObject
{
    public string DungeonName;
    public int FloorCount;
    public DungeonTableSO DungeonTable;
    
    
}
