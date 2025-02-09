using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using System.Threading.Tasks;

public class DungeonEventLogic {

    private StateMachine stateMachine;
    private State playerState;
    private State enemyState;

    private GameObject enemyParent;

    public List<IPositionAdapter> objectsPositionAdapters = new List<IPositionAdapter>();
    public List<Transform> gameObjectsTransform = new List<Transform>();
    public List<Transform> enemies = new List<Transform>();        

    public DungeonEventLogic(GameObject enemyParent) {
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
        await Task.Delay(500); //ビミョー

        enemies = new List<Transform>(enemyParent.GetComponentsInChildren<Transform>());
        enemies.RemoveAt(0); //parent分を引く        

        foreach(var enemy in enemies){
            var monster = enemy.GetComponent<IMonsterStatusAdapter>();
            if(monster != null) {
                //敵の行動を開始する
                monster.Action = true;                
            }
        }

        EndEnemyTurn();        
    }

    public void EnemyStateExit() {
    }         

    public void StartEnemyTurn(){        
    }

    private void EndEnemyTurn(){
        stateMachine.SetState(playerState);
    }


}
