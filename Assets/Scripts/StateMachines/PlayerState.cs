using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "StateMachine/PlayerState", order = 0)]
public class PlayerState : State
{
    [SerializeField] GameEvent playerTurnEndEvent;
    [SerializeField] IntVariable sleepTurnCount;
    public override void OnEnter() {        
        if(sleepTurnCount.Value > 0) {
            sleepTurnCount.Value--;
            playerTurnEndEvent.Raise();
            Debug.Log("残りの睡眠ターン:" + sleepTurnCount.Value);
        }                         
    }

    public override void OnExit() {
        base.OnExit();
        // PlayerStateが終了したときの処理を書く        
    }
}
