using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class PlayerMoveHandler {    
    private BoolVariable playerCanMove;
    private IObjectData objectData;
    private StateMachine stateMachine;
    private GameEvent onPlayerStateComplete;
    private GameEvent onPlayerDirectionChanged;
    private Vector2Variable playerFaceDirection;
    private BoolVariable fixDiagonalInput;
    private TileManager tileManager;

    private float roundX;
    private float roundY;
    private Vector2 inputVector;    
    //private List<Vector2Int> inputs = new();

    public PlayerMoveHandler(
        BoolVariable playerCanMove,
        ObjectData playerObjectData,
        GameEvent onPlayerStateComplete,
        GameEvent onPlayerDirectionChanged,
        Vector2Variable playerFaceDirection,
        BoolVariable fixDiagonalInput,
        TileManager tileManager)
    {
        //this.player = player;
        this.playerCanMove = playerCanMove;

        this.objectData = playerObjectData;
        this.stateMachine = GameAssets.i.stateMachine;
        this.onPlayerStateComplete = onPlayerStateComplete;
        this.onPlayerDirectionChanged = onPlayerDirectionChanged;
        this.playerFaceDirection = playerFaceDirection;
        this.fixDiagonalInput = fixDiagonalInput;
        this.tileManager = tileManager;
    }

    public void MoveByInput(Vector2 inputVector) {
        this.inputVector = inputVector;
        roundX = Mathf.Round(inputVector.x);
        roundY = Mathf.Round(inputVector.y);

        Vector2Int inputVectorInt = new Vector2Int((int)roundX, (int)roundY);
        Vector2Int currentPos = objectData.Position.Value;
        Vector2Int targetPos = inputVectorInt + currentPos;
        
        Move(currentPos, targetPos);
    }

    

    public void Move(Vector2Int currentPos, Vector2Int targetPos) {       

        // 向き更新
        playerFaceDirection.SetValue(new Vector2(roundX, roundY));
        onPlayerDirectionChanged.Raise();

        if (fixDiagonalInput.Value){
             if (roundX == 0 || roundY == 0){
                playerCanMove.Value = true;
                 return;
                 }
        }

        if (!tileManager.CheckMovableTile(currentPos, targetPos)){
            playerCanMove.Value = true;
            return;
        } 

        Vector2 newPosition = targetPos + new Vector2(0.5f, 0.5f);
        objectData.SetPosition(newPosition.ToVector2Int());

        onPlayerStateComplete.Raise();
    }

    public bool RandomMove() {
        List<Vector2Int> movableTiles = tileManager.GetSurroundingPositions(objectData.Position.Value)
            .Where(tile => tileManager.CheckMovableTile(objectData.Position.Value, tile)).ToList();

        if (movableTiles.Count == 0) return false;

        Vector2Int randomTile = movableTiles[Random.Range(0, movableTiles.Count)];
        Vector2Int currentPos = objectData.Position.Value;

        roundX = randomTile.x - currentPos.x;
        roundY = randomTile.y - currentPos.y;

        Move(currentPos, randomTile);
        return true;
    }
}