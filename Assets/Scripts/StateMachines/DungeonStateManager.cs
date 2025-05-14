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
        Debug.Log("PlayerActionComplete");
        if(playerTimeGage.Value >= 100) {
            playerTimeGage.Value = 0;
        }
        if(stateMachine == null) {
            Debug.LogError("StateMachine is null");
            return;
        }
        stateMachine.SetState(enemyState);
    }

    public void EnemyActionComplete() {
        Debug.Log("EnemyActionComplete");
        stateMachine.SetState(playerState);
    }
}
