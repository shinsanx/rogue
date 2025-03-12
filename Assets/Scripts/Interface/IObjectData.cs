using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectData
{
    IntVariable Id {get;}
    StringVariable Name {get;}
    StringVariable Type{get;}
    Vector2IntVariable Position{get;}
    IntVariable RoomNum{get;}

    // オブジェクト更新時のイベント
    event System.Action<IObjectData> OnObjectUpdated;

    void SetPosition(Vector2Int position);
}
