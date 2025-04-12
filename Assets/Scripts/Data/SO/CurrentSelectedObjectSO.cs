using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CurrentSelectedObjectSO", fileName = "Dungeon/CurrentSelectedObjectSO")]
public class CurrentSelectedObjectSO : ScriptableObject
{
    public GameObject Object;   
    public BaseItemSO Item;  
    public SubmitMenuSet SubmitMenuSet;

    public void ResetCurrentSelectedObject() {
        Object = null;        
    }

    public void ResetCurrentSelectedSubmitMenuSet() {
        SubmitMenuSet = null;
    }

    
}
