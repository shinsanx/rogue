using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEventManager : MonoBehaviour
{
    //Todo: マップ生成
    //プレイヤーをランダムな位置へ召喚
    //アイテムの生成
    //モンスターの生成

    StateMachine stateMachine;
    State playerState;
    State enemyState;
    [SerializeField] GameObject enemyParent;    
    DungeonEventLogic dungeonEventLogic;
    
    
    void Start()
    {
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;
        stateMachine.init();

        dungeonEventLogic = new DungeonEventLogic(enemyParent);

        playerState.OnEnterEvent += dungeonEventLogic.PlayerStateStart;
        enemyState.OnEnterEvent += dungeonEventLogic.EnemyStateStart;

        playerState.OnExitEvent += dungeonEventLogic.PlayerStateExit;
        enemyState.OnExitEvent += dungeonEventLogic.EnemyStateExit;
                

    }
    
}
