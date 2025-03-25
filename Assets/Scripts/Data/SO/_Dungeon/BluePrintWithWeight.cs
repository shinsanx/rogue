using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RandomDungeonWithBluePrint;

[Serializable]
public class BluePrintWithWeight {
    public FieldBluePrint BluePrint = default;
    public int Weight = default;//BluePringを複数設定した場合の重さ

}
