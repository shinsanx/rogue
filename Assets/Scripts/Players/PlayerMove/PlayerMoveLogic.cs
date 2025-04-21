
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// プレイヤーの移動に関するロジックを管理するクラス
/// </summary>
public class PlayerMoveLogic {
    // ================================================
    // ================ フィールド変数 ================
    // ================================================

    // 基本コンポーネント
    private readonly ObjectData objectData;
    private readonly TileManager tileManager;

    // Handler
    private readonly PlayerMoveHandler moveHandler;
    private readonly PlayerItemHandler itemHandler;
    private readonly PlayerStairHandler stairHandler;
    private readonly PlayerDashHandler dashHandler;
    private readonly PlayerDirectionHandler directionHandler;
    
    // ================================================
    // ================ コンストラクタ ================
    // ================================================
    public PlayerMoveLogic(
        BoolVariable playerCanMove,
        ObjectData playerObjectData,
        GameEvent OnPlayerStateComplete,
        GameEvent OnPlayerDirectionChanged,
        Vector2Variable playerFaceDirection,
        ItemEventChannelSO OnItemPicked,
        CurrentSelectedObjectSO currentSelectedObjectSO,
        BoolVariable fixDiagonalInput,
        TileManager tileManager
        ) {
                
        this.objectData = playerObjectData;                        
        this.tileManager = tileManager;        
        this.objectData.SetTileManager(tileManager);
        moveHandler = new PlayerMoveHandler(playerCanMove, playerObjectData, OnPlayerStateComplete, OnPlayerDirectionChanged, playerFaceDirection, fixDiagonalInput, tileManager);
        itemHandler = new PlayerItemHandler(currentSelectedObjectSO, OnItemPicked, tileManager);
        stairHandler = new PlayerStairHandler(tileManager);
        dashHandler = new PlayerDashHandler(playerObjectData, tileManager, moveHandler, playerFaceDirection, OnPlayerDirectionChanged);
        directionHandler = new PlayerDirectionHandler(playerFaceDirection, OnPlayerDirectionChanged);
    }

    // ================================================
    // ================ 通常移動処理 ================
    // ================================================

    /// <summary>
    /// 入力ベクトルに基づいてプレイヤーを移動させる
    /// </summary>
    public void MoveByInput(Vector2 inputVector) {
        Vector2Int inputVectorInt = new Vector2Int(Mathf.RoundToInt(inputVector.x), Mathf.RoundToInt(inputVector.y));
        Vector2Int currentPos = objectData.Position.Value;
        Vector2Int targetPos = inputVectorInt + currentPos;

        // 先にアイテムを確認
        itemHandler.TryPickupItem(targetPos);

        // 階段があるか確認し、あれば選択処理を実行する 
        stairHandler.TryUseStair(targetPos);

        // その後に実際の移動処理
        moveHandler.MoveByInput(inputVector);
    }


    /// <summary>
    /// アイテムを拾った時の処理
    /// </summary>
    public void HandleItemPicked(bool success) {
        itemHandler.HandleItemPicked(success);
    }

    /// <summary>
    /// 入力方向にダッシュする
    /// </summary>
    public async Task DashByInput(Vector2 inputVector) {
        await dashHandler.DashByInput(inputVector);
    }

    /// <summary>
    /// Z入力によるダッシュ処理
    /// </summary>
    public async Task ZDash(Vector2 inputVector) {        
        await dashHandler.ZDash(inputVector);
    }

    /// <summary>
    /// 周囲の敵に基づいて自動的に向きを変更する
    /// </summary>
    public void AutoTurn(Vector2Int currentPos) {
        directionHandler.AutoTurn(currentPos, tileManager);
    }

    /// <summary>
    /// 入力に基づいて手動で向きを変更する
    /// </summary>
    public void ManualTurn(Vector2 inputVector) {
        directionHandler.ManualTurn(inputVector);
    }

    /// <summary>
    /// ランダムに移動する
    /// </summary>    
    public bool RandomMove() {
        return moveHandler.RandomMove();
    }
}
