using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPositionAdapter
{
    Vector2Int Position {get;set;}
    int CharacterType{get;set;}
}
