using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="State", menuName = "StateMachine/State", order = 0)]
public class State : ScriptableObject
{
    public Action OnEnterEvent;
    public Action OnExitEvent;

    public void OnEnter(){
        OnEnterEvent?.Invoke();        
    }

    public void OnExit(){
        OnExitEvent?.Invoke();
    }
}
