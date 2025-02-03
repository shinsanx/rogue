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

    private DungeonEventManager dungeonEventManager;

    private int actableEnemies = 0;

    public DungeonEventLogic(GameObject enemyParent, DungeonEventManager dungeonEventManager) {
        this.enemyParent = enemyParent;
        this.dungeonEventManager = dungeonEventManager;
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

        actableEnemies = enemies.Count;

        foreach(var enemy in enemies){
            var monster = enemy.GetComponent<IMonsterStatusAdapter>();
            if(monster != null) {
                //敵の行動を開始する
                monster.Action = true;
                actableEnemies -= 1;
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
