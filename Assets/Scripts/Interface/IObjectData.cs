using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectData
{
    IntVariable Id {get;set;}
    StringVariable Name {get;set;}
    StringVariable Type{get;set;}
    Vector2IntVariable Position{get;set;}
    IntVariable RoomNum{get;set;}

    // オブジェクト更新時のイベント
    event System.Action<IObjectData> OnObjectUpdated;

    void SetPosition(Vector2Int position);
}
