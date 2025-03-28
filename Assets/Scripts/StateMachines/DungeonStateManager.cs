using System.Collections;
using System.Collections.Generic;
using RandomDungeonWithBluePrint;
using UnityEngine;

// ダンジョンの状態を管理するクラス
public class DungeonStateManager : MonoBehaviour {

    StateMachine stateMachine;
    State playerState;
    State enemyState;
    [SerializeField] GameObject enemyParent;
    [SerializeField] EnemyManager enemyManager;
    DungeonStateLogic dungeonStateLogic;
    [SerializeField] GameEvent updateMiniMap;


    public void Initialize() {
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;        

        dungeonStateLogic = new DungeonStateLogic(enemyManager, updateMiniMap);

        playerState.OnEnterEvent += dungeonStateLogic.PlayerStateStart;
        enemyState.OnEnterEvent += dungeonStateLogic.EnemyStateStart;

        playerState.OnExitEvent += dungeonStateLogic.PlayerStateExit;
        enemyState.OnExitEvent += dungeonStateLogic.EnemyStateExit;
        
    }

    // Playerのターンを終わらせる
    public void PlayerActionComplete() {
        stateMachine.SetState(enemyState);
    }
}
