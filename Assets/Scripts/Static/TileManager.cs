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
        this.field = MessageBus.Instance.PublishDelegate<Field>(DungeonConstants.GetCurrentField, this);
        MessageBus.Instance.Subscribe("UpdateFieldInformation", UpdateFieldInformation);
    }

    //床が移動可能かどうか判別する
    public bool CheckMovableTile(Vector2Int currentPos, Vector2Int targetPos) {
        //floor =>0 wall =>1
        bool canMoveY = field.tileInfo[currentPos.x, targetPos.y].mapChipType == 0;
        bool canMoveX = field.tileInfo[targetPos.x, currentPos.y].mapChipType == 0;
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0;
        bool existEnemy = CharacterManager.i.GetObjectTypeByPosition(targetPos) != null;

        return canMoveX && canMoveY && canMove && !existEnemy;
    }

    //AstarPathfinding専用。
    public bool CheckWalkableTile(Vector2Int currentPos, Vector2Int targetPos) {
        bool canMoveY = field.tileInfo[currentPos.x, targetPos.y].mapChipType == 0;
        bool canMoveX = field.tileInfo[targetPos.x, currentPos.y].mapChipType == 0;
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0;
        return canMoveX && canMoveY && canMove;
    }

    //そのポジションに他オブジェクトがないかチェックする
    public bool CheckTileStandable(Vector2Int targetPos) {
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0;
        bool existEnemy = CharacterManager.i.GetObjectTypeByPosition(targetPos) != null;

        return canMove && !existEnemy;
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
        Debug.Log($"Room {roomNum} has {allRoomPositions.Count} positions initially.");

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

        Debug.Log($"Room {roomNum} final positions count: {allRoomPositions.Count}");
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


}
