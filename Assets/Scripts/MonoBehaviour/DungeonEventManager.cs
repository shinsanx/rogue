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
    [SerializeField] GameObject objectParent;
    [SerializeField] GameObject enemyParent;
    [SerializeField] RandomDungeonWithBluePrint.RandomMapTest randomMapTest;
    DungeonEventLogic dungeonEventLogic;

    public DungeonData dungeonData;
    
    void Start()
    {
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;
        stateMachine.init();

        dungeonData = new DungeonData();

        dungeonEventLogic = new DungeonEventLogic(randomMapTest.currentField, objectParent, enemyParent, dungeonData);

        playerState.OnEnterEvent += dungeonEventLogic.PlayerStateStart;
        enemyState.OnEnterEvent += dungeonEventLogic.EnemyStateStart;

        playerState.OnExitEvent += dungeonEventLogic.PlayerStateExit;
        enemyState.OnExitEvent += dungeonEventLogic.EnemyStateExit;

        //Enemyが行動終了後に通知できるようSubscribeしておく
        MessageBus.Instance.Subscribe(DungeonConstants.NotifyEnemyAct, dungeonEventLogic.NotifyEnemyAct);
        //プレイヤーのポジションを取得する用
        MessageBus.Instance.DelegateSubscribe<Vector2Int>(DungeonConstants.GetPlayerPosition, dungeonEventLogic.GetPlayerPosition);
    }

    
}
