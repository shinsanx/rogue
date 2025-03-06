using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] StatusUI statusUI;
    [SerializeField] DungeonStateManager dungeonStateManager;
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] InventoryController inventoryUI;

    private async void Start() {
        //try {
            // 1. StateMachineの初期化を最初に行う
            await InitializeStateMachine();

            // 2. ミニマップの初期化
            await InitializeAutoMapping();

            // 3. マップ生成
            await InitializeRandomMapGenerator();

            // 4. キャラクターマネージャーの初期化
            await InitializeCharacterManager();                        

            // 6. ダンジョンステートマネージャーの初期化
            await InitializeDungeonStateManager();

            // 7. プレイヤーをランダムな位置へ召喚
            await InitializePlayer();

            // 8. モンスターの生成
            //await GenerateEnemies();

            // 9. インベントリUIの初期化
            // await InitializeInventoryUI();

            // 10. アイテムの生成（必要に応じて実装）
            //await GenerateItems();

            // 11. ミニマップの生成
            await CreateMiniMap();

            Debug.Log("Initialize completed");
        // }
        // catch (System.Exception ex) {
        //     Debug.LogError($"DungeonEventManager Initialization Failed: {ex.Message}");
        // }
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
        CharacterManager.i.Initialize();
        return Task.CompletedTask;
    }    

    private Task InitializeDungeonStateManager() {
        dungeonStateManager.Initialize();
        return Task.CompletedTask;
    }

    private Task InitializePlayer() {
        player.InitializePlayer();
        player.GetComponent<IObjectData>().Position.SetValue(TileManager.i.GetRandomPosition());
        return Task.CompletedTask;
    }

    // private Task InitializeInventoryUI() {
    //     inventoryUI.InitializeMenu();
    //     return Task.CompletedTask;
    // }

    private async Task GenerateEnemies() {
        await ArrangeManager.i.ArrangeEnemyToRandomPosition();
        enemyManager.Initialize();
    }

    private async Task GenerateItems() {
        await ArrangeManager.i.ArrangeItemToRandomPosition();        
    }

    private Task CreateMiniMap() {
        randomMapGenerator.CreateMiniMap();
        return Task.CompletedTask;
    }
}
