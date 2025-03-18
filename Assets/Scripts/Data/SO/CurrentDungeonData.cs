using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/CurrentDungeonData")]
public class CurrentDungeonData : ScriptableObject
{    
    public int currentFloor;
    public DungeonDataSO currentDungeonData;     
}
