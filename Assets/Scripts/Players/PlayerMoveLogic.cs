using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
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

            //アイテムを拾う
            if (TileManager.i.CheckExistItem(targetPos) is Item item) {
                currentSelectedObjectSO.Object = item.gameObject;
                // itemSOがnullでないことを確認
                if (item.itemSO != null) {
                    currentItemObject = item.gameObject;
                    OnItemPicked.RaiseEvent(item.itemSO);
                } else {
                    Debug.LogError("Item " + item.name + " has no ItemSO assigned!");
                }
            }

            if (TileManager.i.CheckExistStair(targetPos) != null) {
                TileManager.i.CheckExistStair(targetPos).OnSelected();
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
