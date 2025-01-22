using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RandomDungeonWithBluePrint;
using JetBrains.Annotations;

public class TileLogic {
    private Field field;

    //コンストラクタ
    public TileLogic() {
        this.field = MessageBus.Instance.PublishDelegate<Field>(DungeonConstants.GetCurrentField, this);
        MessageBus.Instance.Subscribe("UpdateFieldInformation", UpdateFieldInformation);
    }

    //床が移動可能かどうか判別する
    public bool CheckMovableTile(Vector2Int currentPos, Vector2Int targetPos) {
        //floor =>0 wall =>1
        if(field == null)Debug.Log(field + " fieldが空です");
        bool canMoveY = field.tileInfo[currentPos.x, targetPos.y].mapChipType == 0 ? true : false;
        bool canMoveX = field.tileInfo[targetPos.x, currentPos.y].mapChipType == 0 ? true : false;
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0 ? true : false;
        bool existEnemy = field.tileInfo[targetPos.x, targetPos.y].objType == (int)DungeonConstants.ObjTypelnTile.Enemy ? true : false;
        if (canMoveX && canMoveY && canMove && !existEnemy) {
            return true;
        }
        return false;
    }

    //AstarPathfinding専用。
    public bool CheckWalkableTile(Vector2Int currentPos, Vector2Int targetPos) {
        //floor =>0 wall =>1
        bool canMoveY = field.tileInfo[currentPos.x, targetPos.y].mapChipType == 0 ? true : false;
        bool canMoveX = field.tileInfo[targetPos.x, currentPos.y].mapChipType == 0 ? true : false;
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0 ? true : false;
        //bool existEnemy = field.tileInfo[targetPos.x, targetPos.y].objType == (int)DungeonConstants.ObjTypelnTile.Enemy ? true:false;
        if (canMoveX && canMoveY && canMove) {
            return true;
        }
        return false;
    }

    //そのポジションに他オブジェクトがないか
    public bool CheckTileStandable(Vector2Int targetPos) {
        bool canMove = field.tileInfo[targetPos.x, targetPos.y].mapChipType == 0 ? true : false;
        bool existEnemy = field.tileInfo[targetPos.x, targetPos.y].objType == (int)DungeonConstants.ObjTypelnTile.Enemy ? true : false;

        if (canMove && !existEnemy) {
            return true;
        }
        return false;
    }

    //tile番号からpositionを返す
    public Vector2 GetTilePosition(Vector2Int targetPos) {
        Vector2 tilePos = field.tileInfo[targetPos.x, targetPos.y].position;
        return tilePos;
    }

    //tile番号から存在するオブジェクトのタイプを返す
    //objTypeはそれぞれのオブジェクトのIPositionAdapterのCharacterTypeからDungeonEventLogicが割り振ってる
    public int GetObjectTypeOnTile(Vector2Int vector) {
        return field.tileInfo[vector.x, vector.y].objType;
    }

    //aisleとnothingのみ。MapChipTypeとは別
    public int GetTileType(Vector2Int vector) {
        return field.tileInfo[vector.x, vector.y].tileType;
    }

    //TileTypeとは別。
    public int GetMapChipType(Vector2Int vector) {
        return field.tileInfo[vector.x, vector.y].mapChipType;
    }

    //vector2IntからTileに存在するオブジェクトそのものを抜き出す
    public GameObject GetObjectOnTile(object data) {
        Vector2Int vector = (Vector2Int)data;
        return field.tileInfo[vector.x, vector.y].objectOnTile;
    }

    //マップが変わった時に使用。Field情報を更新する
    public void UpdateFieldInformation(object fieldData) {
        this.field = (Field)fieldData;
    }

    //タイルにオブジェクト、オブジェクトタイプを配備する
    public void SetObjectToTile(List<Transform> objectsTransform) {
        ResetObjectOnTile(); //オブジェクト情報をリセット
        field.tileInfo.ResetObjType(); //オブジェクトタイプ情報をリセット
        Debug.Log("タイル情報がリセットされました。");

        foreach (Transform go in objectsTransform) {
            if (!go.TryGetComponent(out IPositionAdapter positionAdapter)) continue;
            Vector2Int objectPos = positionAdapter.Position;

            int x = objectPos.x;
            int y = objectPos.y;

            field.tileInfo[x, y].objType = positionAdapter.CharacterType;
            field.tileInfo[x, y].objectOnTile = go.gameObject;
        }
    }

    //それぞれのRoomにオブジェクト情報を配備する
    public void DistributeGameObjectToRooms(List<Transform> objectsTransform) {
        //ObjectたちをClear
        foreach (var room in field.Rooms) {
            room.gameObjects.Clear();
        }

        foreach (var oTransform in objectsTransform) {
            if (oTransform.gameObject.GetComponent<IPositionAdapter>() != null) {
                //IPositionAdapterがある場合のみ。親のPlayersとかも取得してるから。
                Vector2Int vecInt = oTransform.gameObject.GetComponent<IPositionAdapter>().Position;
                int roomNum = LookupRoomNum(vecInt);
                Room room = field.Rooms.FirstOrDefault(r => r.roomNum == roomNum);
                if (room != null) {
                    room.gameObjects.Add(oTransform.gameObject);
                }
            }
        }
    }

    //ObjectOnTileをリセット
    public void ResetObjectOnTile() {
        int x = field.tileInfo.mapSize.x;
        int y = field.tileInfo.mapSize.y;
        for (int i = 0; i < y; i++) {
            for (int j = 0; j < x; j++) {
                field.tileInfo[j, i].objectOnTile = null;
            }
        }

    }

    public List<GameObject> ExtractObjInSameRoom(object selfPos_Vector2Int) {
        Vector2Int selfPos = (Vector2Int)selfPos_Vector2Int;

        int roomNum = LookupRoomNum(selfPos);
        Room room = field.Rooms.FirstOrDefault(r => r.roomNum == roomNum);
        if (room == null) return null;
        if (room.gameObjects != null) return room.gameObjects;
        return null;
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

    private Vector2Int LookupObjectPos(object tagName_str) {
        string tagName = (string)tagName_str;
        int x = field.tileInfo.mapSize.x;
        int y = field.tileInfo.mapSize.y;
        for (int i = 0; i < y; i++) {
            for (int j = 0; j < x; j++) {
                GameObject obj = field.tileInfo[j, i].objectOnTile;
                if (obj != null && obj.CompareTag(tagName)) {
                    return obj.GetComponent<IPositionAdapter>().Position;
                }
            }
        }
        return Vector2Int.zero;
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
        if (roomNum <= 0) return null;

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
