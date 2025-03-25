using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variable/BoolVariable")]
public class BoolVariable : ScriptableObject
{
    public bool Value;

    public void ApplyChange(bool value) {
        Value = value;
    }

    public void ApplyChange(BoolVariable value) {
        Value = value.Value;
    }

    public void SetValue(bool value) {
        Value = value;
    }

    public void SetValue(BoolVariable value) {
        Value = value.Value;
    }


    
    
    
}

