using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StringVariable", menuName = "Variable/StringVariable")]
public class StringVariable : ScriptableObject
{
    [SerializeField] private string value = "";

    public string Value {
        get => value;
        set => this.value = value;
    }

    public void SetValue(string value) {
        this.value = value;
    }

    public void SetValue(StringVariable value) {
        this.value = value.Value;
    }
}
