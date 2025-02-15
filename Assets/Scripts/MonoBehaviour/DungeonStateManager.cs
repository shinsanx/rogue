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


    public void Initialize() {
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;        

        dungeonStateLogic = new DungeonStateLogic(enemyParent, enemyManager);

        playerState.OnEnterEvent += dungeonStateLogic.PlayerStateStart;
        enemyState.OnEnterEvent += dungeonStateLogic.EnemyStateStart;

        playerState.OnExitEvent += dungeonStateLogic.PlayerStateExit;
        enemyState.OnExitEvent += dungeonStateLogic.EnemyStateExit;
    }

}
