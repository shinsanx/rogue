using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RandomDungeonWithBluePrint;
using JetBrains.Annotations;
public class TileManager{

    private static TileManager _i;
    public static TileManager i{
        get {
            if(_i == null) {
                _i = new TileManager();
            }
            return _i;
        }
    }

    private Field field;

    //コンストラクタ
    public TileManager() {
        this.field = MessageBus.Instance.Publish<Field>(DungeonConstants.GetCurrentField, this);
        MessageBus.Instance.Subscribe("UpdateFieldInformation", UpdateFieldInformation);
    }

    // 基本的な通行可能判定を行う共通メソッド
    private bool IsBasicallyWalkable(Vector2Int currentPos, Vector2Int targetPos) {
        //floor =>0 wall =>1
        bool canMoveY = field.tileInfo[currentPos.x, targetPos.y].mapChipType == 0;
        bool canMoveX = field.tileInfo[targetPos.x, currentPos.y].mapChipType == 0;
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0;
        return canMoveX && canMoveY && canMove;
    }

    //床が移動可能かどうか判別する
    public bool CheckMovableTile(Vector2Int currentPos, Vector2Int targetPos) {
        bool existCharacter = CharacterManager.i.GetObjectTypeByPosition(targetPos) != null;
        return IsBasicallyWalkable(currentPos, targetPos) && !existCharacter;
    }

    // 攻撃可能なタイルかどうか判別する
    public bool CheckAttackableTile(Vector2Int currentPos, Vector2Int targetPos) {
        return IsBasicallyWalkable(currentPos, targetPos);
    }

    //AstarPathfinding専用。キャラクターがいるかどうかはチェックしない
    public bool CheckWalkableTile(Vector2Int currentPos, Vector2Int targetPos) {
        return IsBasicallyWalkable(currentPos, targetPos);
    }

    //そのポジションに他オブジェクトがないかチェックする
    public bool CheckTileStandable(Vector2Int targetPos) {
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0;
        bool existCharacter = CharacterManager.i.GetObjectTypeByPosition(targetPos) != null;
        return canMove && !existCharacter;
    }

    //aisleとnothingのみ。MapChipTypeとは別
    public int GetTileType(Vector2Int vector) {
        return field.tileInfo[vector.x, vector.y].tileType;
    }

    //TileTypeとは別。
    public int GetMapChipType(Vector2Int vector) {
        return field.tileInfo[vector.x, vector.y].mapChipType;
    }

    //マップが変わった時に使用。Field情報を更新する
    public void UpdateFieldInformation(object fieldData) {
        this.field = (Field)fieldData;
    }


    public int LookupRoomNum(Vector2Int selfPos) {
        //自身のpositionから自身が存在するRoomを特定。通路の場合はゼロ。
        for (int i = 0; i < field.Rooms.Count; i++) {
            for (int j = 0; j < field.Rooms[i].Positions.Count; j++) {
                if (selfPos == field.Rooms[i].Positions[j]) {
                    return field.Rooms[i].roomNum;
                }
            }
        }
        return 0; //引っ掛からなかった場合0を返す
    }


    //Room内のジョイントポジションを検索する
    public List<Vector2Int> ExtractJointPosInRoom(Vector2Int selfPos) {
        int roomNum = LookupRoomNum(selfPos);        
        Room room = field.Rooms.FirstOrDefault(r => r.roomNum == roomNum);        
        if (room == null) return null;        
        if (room.jointPositions == null) return null;
        return room.jointPositions;
    }


    //部屋の中の視野検索用
    public List<Vector2Int> ExtractAllRoomPositions(int roomNum) {
        // 無効なルーム番号はnullを返す
        if (roomNum == 0) return null;

        // 指定されたroomNumのRoomオブジェクトを取得
        Room room = field.Rooms.FirstOrDefault(r => r.roomNum == roomNum);
        if (room == null) return null;

        // room内の全ポジションを取得
        List<Vector2Int> allRoomPositions = new List<Vector2Int>(room.Positions);

        // 接続点（joint）を取得し、接続点から1マス隣の通路を探索
        List<Vector2Int> joints = ExtractJointPosInRoom(room.Positions[0]);
        if (joints.Count == 0) return allRoomPositions; // 接続点がなければそのまま返す

        foreach (Vector2Int joint in joints) {
            // 隣接する通路を取得
            List<Vector2Int> neighboringAisles = GetNeighborBranchPositions(joint);

            // 通路がallRoomPositionsに含まれていなければ追加
            foreach (Vector2Int aisle in neighboringAisles) {
                if (!allRoomPositions.Contains(aisle)) {
                    allRoomPositions.Add(aisle);
                }
            }
        }
        return allRoomPositions;
    }

    


    // 周囲１マスにある通路タイルを検出する
    public List<Vector2Int> GetNeighborBranchPositions(Vector2Int selfPos) {
        List<Vector2Int> neighborBranches = new List<Vector2Int>();
        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int neighborPos = selfPos + DungeonConstants.ToVector2Int[direction];

            // タイルタイプが通路の場合にのみリストに追加
            if (GetTileType(neighborPos) == (int)Constants.TileType.Aisle) {
                neighborBranches.Add(neighborPos);
            }
        }
        return neighborBranches;
    }

    // 周囲１マスのポジションを取得
    public List<Vector2Int> GetSurroundingPositions(Vector2Int selfPos) {
        List<Vector2Int> surroundingPositions = new List<Vector2Int>();
        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int neighborPos = selfPos + DungeonConstants.ToVector2Int[direction];
            
                surroundingPositions.Add(neighborPos);
            
        }
        return surroundingPositions;
    }

    // 指定のポジションが隣接しているか確認する
    public bool IsAdjacentTo(Vector2Int selfPos, Vector2Int targetPos) {
        return GetSurroundingPositions(selfPos).Contains(targetPos);
    }

    // Character自動配置用
    // ランダムでRoomを選択して、そのRoom内のランダムなポジションを返す
    public Vector2Int GetRandomPosition(){
        // Fieldの中からランダムでRoomを選択
        int roomNum = Random.Range(1, field.Rooms.Count + 1);
        return GetRandomRoomPositions(roomNum);
    }

    // 指定されたRoomの中からランダムなポジションを返す
    private Vector2Int GetRandomRoomPositions(int roomNum){
        List<Vector2Int> roomPositions = ExtractAllRoomPositions(roomNum);
        Vector2Int randomPosition = roomPositions[Random.Range(0, roomPositions.Count)];
        return randomPosition;
    }


}
