using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
using RandomDungeonWithBluePrint;

/// <summary>
/// プレイヤーの移動に関するロジックを管理するクラス
/// </summary>
public class PlayerMoveLogic {
    // ================================================
    // ================ フィールド変数 ================
    // ================================================
    
    // 基本コンポーネント
    private IObjectData objectData;
    private StateMachine stateMachine;
    private Player player;
    
    // 設定変数
    private Vector2Variable playerFaceDirection;
    private BoolVariable fixDiagonalInput;
    private CurrentSelectedObjectSO currentSelectedObjectSO;
    
    // アイテム関連
    private GameObject currentItemObject;
    
    // 移動関連
    private Vector2 inputVector;
    private float roundX;
    private float roundY;
    private Vector2 moveOffset = new Vector2(.5f, .5f);
    private bool isMoving = false;
    private List<Vector2Int> inputs = new List<Vector2Int>();
    
    // ================================================
    // ============= イベントチャンネル =============
    // ================================================
    private GameEvent OnPlayerStateComplete;
    public GameEvent OnPlayerDirectionChanged;
    public ItemEventChannelSO OnItemPicked;

    // ================================================
    // ================ コンストラクタ ================
    // ================================================
    public PlayerMoveLogic(
        Player player,
        GameEvent OnPlayerStateComplete,
        GameEvent OnPlayerDirectionChanged,
        Vector2Variable playerFaceDirection,
        ItemEventChannelSO OnItemPicked,
        CurrentSelectedObjectSO currentSelectedObjectSO,
        BoolVariable fixDiagonalInput) {

        this.player = player;
        this.stateMachine = GameAssets.i.stateMachine;
        this.OnPlayerStateComplete = OnPlayerStateComplete;
        this.objectData = player.playerObjectData;
        this.OnPlayerDirectionChanged = OnPlayerDirectionChanged;
        this.playerFaceDirection = playerFaceDirection;
        this.OnItemPicked = OnItemPicked;
        this.currentSelectedObjectSO = currentSelectedObjectSO;
        this.fixDiagonalInput = fixDiagonalInput;
    }

    // ================================================
    // ================ 通常移動処理 ================
    // ================================================
    
    /// <summary>
    /// 入力ベクトルに基づいてプレイヤーを移動させる
    /// </summary>
    public void MoveByInput(Vector2 inputVector) {
        this.inputVector = inputVector;
        
        // 入力値を四捨五入して整数値に変換
        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int inputVectorInt = new Vector2Int((int)roundX, (int)roundY);
        Vector2Int currentPos = objectData.Position.Value;
        Vector2Int targetPos = inputVectorInt + currentPos;

        // 移動中なら処理しない
        if (isMoving) return;
        
        inputs.Add(inputVectorInt);
        ProcessInputAsync(currentPos, targetPos);

        // 階段があれば選択処理を実行
        CheckAndInteractWithStairs(targetPos);
    }

    /// <summary>
    /// 階段があるか確認し、あれば選択処理を実行する
    /// </summary>
    private void CheckAndInteractWithStairs(Vector2Int targetPos) {
        GameObject stair = TileManager.i.CheckExistStair(targetPos);
        if (stair != null) {
            stair.GetComponent<IMenuActionAdapter>().OnSelected();
        }
    }

    /// <summary>
    /// 入力を処理し、適切な方向に移動する
    /// </summary>
    private async void ProcessInputAsync(Vector2Int currentPos, Vector2Int targetPos) {
        // 入力処理の遅延（連続入力対策）
        await Task.Delay(30);

        // 単一方向の入力
        if (inputs.Count == 1) {
            Move(currentPos, targetPos);
        }

        // 斜め方向の入力を処理
        ProcessDiagonalInput(currentPos);
        
        // 入力リストをクリア
        inputs.Clear();
    }

    /// <summary>
    /// 斜め方向の入力を処理する
    /// </summary>
    private void ProcessDiagonalInput(Vector2Int currentPos) {
        // 斜め方向の入力を優先順位に従って処理
        if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.UpRight])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.UpRight]);
        } else if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.UpLeft])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.UpLeft]);
        } else if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.DownRight])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.DownRight]);
        } else if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.DownLeft])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.DownLeft]);
        }
    }

    /// <summary>
    /// 指定された位置にプレイヤーを移動させる
    /// </summary>
    private void Move(Vector2Int currentPos, Vector2Int targetPos) {
        // プレイヤーのターンでなければ移動しない
        if (stateMachine.CurrentState != GameAssets.i.playerState) {
            Debug.Log("playerのターンではありません");
            return;
        }

        // プレイヤーが移動中なら処理しない
        if (player.IsMoving()) {
            Debug.Log("playerが動いています");
            return;
        }

        // プレイヤーの向きを変更
        UpdatePlayerDirection();

        // 斜め移動の制限を適用
        if (ShouldRestrictDiagonalMovement()) return;

        // 移動可能なタイルでなければ処理しない
        if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) return;

        // 移動先にアイテムがあれば拾う
        TryPickupItem(targetPos);

        // 位置を更新
        UpdatePlayerPosition(targetPos);
        
        // プレイヤーのターン終了を通知
        OnPlayerStateComplete.Raise();
    }

    /// <summary>
    /// プレイヤーの向きを更新する
    /// </summary>
    private void UpdatePlayerDirection() {
        playerFaceDirection.SetValue(new Vector2(roundX, roundY));
        OnPlayerDirectionChanged.Raise();
    }

    /// <summary>
    /// 斜め移動を制限すべきかどうかを判定
    /// </summary>
    private bool ShouldRestrictDiagonalMovement() {
        // 斜め移動を固定する設定がオンで、斜め方向の入力の場合
        return fixDiagonalInput.Value && (roundX != 0 && roundY != 0);
    }

    /// <summary>
    /// 指定位置にあるアイテムを拾う処理を試みる
    /// </summary>
    private void TryPickupItem(Vector2Int targetPos) {
        Item item = TileManager.i.CheckExistItem(targetPos);
        if (item != null) {
            Debug.Log(targetPos + "にアイテムがあります");
            currentSelectedObjectSO.Object = item.gameObject;
            
            // itemSOがnullでないことを確認
            if (item.itemSO != null) {
                currentItemObject = item.gameObject;
                OnItemPicked.RaiseEvent(item.itemSO);
            } else {
                Debug.LogError("Item " + item.name + " has no ItemSO assigned!");
            }
        }
    }

    /// <summary>
    /// プレイヤーの位置を更新する
    /// </summary>
    private void UpdatePlayerPosition(Vector2Int targetPos) {
        Vector2 newPosition = targetPos + moveOffset;
        objectData.SetPosition(newPosition.ToVector2Int());
    }

    /// <summary>
    /// アイテムを拾った時の処理
    /// </summary>
    public void HandleItemPicked(bool success) {
        if (success) {
            Item item = currentItemObject.GetComponent<Item>();
            if (item != null) {
                item.OnPicked();
            }
        } else {
            Debug.Log("アイテムを拾えませんでした。");
        }
    }

    // ================================================
    // ================ ダッシュ処理 ================
    // ================================================
    
    /// <summary>
    /// 入力方向にダッシュする
    /// </summary>
    public async void DashByInput(Vector2 inputVector) {
        this.inputVector = inputVector;
        
        // 入力値を四捨五入して整数値に変換
        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int direction = new Vector2Int((int)roundX, (int)roundY);
        Vector2Int currentPos = objectData.Position.Value;

        // アイテムに乗る処理
        if (TryGetOnItem(currentPos, direction)) return;

        // オブジェクトがあるまで移動する
        await DashUntilObstacleAsync(currentPos, direction);
    }

    /// <summary>
    /// アイテムに乗る処理を試みる
    /// </summary>
    private bool TryGetOnItem(Vector2Int currentPos, Vector2Int direction) {
        Item item = TileManager.i.CheckExistItem(currentPos + direction) as Item;
        if (item != null) {
            item.OnGetOnItem();
            Move(currentPos, currentPos + direction);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 障害物があるまでダッシュする
    /// </summary>
    private async Task DashUntilObstacleAsync(Vector2Int currentPos, Vector2Int direction) {
        while (true) {
            Vector2Int targetPos = currentPos + direction;
            
            // 移動不可なら終了
            if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) {
                return;
            }

            // オブジェクトがあったら終了
            if (TileManager.i.CheckExistObject(targetPos)) {
                return;
            }

            // 移動
            currentPos = targetPos;
            Move(currentPos, targetPos);

            // 移動後に周囲8マスにEnemyがいるかつ、攻撃可能なら終了
            if (ShouldStopForEnemies(currentPos)) {
                return;
            }
            
            // 移動後にジョイントポジションなら終了
            if (TileManager.i.CheckExistJoint(currentPos)) {
                return;
            }
            
            await Task.Delay(50);
        }
    }

    /// <summary>
    /// 周囲に攻撃可能な敵がいるかどうかを判定
    /// </summary>
    private bool ShouldStopForEnemies(Vector2Int currentPos) {
        List<GameObject> surroundingEnemy = TileManager.i.GetSurroundingObjects(currentPos)
            .Where(o => o.GetComponent<Enemy>() != null).ToList();
            
        foreach (var enemy in surroundingEnemy) {
            if (TileManager.i.CheckAttackableTile(currentPos, enemy.GetComponent<Enemy>().objectData.Position.Value)) {
                return true;
            }
        }
        return false;
    }

    // ================================================
    // ==================== Zダッシュ ====================
    // ================================================    

    /// <summary>
    /// Z入力によるダッシュ処理
    /// </summary>
    public async Task ZDash(Vector2 inputVector) {
        this.inputVector = inputVector;

        // 入力値を四捨五入して整数値に変換
        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int direction = new Vector2Int((int)roundX, (int)roundY);
        Vector2Int currentPos = objectData.Position.Value;

        // ルーム内の場合は扇形の範囲内にあるアイテムを探索
        if (TileManager.i.LookupRoomNum(currentPos + direction) != 0) {
            await ZDashInRoomAsync(currentPos, direction);
        } else {
            // 通路の場合、ルームに入るか移動不可になるまでダッシュ
            await ZDashInCorridorAsync(currentPos, direction);
        }
    }

    /// <summary>
    /// ルーム内でのZダッシュ処理
    /// </summary>
    private async Task ZDashInRoomAsync(Vector2Int currentPos, Vector2Int direction) {
        // 扇形の範囲内にあるアイテムを探索
        List<Vector2Int> objectsInFanShape = FindObjectPosInFanShape(currentPos, direction, 5, 60f);

        if (objectsInFanShape.Count > 0) {
            // 最も近いアイテムを選択
            Vector2Int nearestObjectPos = GetNearestObjectPos(currentPos, objectsInFanShape);

            // アイテムの方向を向く
            Vector2Int objectDirection = nearestObjectPos - currentPos;
            playerFaceDirection.SetValue(new Vector2(objectDirection.x, objectDirection.y));
            OnPlayerDirectionChanged.Raise();

            // アイテムまでダッシュ
            await DashToObjectAsync(currentPos, nearestObjectPos);
        }
    }

    /// <summary>
    /// 通路でのZダッシュ処理
    /// </summary>
    private async Task ZDashInCorridorAsync(Vector2Int currentPos, Vector2Int direction) {
        // 一歩進む
        roundX = direction.x;
        roundY = direction.y;
        Move(currentPos, currentPos + direction);

        while (true) {
            // 前方5方向に移動可能なタイルを探索
            List<Vector2Int> facingTiles = DirectionUtils.GetSurroundingFacingTiles(currentPos, 
                DungeonConstants.ToDirection[playerFaceDirection.Value.ToVector2Int()]);
                
            List<Vector2Int> movableTiles = facingTiles
                .Where(tile => TileManager.i.CheckMovableTile(currentPos, tile)).ToList();
                
            if (movableTiles.Count == 0) {
                return;
            }

            // 移動可能なタイルが2つ以上なら終了（部屋に到達した場合）
            if (movableTiles.Count > 1 && TileManager.i.LookupRoomNum(currentPos + direction) != 0) {
                return;
            }
            
            // 移動可能なタイルが1つなら移動
            if (movableTiles.Count == 1) {
                Vector2Int targetPos = movableTiles[0];
                roundX = targetPos.x - currentPos.x;
                roundY = targetPos.y - currentPos.y;
                Move(currentPos, targetPos);
                
                // 移動不可なら終了
                if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) {
                    return;
                }
                
                // currentPosを更新
                currentPos = targetPos;
            }
            
            await Task.Delay(50);
        }
    }

    /// <summary>
    /// 扇形の範囲内にあるアイテム、階段、ジョイントを探索する
    /// </summary>
    private List<Vector2Int> FindObjectPosInFanShape(Vector2Int origin, Vector2Int direction, int maxDistance, float angleDegrees) {
        List<Vector2Int> objectsInRange = new List<Vector2Int>();

        // 探索する最大の角度（ラジアン）
        float maxAngleRadians = angleDegrees * Mathf.Deg2Rad / 2f;

        // 探索範囲内の全ての位置をチェック
        for (int distance = 1; distance <= maxDistance; distance++) {
            // 距離に応じて横幅を広げる（扇形を作る）
            int width = Mathf.CeilToInt(distance * Mathf.Tan(maxAngleRadians));

            // 主方向に対して垂直な方向を計算
            Vector2Int perpendicular = CalculatePerpendicularVector(direction);

            // 斜め入力の場合、追加の探索が必要
            bool isDiagonal = direction.x != 0 && direction.y != 0;

            // 主方向の位置
            Vector2Int mainPos = origin + direction * distance;

            // 主方向の位置をチェック
            CheckPositionForObject(mainPos, objectsInRange);

            // 斜め入力時の追加探索（穴を埋める）
            if (isDiagonal && distance > 0) {
                CheckDiagonalPositions(origin, direction, distance, objectsInRange);
            }

            // 主方向の両側をチェック
            CheckSurroundingPositions(origin, mainPos, perpendicular, width, distance, objectsInRange);
        }

        return objectsInRange;
    }

    /// <summary>
    /// 主方向に対して垂直なベクトルを計算する
    /// </summary>
    private Vector2Int CalculatePerpendicularVector(Vector2Int direction) {
        if (direction.x == 0) {
            return new Vector2Int(1, 0);
        } else if (direction.y == 0) {
            return new Vector2Int(0, 1);
        } else {
            // 垂直ベクトル（x, y）→（-y, x）
            Vector2 perpVector = new Vector2(-direction.y, direction.x).normalized;
            return new Vector2Int(Mathf.RoundToInt(perpVector.x), Mathf.RoundToInt(perpVector.y));
        }
    }

    /// <summary>
    /// 斜め入力時の追加位置をチェックする
    /// </summary>
    private void CheckDiagonalPositions(Vector2Int origin, Vector2Int direction, int distance, List<Vector2Int> objectsInRange) {
        // 水平方向の位置
        Vector2Int horizontalPos = origin + new Vector2Int(direction.x * distance, 0);
        CheckPositionForObject(horizontalPos, objectsInRange);

        // 垂直方向の位置
        Vector2Int verticalPos = origin + new Vector2Int(0, direction.y * distance);
        CheckPositionForObject(verticalPos, objectsInRange);
    }

    /// <summary>
    /// 主方向の周囲の位置をチェックする
    /// </summary>
    private void CheckSurroundingPositions(Vector2Int origin, Vector2Int mainPos, Vector2Int perpendicular, 
                                          int width, int distance, List<Vector2Int> objectsInRange) {
        // 主方向を計算
        Vector2Int mainDirection;
        if (distance > 0) {
            mainDirection = new Vector2Int(
                mainPos.x != origin.x ? (mainPos.x - origin.x) / distance : 0,
                mainPos.y != origin.y ? (mainPos.y - origin.y) / distance : 0
            );
        } else {
            // 距離が0の場合（ありえないが念のため）
            mainDirection = new Vector2Int(0, 0);
        }

        for (int w = 1; w <= width; w++) {
            // 左側
            Vector2Int leftPos = mainPos + perpendicular * w;
            CheckPositionForObject(leftPos, objectsInRange);

            // 右側
            Vector2Int rightPos = mainPos - perpendicular * w;
            CheckPositionForObject(rightPos, objectsInRange);

            // 斜め方向もチェック（より自然な扇形にするため）
            if (distance > 1) {
                for (int d = 1; d < distance; d++) {
                    Vector2Int diagLeftPos = origin + mainDirection * d + perpendicular * w;
                    CheckPositionForObject(diagLeftPos, objectsInRange);

                    Vector2Int diagRightPos = origin + mainDirection * d - perpendicular * w;
                    CheckPositionForObject(diagRightPos, objectsInRange);
                }
            }
        }
    }

    /// <summary>
    /// 指定位置にオブジェクトがあるかチェックする
    /// </summary>
    private void CheckPositionForObject(Vector2Int pos, List<Vector2Int> objects) {
        // 壁の場合はスキップ
        if (TileManager.i.GetMapChipType(pos) == (int)Constants.MapChipType.Wall) {
            return;
        }

        // アイテムがあるかチェック
        Item item = TileManager.i.CheckExistItem(pos);
        if (item != null) {
            objects.Add(item.objectData.Position.Value);
        }

        // 階段があるかチェック
        GameObject stair = TileManager.i.CheckExistStair(pos);
        if (stair != null) {
            objects.Add(stair.GetComponent<ObjectData>().Position.Value);
        }

        // ジョイントがあるかチェック
        if (TileManager.i.CheckExistJoint(pos)) {
            objects.Add(pos);
        }
    }

    /// <summary>
    /// 最も近いオブジェクトの位置を取得する
    /// </summary>
    private Vector2Int GetNearestObjectPos(Vector2Int origin, List<Vector2Int> objects) {
        Vector2Int nearest = new Vector2Int(0, 0);
        float minDistance = float.MaxValue;

        foreach (Vector2Int objectPos in objects) {
            float distance = Vector2.Distance(new Vector2(origin.x, origin.y), new Vector2(objectPos.x, objectPos.y));

            if (distance < minDistance) {
                minDistance = distance;
                nearest = objectPos;
            }
        }

        return nearest;
    }

    /// <summary>
    /// オブジェクトまでダッシュする
    /// </summary>
    private async Task DashToObjectAsync(Vector2Int currentPos, Vector2Int targetPos) {
        AStarPathfinding pathfinding = new AStarPathfinding();
        List<Vector2Int> path = pathfinding.FindPath(currentPos, targetPos, 
            TileManager.i.ExtractAllRoomPositions(objectData.RoomNum.Value));

        Vector2Int movePos = currentPos;

        foreach (var pos in path) {
            Move(movePos, pos);
            await Task.Delay(50);
            movePos = pos;
        }
    }

    // ================================================
    // ================ 向き変更処理 ================
    // ================================================
    
    /// <summary>
    /// 周囲の敵に基づいて自動的に向きを変更する
    /// </summary>
    public void AutoTurn(Vector2Int currentPos) {
        // 周囲8マスを調べる
        List<GameObject> surroundingObjects = TileManager.i.GetSurroundingObjects(currentPos);
        foreach (var obj in surroundingObjects) {
            if (obj.GetComponent<Enemy>() != null) {
                // 敵がいる場合は敵の方向を向く
                playerFaceDirection.SetValue(obj.GetComponent<Enemy>().objectData.Position.Value - currentPos);
                OnPlayerDirectionChanged.Raise();
            }
        }
    }

    /// <summary>
    /// 入力に基づいて手動で向きを変更する
    /// </summary>
    public void ManualTurn(Vector2 inputVector) {
        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);
        playerFaceDirection.SetValue(new Vector2(roundX, roundY));
        OnPlayerDirectionChanged.Raise();
    }

    // ================================================
    // ================ ランダム移動処理 ================
    // ================================================
    
    /// <summary>
    /// ランダムな方向に移動する（混乱状態用）
    /// </summary>
    public bool RandomMove() {
        // 周囲8マスから移動可能なマスを探す
        List<Vector2Int> movableTiles = TileManager.i.GetSurroundingPositions(objectData.Position.Value)
            .Where(tile => TileManager.i.CheckMovableTile(objectData.Position.Value, tile)).ToList();
            
        if(movableTiles.Count == 0) {
            return false;
        }

        // ランダムな移動可能タイルを選択
        int randomIndex = UnityEngine.Random.Range(0, movableTiles.Count);
        Vector2Int randomTile = movableTiles[randomIndex];

        // 移動方向を計算
        roundX = randomTile.x - objectData.Position.Value.x;
        roundY = randomTile.y - objectData.Position.Value.y;

        Vector2Int inputVectorInt = new Vector2Int((int)roundX, (int)roundY);
        Vector2Int currentPos = objectData.Position.Value;
        Vector2Int targetPos = inputVectorInt + currentPos;

        if (isMoving) return false;
        
        // 移動実行
        Move(currentPos, targetPos);

        // 階段があれば選択処理を実行
        CheckAndInteractWithStairs(targetPos);

        return true;
    }
}
