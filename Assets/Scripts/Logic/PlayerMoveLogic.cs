using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
public class PlayerMoveLogic
{    
    private IObjectData objectData;
    private PlayerAnimLogic playerAnimLogic;        
    private StateMachine stateMachine;
    private Player player;

    //コンストラクタ
    public PlayerMoveLogic(IObjectData objectData, PlayerAnimLogic playerAnimLogic, Player player){        
        this.objectData = objectData;
        this.playerAnimLogic = playerAnimLogic;
        this.stateMachine = GameAssets.i.stateMachine;
        this.player = player;
    }

    Vector2 inputVector;
    float roundX;
    float roundY;
    Vector2 moveOffset = new Vector2(.5f, .5f);
    bool isMoving = false;
    List<Vector2Int> inputs = new List<Vector2Int>();

    public void MoveByInput(Vector2 inputVector){
        try {
            this.inputVector = inputVector;

        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int inputVectorInt = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理
        Vector2Int currentPos = objectData.Position;
        Vector2Int targetPos = inputVectorInt + currentPos;

            if(isMoving) return;
            inputs.Add(inputVectorInt);
            DevideInput(currentPos, targetPos);
        } catch (Exception e) {
            Debug.LogError($"PlayerMoveLogic MoveByInput failed: {e.Message}");
        }
    }

    async void DevideInput(Vector2Int currentPos, Vector2Int targetPos){
        try {
            //0.03秒待つ
            await Task.Delay(30);

        if(inputs.Count == 1) Move(currentPos, targetPos);

        if(inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.UpRight])){
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.UpRight]);
        } else if(inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.UpLeft])){
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.UpLeft]);
        } else if(inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.DownRight])){
            Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.DownRight]);
        }else if(inputs.Any(i => i == DungeonConstants.ToVector2Int[DungeonConstants.DownLeft])){
                Move(currentPos, currentPos + DungeonConstants.ToVector2Int[DungeonConstants.DownLeft]);
            }
            inputs.Clear();
        } catch (Exception e) {
            Debug.LogError($"PlayerMoveLogic DevideInput failed: {e.Message}");
        }
    }    

    private void Move(Vector2Int currentPos, Vector2Int targetPos) {
        try {
            if (stateMachine.CurrentState != GameAssets.i.playerState){
                Debug.Log("enemyStateで動けません");
                return;
        }
        
        if (player.IsMoving()) {
            Debug.Log("playerが動いています");
            return;
        }

        playerAnimLogic.SetMoveAnimation(new Vector2(inputVector.x, inputVector.y));
        if(!TileManager.i.CheckMovableTile(currentPos, targetPos)) return;

            Vector2 newPosition = targetPos + moveOffset;
            objectData.Position = newPosition.ToVector2Int();
            ActionEventManager.NotifyActionComplete();
        } catch (Exception e) {
            Debug.LogError($"PlayerMoveLogic Move failed: {e.Message}");
        }
    }

    async void LockInputWhileMoving(){
        isMoving = true;
        await Task.Delay(15);        
        isMoving = false;
    }

}
