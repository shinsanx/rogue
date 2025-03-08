using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorTableSO", menuName = "Dungeon/FloorTableSO")]
public class FloorTableSO : ScriptableObject
{
    public int InitialEnemyCount;
    public int InitialItemCount;
    public EnemyTableSO EnemyTable;
    public ItemTableSO ItemTable;    
}
