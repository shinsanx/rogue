using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomDungeonWithBluePrint;
using System;

[CreateAssetMenu(fileName = "DungeonFieldTable", menuName = "Dungeon/DungeonFieldTable")]
public class DungeonFieldTable : ScriptableObject
{
    public BluePrintWithWeight[] BluePrintWithWeights;
}
