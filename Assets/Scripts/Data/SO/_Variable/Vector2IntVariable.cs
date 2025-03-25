using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vector2IntVariable", menuName = "Variable/Vector2IntVariable")]
public class Vector2IntVariable : ScriptableObject
{
    public Vector2Int Value;

    

    public void SetValue(Vector2Int value) {
        Value = value;
    }

    public void SetValue(Vector2IntVariable value) {
        Value = value.Value;
    }

    public void SetValue(Vector2 value) {
        Value = value.ToVector2Int();
    }

    public void SetValue(Vector3 value) {
        Value = value.ToVector2Int();
    }

    
    
    
}

