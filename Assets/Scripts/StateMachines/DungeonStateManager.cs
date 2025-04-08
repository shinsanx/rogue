using System.Collections;
using System.Collections.Generic;
using RandomDungeonWithBluePrint;
using UnityEngine;

// ダンジョンの状態を管理するクラス
public class DungeonStateManager : MonoBehaviour {

    StateMachine stateMachine;
    State playerState;
    State enemyState;
    [SerializeField] private IntVariable playerTimeGage;

    public void Initialize() {
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;        
    }

    // Playerのターンを終わらせる
    public void PlayerActionComplete() {
        if(playerTimeGage.Value >= 100) {
            playerTimeGage.Value = 0;
        }
        stateMachine.SetState(enemyState);
    }

    public void EnemyActionComplete() {
        stateMachine.SetState(playerState);
    }
}
