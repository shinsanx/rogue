using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="State", menuName = "StateMachine/State", order = 0)]
public class State : ScriptableObject
{
    public Action OnEnterEvent;
    public Action OnExitEvent;

    public virtual void OnEnter(){
        OnEnterEvent?.Invoke();        
    }

    public virtual void OnExit(){
        OnExitEvent?.Invoke();
    }
}
