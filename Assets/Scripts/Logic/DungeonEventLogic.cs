using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using System.Threading.Tasks;

public class DungeonEventLogic
{
    private StateMachine stateMachine;
    private State playerState;
    private State enemyState;
    private RandomDungeonWithBluePrint.Field field;
    private TileLogic tileLogic;
    public GameObject objectParent;
    public GameObject enemyParent;
    private DungeonData dungeonData;

    public List<IPositionAdapter> objectsPositionAdapters = new List<IPositionAdapter>();
    public List<Transform> gameObjectsTransform = new List<Transform>();
    public List<Transform> enemies = new List<Transform>();

    private int _actableEnemies = default;
    public int ActableEnemies{
        get {return _actableEnemies;}
        set{
            _actableEnemies = value;
            if(_actableEnemies <= 0){
                stateMachine.SetState(playerState);
            } else {
                IMonsterStatusAdapter monsterStatusAdapter = enemies[_actableEnemies-1].GetComponent<IMonsterStatusAdapter>();
                if(monsterStatusAdapter != null)monsterStatusAdapter.Action = true;
            }
        }
    }

    public DungeonEventLogic(RandomDungeonWithBluePrint.Field field, GameObject objectParent, GameObject enemyParent, DungeonData dungeonData){
        this.field = field;
        this.objectParent = objectParent;
        this.enemyParent = enemyParent;
        this.dungeonData = dungeonData;

        tileLogic = new TileLogic();
        stateMachine = GameAssets.i.stateMachine;
        playerState = GameAssets.i.playerState;
        enemyState = GameAssets.i.enemyState;
    }

    public void PlayerStateStart(){
        
    }

    public void PlayerStateExit(){
        ResetObjectTypeAndPosition();
        dungeonData.playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<IObjectData>().Position;
    }

    public async void EnemyStateStart(){
        await Task.Delay(500); //ビミョー
        ConfirmActableEnemies();
    }

    public void EnemyStateExit(){
        ResetObjectTypeAndPosition();
    }

    //敵個々が行動したかどうかを検出する
    public void ConfirmActableEnemies(){
        enemies = enemyParent.GetComponentsInChildren<Transform>().Skip(1).ToList();
        foreach(var enemy in enemies){
            IMonsterStatusAdapter monsterStatusAdapter = enemy.GetComponent<IMonsterStatusAdapter>();            
        }

        //未行動のEnemyの数。ゼロになると(プロパティで)PlayerTurnになる。
        ActableEnemies = enemies.Count; //parent分を引く
    }

    //メッセージバスで利用。DungeonEventManagerがpublish。
    public void NotifyEnemyAct(object data){
        ActableEnemies -= 1;
    }

    //すべてのオブジェクトのタイプとポジションをtileLogicに渡す。重いかも？
    public void ResetObjectTypeAndPosition(){
        objectsPositionAdapters.Clear();
        gameObjectsTransform.Clear();

        objectParent.transform.GetComponentsInChildren(objectsPositionAdapters);
        objectParent.GetComponentsInChildren(gameObjectsTransform);

        tileLogic.SetObjectToTile(gameObjectsTransform);
        tileLogic.DistributeGameObjectToRooms(gameObjectsTransform);
    }

    //PlayerPositionを返す。ほんとにこのクラスでいいの？
    public Vector2Int GetPlayerPosition(object data){
        return dungeonData.playerPosition;
    }
}
