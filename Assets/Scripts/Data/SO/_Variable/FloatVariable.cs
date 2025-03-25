using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variable/FloatVariable")]
public class FloatVariable : ScriptableObject
{
    public float Value;

    public void ApplyChange(float amount) {
        Value += amount;
    }

    public void ApplyChange(FloatVariable amount) {
        Value += amount.Value;
    }

    public void SetValue(float value) {
        Value = value;
    }

    public void SetValue(FloatVariable value) {
        Value = value.Value;
    }


    
    
    
}

