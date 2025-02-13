using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "PlayerState/PlayerState", order = 0)]
public class PlayerState : State
{
    public override void OnEnter() {
        Debug.Log("PlayerState OnEnter - Beginning");  // デバッグログを追加
        base.OnEnter();
        //  Debug.Log("PlayerState start");
        // PlayerStateがスタートしたときの処理を書く
    }

    public override void OnExit() {
        base.OnExit();
        // PlayerStateが終了したときの処理を書く        
    }
}
