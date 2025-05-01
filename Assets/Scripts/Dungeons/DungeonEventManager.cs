using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.LuisPedroFonseca.ProCamera2D;
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
    [SerializeField] DungeonDataSO dungeonData;
    //[SerializeField] CharacterManager characterManager;
    [SerializeField] TileManager tileManager;
    [SerializeField] CurrentDungeonData currentDungeonData;
    private EnemyTableSO currentEnemyTable;
    private ItemTableSO currentItemTable;

    [SerializeField] private ProCamera2DNumericBoundaries numericBoundaries;


    private async void Start() {
        // 1. TileManagerの初期化
        await InitializeTileManager();


        // 2. キャラクターマネージャーの初期化
        //await InitializeCharacterManager();
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
        await InitializeInventoryUI();

        // 11. アイテムの生成（必要に応じて実装）
        await GenerateItems();

        // 12. 階段の生成
        await GenerateStair();

        // 13. ミニマップの生成
        await CreateMiniMap();

        // 14. ダンジョンデータの保存
        SaveDungeonData();

        Debug.Log("Initialize completed");
        // }
        // catch (System.Exception ex) {
        //     Debug.LogError($"DungeonEventManager Initialization Failed: {ex.Message}");
        // }
    }

    private async Task NextFloor() {      
        ArrangeManager.i.DestroyAllObjects();
        
                // 2. キャラクターマネージャーの初期化
        //await InitializeCharacterManager();
        // 1. StateMachineの初期化を最初に行う
        await InitializeStateMachine();

        // 2. ミニマップの初期化
        await InitializeAutoMapping();

        // 3. マップ生成
        await InitializeRandomMapGenerator();

        InitializeMapBoundaries(randomMapGenerator.currentField.Size);

        // 6. ダンジョンステートマネージャーの初期化
        await InitializeDungeonStateManager();

        // 7. プレイヤーをランダムな位置へ召喚
        await InitializePlayer();

        // 8. ダンジョンデータの読み込み
        LoadDungeonData();

        // 9. モンスターの生成
        await GenerateEnemies();

        // 10. インベントリUIの初期化
        await InitializeInventoryUI();

        // 11. アイテムの生成（必要に応じて実装）
        await GenerateItems();

        // 12. 階段の生成
        await GenerateStair();

        // 13. ミニマップの生成
        await CreateMiniMap();

        // 14. ダンジョンデータの保存
        SaveDungeonData();
        MenuManager.Instance.CloseAllMenus();
    }

    //ダンジョンデータを読み込む
    private void LoadDungeonData() {
        currentEnemyTable = dungeonData.DungeonTable.Floors[currentDungeonData.currentFloor].EnemyTable;
        currentItemTable = dungeonData.DungeonTable.Floors[currentDungeonData.currentFloor].ItemTable;
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
        randomMapGenerator.Initialize(dungeonData.DungeonFieldTable.BluePrintWithWeights);
        return Task.CompletedTask;
    }

    // private Task InitializeCharacterManager() {
    //     characterManager.Initialize();
    //     return Task.CompletedTask;
    // }

    private Task InitializeDungeonStateManager() {
        dungeonStateManager.Initialize();
        return Task.CompletedTask;
    }

    private Task InitializePlayer() {
        player.InitializePlayer();
        player.playerObjectData.SetPosition(TileManager.i.GetRandomPosition());
        return Task.CompletedTask;
    }

    private Task InitializeInventoryUI() {
        //inventoryUI.InitializeMenu();
        return Task.CompletedTask;
    }

    private async Task GenerateEnemies() {
        await ArrangeManager.i.ArrangeEnemyToRandomPosition(currentEnemyTable.Enemies, dungeonData.DungeonTable.Floors[0].InitialEnemyCount);
        enemyManager.Initialize();
    }

    private async Task GenerateItems() {
        await ArrangeManager.i.ArrangeItemToRandomPosition(currentItemTable, dungeonData.DungeonTable.Floors[0].InitialItemCount);
    }

    private Task CreateMiniMap() {
        randomMapGenerator.CreateMiniMap();
        return Task.CompletedTask;
    }

    private Task InitializeTileManager() {
        tileManager.Initialize();
        return Task.CompletedTask;
    }

    private void SaveDungeonData() {
        currentDungeonData.currentDungeonData = dungeonData;
    }

    private async Task GenerateStair() {
        await ArrangeManager.i.ArrangeStairToRandomPosition();
    }

    public async void MoveOnFloor(){
        currentDungeonData.currentFloor++;
        await NextFloor();
    }

    private void InitializeMapBoundaries(Vector2 mapSize) {
        // 左右と上下の範囲を設定する
        numericBoundaries.LeftBoundary = 0;
        numericBoundaries.RightBoundary = mapSize.x;
        numericBoundaries.BottomBoundary = 0;
        numericBoundaries.TopBoundary = mapSize.y;

    }
    
}
