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
        // TODO:UIを有効化してプレイヤーのアクションを待つ        
    }

    public void PlayerStateExit() {        
    }

    public async void EnemyStateStart() {         
        await Task.Delay(10); //ビミョー

        enemies = CharacterManager.i.GetAllEnemies();                
        int completedActions = 0;
        int totalEnemies = enemies.Count;

        // 一つのイベントハンドラを作成
        Action<object> actionCompleteHandler = null;
        actionCompleteHandler = (object data) => {
            if (data is int enemyId && enemies.Any(e => e.Id == enemyId)) {
                completedActions++;
                
                // すべての敵のアクションが完了したらクリーンアップしてターン終了
                if (completedActions >= totalEnemies) {
                    MessageBus.Instance.Unsubscribe("EnemyActionComplete", actionCompleteHandler);
                    EndEnemyTurn();
                }
            }
        };

        // 一度だけSubscribeする
        MessageBus.Instance.Subscribe("EnemyActionComplete", actionCompleteHandler);

        // 全ての敵のアクションを開始
        foreach(var enemy in enemies) {
            enemy.ActionStart();
        }
    }

    public void EnemyStateExit() {
    }         

    public void StartEnemyTurn(){        
    }

    private void EndEnemyTurn(){
        MessageBus.Instance.Publish("CreateCharacterUI", null);
        stateMachine.SetState(playerState);
    }


}
