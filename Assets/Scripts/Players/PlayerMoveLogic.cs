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
    private PlayerInventory playerInventory;
    // ================================================
    // ============= イベントチャンネル =============
    // ================================================
    private GameEvent OnPlayerStateComplete;
    public GameEvent OnPlayerDirectionChanged;

    //コンストラクタ
    public PlayerMoveLogic(Player player, GameEvent OnPlayerStateComplete, GameEvent OnPlayerDirectionChanged, Vector2Variable playerFaceDirection, PlayerInventory playerInventory) {
        this.player = player;        
        this.stateMachine = GameAssets.i.stateMachine;
        this.OnPlayerStateComplete = OnPlayerStateComplete;
        this.objectData = player.playerObjectData;
        this.OnPlayerDirectionChanged = OnPlayerDirectionChanged;
        this.playerFaceDirection = playerFaceDirection;
        this.playerInventory = playerInventory;
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
                return;
            }

            if (player.IsMoving()) {
                Debug.Log("playerが動いています");
                return;
            }

            //アニメーションを再生する
            playerFaceDirection.SetValue(new Vector2(roundX, roundY));
            OnPlayerDirectionChanged.Raise();
            if (!TileManager.i.CheckMovableTile(currentPos, targetPos)) return;

            //アイテムを拾う
            if(TileManager.i.CheckExistItem(targetPos) is Item item) {
                item.GetComponent<Item>().OnPicked(playerInventory);
            }

            Vector2 newPosition = targetPos + moveOffset;
            objectData.SetPosition(newPosition.ToVector2Int());
            OnPlayerStateComplete.Raise();
    }

    // async void LockInputWhileMoving() {
    //     isMoving = true;
    //     await Task.Delay(15);
    //     isMoving = false;
    // }

}
