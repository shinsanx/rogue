using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using RandomDungeonWithBluePrint;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class DungeonEventManager : MonoBehaviour {
    //Todo: マップ生成
    //プレイヤーをランダムな位置へ召喚
    //アイテムの生成
    //モンスターの生成                    

    [SerializeField] RandomMapGenerator randomMapGenerator;
    [SerializeField] AutoMapping autoMapping;
    [SerializeField] Player player;
    [SerializeField] StatusUI statusUI;
    [SerializeField] DungeonStateManager dungeonStateManager;
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] InventoryController inventoryUI;
    [SerializeField] DungeonDataSO dungeonData;
    [SerializeField] CharacterManager characterManager;
    [SerializeField] TileManager tileManager;

    private EnemyTableSO currentEnemyTable;
    private ItemTableSO currentItemTable;



    private async void Start() {
        // 1. TileManagerの初期化
        await InitializeTileManager();

        
        // 2. キャラクターマネージャーの初期化
        await InitializeCharacterManager();
        // 1. StateMachineの初期化を最初に行う
        await InitializeStateMachine();


        // 2. ミニマップの初期化
        await InitializeAutoMapping();

        // 3. マップ生成
        await InitializeRandomMapGenerator();



        // 6. ダンジョンステートマネージャーの初期化
        await InitializeDungeonStateManager();

        // 7. プレイヤーをランダムな位置へ召喚
        await InitializePlayer();

        // 8. ダンジョンデータの読み込み
        LoadDungeonData();

        // 9. モンスターの生成
        await GenerateEnemies();

        // 10. インベントリUIの初期化
        // await InitializeInventoryUI();

        // 11. アイテムの生成（必要に応じて実装）
        //await GenerateItems();

        // 12. ミニマップの生成
        //await CreateMiniMap();

        Debug.Log("Initialize completed");
        // }
        // catch (System.Exception ex) {
        //     Debug.LogError($"DungeonEventManager Initialization Failed: {ex.Message}");
        // }
    }

    //ダンジョンデータを読み込む
    private void LoadDungeonData() {
        currentEnemyTable = dungeonData.DungeonTable.Floors[0].EnemyTable;
        currentItemTable = dungeonData.DungeonTable.Floors[0].ItemTable;
    }

    // //EnemyTableから敵をランダムに選択する
    // private MonsterStatusSO SelectEnemy() {
    //     int randomIndex = Random.Range(0, currentEnemyTable.Enemies.Count);
    //     return currentEnemyTable.Enemies[randomIndex];
    // }

    //ItemTableからアイテムをランダムに選択する
    private ItemSO SelectItem() {
        int randomIndex = Random.Range(0, currentItemTable.Items.Count);
        return currentItemTable.Items[randomIndex];
    }

    private Task InitializeStateMachine() {
        // StateMachineの初期化はメインスレッドで実行
        GameAssets.i.stateMachine.init();
        return Task.CompletedTask;
    }

    private Task InitializeAutoMapping() {
        autoMapping.Initialize();
        return Task.CompletedTask;
    }

    private Task InitializeRandomMapGenerator() {
        randomMapGenerator.Initialize();
        return Task.CompletedTask;
    }

    private Task InitializeCharacterManager() {
        characterManager.Initialize();
        return Task.CompletedTask;
    }

    private Task InitializeDungeonStateManager() {
        dungeonStateManager.Initialize();
        return Task.CompletedTask;
    }

    private Task InitializePlayer() {
        player.InitializePlayer();
        player.playerObjectData.SetPosition(TileManager.i.GetRandomPosition());
        return Task.CompletedTask;
    }

    // private Task InitializeInventoryUI() {
    //     inventoryUI.InitializeMenu();
    //     return Task.CompletedTask;
    // }

    private async Task GenerateEnemies() {
        await ArrangeManager.i.ArrangeEnemyToRandomPosition(currentEnemyTable.Enemies, dungeonData.DungeonTable.Floors[0].InitialEnemyCount);
        enemyManager.Initialize();
    }

    private async Task GenerateItems() {
        await ArrangeManager.i.ArrangeItemToRandomPosition();
    }

    private Task CreateMiniMap() {
        randomMapGenerator.CreateMiniMap();
        return Task.CompletedTask;
    }

    private Task InitializeTileManager() {
        tileManager.Initialize();
        return Task.CompletedTask;
    }
}
