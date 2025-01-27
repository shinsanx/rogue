using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectData
{
    int Id {get;set;}
    string Name {get;set;}
    string Type{get;set;}
    Vector2Int Position{get;set;}
    int RoomNum{get;set;}

    // オブジェクト更新時のイベント
    event System.Action<IObjectData> OnObjectUpdated;
}
