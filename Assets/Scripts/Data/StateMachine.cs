using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StateMachine", menuName ="StateMachine/StateMachine", order = 0)]
public class StateMachine : ScriptableObject
{
    public State CurrentState => _currentState;
    public State _currentState;

    [SerializeField] private State defaultState;

    //Stateが変わった時に呼ばれるイベント 未使用。
    // public event Action<State> OnStateChanged;

    public void init(){
        _currentState = null; //生成した時に前の情報を引き継がない
        SetState(defaultState);
    }

    public void SetState(State newState){
        if(_currentState != null){
            _currentState.OnExit();
        }

        _currentState = newState;
        _currentState.OnEnter();
    }
}
