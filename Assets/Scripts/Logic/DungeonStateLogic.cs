using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using System.Threading.Tasks;

public class DungeonStateLogic {

    private StateMachine stateMachine;
    private State playerState;
    private State enemyState;    
    private GameObject enemyParent;

    public List<IPositionAdapter> objectsPositionAdapters = new List<IPositionAdapter>();
    public List<Transform> gameObjectsTransform = new List<Transform>();
    public List<Enemy> enemies = new List<Enemy>();        

    public DungeonStateLogic(GameObject enemyParent) {        
        this.enemyParent = enemyParent;        
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;
    }

    public void PlayerStateStart() {
        Debug.Log("PlayerStateStart - Beginning");  // デバッグログを追加
        // TODO:UIを有効化してプレイヤーのアクションを待つ     
    }

    public void PlayerStateExit() {        
    }

    public async void EnemyStateStart() {         
        Debug.Log("EnemyStateStart - Beginning");
        await Task.Delay(10);

        enemies = CharacterManager.i.GetAllEnemies();
        Debug.Log($"Found {enemies.Count} enemies");
                
        int completedActions = 0;
        int totalEnemies = enemies.Count;

        // 一つのイベントハンドラを作成
        Action<object> actionCompleteHandler = null;
        actionCompleteHandler = (object data) => {
            Debug.Log($"Enemy action complete received: {data}");
            if (data is int enemyId && enemies.Any(e => e.Id == enemyId)) {
                completedActions++;
                Debug.Log($"Completed actions: {completedActions}/{totalEnemies}");
                
                // すべての敵のアクションが完了したらクリーンアップしてターン終了
                if (completedActions >= totalEnemies) {
                    Debug.Log("All enemies completed their actions");
                    MessageBus.Instance.Unsubscribe("EnemyActionComplete", actionCompleteHandler);
                    EndEnemyTurn();
                }
            }
        };

        MessageBus.Instance.Subscribe("EnemyActionComplete", actionCompleteHandler);
        Debug.Log("Starting enemy actions");

        foreach(var enemy in enemies) {
            Debug.Log($"Starting action for enemy ID: {enemy.Id}");
            enemy.ActionStart();
        }
    }

    public void EnemyStateExit() {        
    }         

    public void StartEnemyTurn(){        
    }

    private void EndEnemyTurn(){
        Debug.Log("EndEnemyTurn");
        MessageBus.Instance.Publish("CreateCharacterUI", null);
        stateMachine.SetState(playerState);
    }


}
