using System.Collections;
using System.Collections.Generic;
using RandomDungeonWithBluePrint;
using UnityEngine;

public class DungeonEventManager : MonoBehaviour {
    //Todo: マップ生成
    //プレイヤーをランダムな位置へ召喚
    //アイテムの生成
    //モンスターの生成                    

    [SerializeField] RandomMapGenerator randomMapGenerator;
    [SerializeField] AutoMapping autoMapping;
    [SerializeField] Player player;
    [SerializeField] GameObject enemyParent;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] StatusUI statusUI;
    [SerializeField] int generateEnemyCount;
    [SerializeField] DungeonStateManager dungeonStateManager;
    [SerializeField] EnemyManager enemyManager;

    private void Start() {
        // StateMachineの初期化を最初に行う
        GameAssets.i.stateMachine.init();
        // ミニマップの初期化
        autoMapping.Initialize();
        
        // マップ生成
        randomMapGenerator.Initialize();

        // キャラクターマネージャーの初期化
        CharacterManager.i.Initialize();

        // ステータスUIの初期化
        statusUI.Initialize();

        // ダンジョンステートマネージャーの初期化
        dungeonStateManager.Initialize();

        // プレイヤーをランダムな位置へ召喚
        player.InitializePlayer();
        player.GetComponent<IObjectData>().Position = TileManager.i.GetRandomPosition();
        
        // モンスターの生成
        for (int i = 0; i < generateEnemyCount; i++) {
            GameObject enemy = Instantiate(enemyPrefab, enemyParent.transform);
            enemy.GetComponent<Enemy>().InitializeEnemy();
            enemy.GetComponent<IObjectData>().Position = TileManager.i.GetRandomPosition();
        }
        enemyManager.Initialize();
        
        // アイテムの生成
        // ミニマップの生成
        randomMapGenerator.CreateMiniMap();

        Debug.Log("Initialize completed");
    }

}
