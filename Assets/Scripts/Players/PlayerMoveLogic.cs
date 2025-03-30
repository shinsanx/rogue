using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
using RandomDungeonWithBluePrint;

public class PlayerMoveLogic {
    private IObjectData objectData;
    private StateMachine stateMachine;
    private Player player;
    private Vector2Variable playerFaceDirection;
    private GameObject currentItemObject;
    private CurrentSelectedObjectSO currentSelectedObjectSO;
    private BoolVariable fixDiagonalInput;
    // ================================================
    // ============= イベントチャンネル =============
    // ================================================
    private GameEvent OnPlayerStateComplete;
    public GameEvent OnPlayerDirectionChanged;
    public ItemEventChannelSO OnItemPicked;

    //コンストラクタ
    public PlayerMoveLogic(
        Player player,
        GameEvent OnPlayerStateComplete,
        GameEvent OnPlayerDirectionChanged,
        Vector2Variable playerFaceDirection,
        ItemEventChannelSO OnItemPicked,
        CurrentSelectedObjectSO currentSelectedObjectSO,
        BoolVariable fixDiagonalInput) {//引数ここまで

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

    Vector2 inputVector;
    float roundX;
    float roundY;
    Vector2 moveOffset = new Vector2(.5f, .5f);
    bool isMoving = false;
    List<Vector2Int> inputs = new List<Vector2Int>();

    public void MoveByInput(Vector2 inputVector) {
        try {
            this.inputVector = inputVector;

            roundX = Mathf.Round(inputVector.x);
            roundY = Mathf.Round(inputVector.y);

            Vector2Int inputVectorInt = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理
            Vector2Int currentPos = objectData.Position.Value;
            Vector2Int targetPos = inputVectorInt + currentPos;

            if (isMoving) return;
            inputs.Add(inputVectorInt);
            DevideInput(currentPos, targetPos);            

            if (TileManager.i.CheckExistStair(targetPos) != null) {
                TileManager.i.CheckExistStair(targetPos).GetComponent<IMenuActionAdapter>().OnSelected();
            }

        } catch (Exception e) {
            Debug.LogError($"PlayerMoveLogic MoveByInput failed: {e.Message}");
        }
    }

    async void DevideInput(Vector2Int currentPos, Vector2Int targetPos) {

        //0.03秒待つ
        await Task.Delay(30);

        if (inputs.Count == 1) Move(currentPos, targetPos);

        if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.UpRight])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.UpRight]);
        } else if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.UpLeft])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.UpLeft]);
        } else if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.DownRight])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.DownRight]);
        } else if (inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.DownLeft])) {
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.DownLeft]);
        }
        inputs.Clear();

    }

    private void Move(Vector2Int currentPos, Vector2Int targetPos) {

        if (stateMachine.CurrentState != GameAssets.i.playerState) {
            Debug.Log("playerのターンではありません");
            return;
        }

        if (player.IsMoving()) {
            Debug.Log("playerが動いています");
            return;
        }

        //アニメーションを再生する
        playerFaceDirection.SetValue(new Vector2(roundX, roundY));
        OnPlayerDirectionChanged.Raise();

        // 斜め移動を固定する
        if (fixDiagonalInput.Value) {
            if (roundX == 0 || roundY == 0) return;
        }

        if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) return;

        //アイテムを拾う
        if (TileManager.i.CheckExistItem(targetPos) is Item item) {
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



        Vector2 newPosition = targetPos + moveOffset;
        objectData.SetPosition(newPosition.ToVector2Int());
        OnPlayerStateComplete.Raise();
    }

    //アイテムを拾った時のアイテム側の処理
    //Playerから呼ばれる。SuccessItemPicked
    public void HandleItemPicked(bool success) {
        if (success) {
            Item item = currentItemObject.GetComponent<Item>();
            if (item != null) {
                currentItemObject.GetComponent<Item>().OnPicked();
            }
        } else {
            Debug.Log("アイテムを拾えませんでした。");
        }
    }

    public async void DashByInput(Vector2 inputVector) {

        this.inputVector = inputVector;

        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int direction = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理
        Vector2Int currentPos = objectData.Position.Value;

        //アイテムに乗る処理
        if (TileManager.i.CheckExistItem(currentPos + direction) is Item item) {
            item.OnGetOnItem();
            Move(currentPos, currentPos + direction);
            return;
        }

        // オブジェクトがあるまで移動する
        while (true) {
            Vector2Int targetPos = currentPos + direction;
            //移動不可なら終了
            if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) {
                return;
            }

            //オブジェクトがあったら終了
            if (TileManager.i.CheckExistObject(targetPos)) {
                return;
            }

            //移動
            currentPos = targetPos;
            Move(currentPos, targetPos);

            //移動後に周囲8マスにEnemyがいるかつ、攻撃可能なら終了
            List<GameObject> surroundingEnemy = TileManager.i.GetSurroundingObjects(currentPos).Where(o => o.GetComponent<Enemy>() != null).ToList();
            foreach (var enemy in surroundingEnemy) {
                if (TileManager.i.CheckAttackableTile(currentPos, enemy.GetComponent<Enemy>().objectData.Position.Value)) {
                    return;
                }
            }
            //移動後にジョイントポジションなら終了         
            if (TileManager.i.CheckExistJoint(currentPos)) {
                return;
            }
            await Task.Delay(50);
        }

    }

    // ================================================
    // ==================== Zダッシュ ====================
    // ================================================    

    public async Task ZDash(Vector2 inputVector) {
        this.inputVector = inputVector;

        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int direction = new Vector2Int((int)roundX, (int)roundY); // 四捨五入処理
        Vector2Int currentPos = objectData.Position.Value;

        // ルーム内の場合は扇形の範囲内にあるアイテムを探索
        if (TileManager.i.LookupRoomNum(currentPos + direction) != 0) {

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
                DashToObject(currentPos, nearestObjectPos);
            }
        } else {
            // 通路の場合、ルームに入るか移動不可になるまでダッシュ            

            // 一歩進む
            roundX = direction.x;
            roundY = direction.y;
            Move(currentPos, currentPos + direction);            

            while (true) {
                // 前方5方向に移動可能なタイルを探索
                List<Vector2Int> facingTiles = DirectionUtils.GetSurroundingFacingTiles(currentPos, DungeonConstants.ToDirection[playerFaceDirection.Value.ToVector2Int()]);
                List<Vector2Int> movableTiles = facingTiles.Where(tile => TileManager.i.CheckMovableTile(currentPos, tile)).ToList();                                                        

                // 移動可能なタイルが2つ以上なら終了
                if (movableTiles.Count > 1 && TileManager.i.LookupRoomNum(currentPos + direction) != 0) {                    
                    return;
                }
                // 移動可能なタイルが1つなら移動
                if (movableTiles.Count == 1) {                    
                    roundX = movableTiles[0].x - currentPos.x;                    
                    roundY = movableTiles[0].y - currentPos.y;                    
                    Move(currentPos, movableTiles[0]);
                }
                // 移動不可なら終了
                Vector2Int targetPos = movableTiles[0];                
                if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) {                    
                    return;
                }
                // currentPosを更新
                currentPos = targetPos;
                await Task.Delay(50);
            }
        }
    }

    // 扇形の範囲内にあるアイテム、階段、ジョイントを探索するメソッド
    private List<Vector2Int> FindObjectPosInFanShape(Vector2Int origin, Vector2Int direction, int maxDistance, float angleDegrees) {
        List<Vector2Int> objectsInRange = new List<Vector2Int>();


        // 探索する最大の角度（ラジアン）
        float maxAngleRadians = angleDegrees * Mathf.Deg2Rad / 2f;

        // 探索範囲内の全ての位置をチェック
        for (int distance = 1; distance <= maxDistance; distance++) {
            // 距離に応じて横幅を広げる（扇形を作る）
            int width = Mathf.CeilToInt(distance * Mathf.Tan(maxAngleRadians));

            // 主方向に対して垂直な方向を計算
            Vector2Int perpendicular;
            if (direction.x == 0) {
                perpendicular = new Vector2Int(1, 0);
            } else if (direction.y == 0) {
                perpendicular = new Vector2Int(0, 1);
            } else {
                // 垂直ベクトル（x, y）→（-y, x）
                Vector2 perpVector = new Vector2(-direction.y, direction.x).normalized;
                perpendicular = new Vector2Int(Mathf.RoundToInt(perpVector.x), Mathf.RoundToInt(perpVector.y));
            }

            // 斜め入力の場合、追加の探索が必要
            bool isDiagonal = direction.x != 0 && direction.y != 0;

            // 主方向の位置
            Vector2Int mainPos = origin + direction * distance;

            // 主方向の位置をチェック
            CheckPositionForObject(mainPos, objectsInRange);

            // 斜め入力時の追加探索（穴を埋める）
            if (isDiagonal && distance > 0) {
                // 水平方向の位置
                Vector2Int horizontalPos = origin + new Vector2Int(direction.x * distance, 0);
                CheckPositionForObject(horizontalPos, objectsInRange);

                // 垂直方向の位置
                Vector2Int verticalPos = origin + new Vector2Int(0, direction.y * distance);
                CheckPositionForObject(verticalPos, objectsInRange);
            }

            // 主方向の両側をチェック
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
                        Vector2Int diagLeftPos = origin + direction * d + perpendicular * w;
                        CheckPositionForObject(diagLeftPos, objectsInRange);

                        Vector2Int diagRightPos = origin + direction * d - perpendicular * w;
                        CheckPositionForObject(diagRightPos, objectsInRange);
                    }
                }
            }
        }

        return objectsInRange;
    }

    // 指定位置にアイテムがあるかチェックするヘルパーメソッド
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
        if (TileManager.i.CheckExistStair(pos) != null) {
            objects.Add(TileManager.i.CheckExistStair(pos).GetComponent<ObjectData>().Position.Value);
        }

        // ジョイントがあるかチェック
        if (TileManager.i.CheckExistJoint(pos)) {
            objects.Add(pos);
        }


    }

    // 最も近いアイテムを取得するメソッド
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

    // アイテムまでダッシュするメソッド
    private async void DashToObject(Vector2Int currentPos, Vector2Int targetPos) {

        AStarPathfinding pathfinding = new AStarPathfinding();
        List<Vector2Int> path = pathfinding.FindPath(currentPos, targetPos, TileManager.i.ExtractAllRoomPositions(objectData.RoomNum.Value));

        Vector2Int movePos = currentPos;

        foreach (var pos in path) {
            Move(movePos, pos);                        
            await Task.Delay(50);
            movePos = pos;
        }

    }


    // async void LockInputWhileMoving() {
    //     isMoving = true;
    //     await Task.Delay(15);
    //     isMoving = false;
    // }

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

    public void ManualTurn(Vector2 inputVector) {
        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);
        playerFaceDirection.SetValue(new Vector2(roundX, roundY));
        OnPlayerDirectionChanged.Raise();
    }

}
