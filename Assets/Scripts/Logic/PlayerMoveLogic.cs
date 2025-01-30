using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

public class PlayerMoveLogic
{    
    private IObjectData objectData;
    private PlayerAnimLogic playerAnimLogic;
    private StateMachine stateMachine;
    private State enemyState;

    //コンストラクタ
    public PlayerMoveLogic(IObjectData objectData, PlayerAnimLogic playerAnimLogic){        
        this.objectData = objectData;
        this.playerAnimLogic = playerAnimLogic;
        stateMachine = GameAssets.i.stateMachine;
        enemyState = GameAssets.i.enemyState;
    }

    Vector2 inputVector;
    float roundX;
    float roundY;
    Vector2 moveOffset = new Vector2(.5f, .5f);
    bool isMoving = false;
    List<Vector2Int> inputs = new List<Vector2Int>();

    public void MoveByInput(Vector2 inputVector){

        this.inputVector = inputVector;

        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int inputVectorInt = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理
        Vector2Int currentPos = objectData.Position;
        Vector2Int targetPos = inputVectorInt + currentPos;

        if(isMoving) return;
        inputs.Add(inputVectorInt);
        DevideInput(currentPos, targetPos);
    }

    async void DevideInput(Vector2Int currentPos, Vector2Int targetPos){
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
    }    

    private void Move(Vector2Int currentPos, Vector2Int targetPos) {
        playerAnimLogic.SetMoveAnimation(new Vector2(inputVector.x, inputVector.y));
        if(!TileManager.i.CheckMovableTile(currentPos, targetPos))return;
        if(isMoving == true) return;
        LockInputWhileMoving();
        Vector2 newPosition = targetPos + moveOffset;
        objectData.Position = newPosition.ToVector2Int();
        EndState();
    }

    async void LockInputWhileMoving(){
        isMoving = true;
        await Task.Delay(150);
        isMoving = false;
    }

    private void EndState(){
        stateMachine.SetState(enemyState);
    }
}
