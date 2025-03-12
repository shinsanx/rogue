using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vector2Variable", menuName = "Variable/Vector2Variable")]
public class Vector2Variable : ScriptableObject
{
    public Vector2 Value;

    

    public void SetValue(Vector2Int value) {
        Value = value;
    }

    public void SetValue(Vector2IntVariable value) {
        Value = value.Value;
    }

    public void SetValue(Vector2Variable value) {
        Value = value.Value;
    }

    public void SetValue(Vector2 value) {
        Value = value;
    }

    public void SetValue(Vector3 value) {
        Value = value;
    }

    
    
    
}

