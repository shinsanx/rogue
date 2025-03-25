using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variable/IntVariable")]
public class IntVariable : ScriptableObject
{
    public int Value;

    public void ApplyChange(int amount) {
        Value += amount;
    }

    public void ApplyChange(IntVariable amount) {
        Value += amount.Value;
    }

    public void SetValue(int value) {
        Value = value;
    }

    public void SetValue(IntVariable value) {
        Value = value.Value;
    }


    
    
    
}

