using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomDungeonWithBluePrint;
using System.Linq;

public class TileManager : MonoBehaviour {

    private static TileManager _i;
    public static TileManager i {
        get {
            if (_i == null) {
                Debug.Log("TileManagerが見つかりません");
            }
            return _i;
        }
    }

    //全オブジェクトのセット
    [SerializeField] ObjectDataRuntimeSet objectDataSet;
    [SerializeField] TileSet tileSet;

    //頻繁にroomNumを取得するためのキャッシュ
    //private Dictionary<Vector2Int, int> roomNumCache = new Dictionary<Vector2Int, int>();


    // 基本的な通行可能判定を行う共通メソッド
    private bool IsBasicallyWalkable(Vector2Int currentPos, Vector2Int targetPos) {
        //floor =>0 wall =>1
        Vector2Int direction = targetPos - currentPos;
        bool canMoveY = tileSet.GetMapChipTypeByPosition(currentPos + direction.y * Vector2Int.up) == 0;        
        bool canMoveX = tileSet.GetMapChipTypeByPosition(currentPos + direction.x * Vector2Int.right) == 0;        
        bool canMove = tileSet.GetMapChipTypeByPosition(targetPos) == 0;         
        return canMoveX && canMoveY && canMove;
    }

    //床が移動可能かどうか判別する
    public bool CheckMovableTile(Vector2Int currentPos, Vector2Int targetPos) {
        bool existEnemy = objectDataSet.GetObjectTypeByPosition(targetPos) == "Enemy";
        return IsBasicallyWalkable(currentPos, targetPos) && !existEnemy;
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
        bool canMove = tileSet.GetMapChipTypeByPosition(targetPos) == 0;
        bool existEnemy = objectDataSet.GetObjectTypeByPosition(targetPos) == "Enemy";
        return canMove && !existEnemy;
    }

    public Item CheckExistItem(Vector2Int targetPos) {
        if (objectDataSet.GetObjectTypeByPosition(targetPos) == "Item") {
            return objectDataSet.GetObjectByPosition(targetPos).GetComponent<Item>();
        }
        return null;
    }

    public bool CheckExistObject(Vector2Int targetPos) {
        if (objectDataSet.GetObjectTypeByPosition(targetPos) != null) {
            return true;
        }
        return false;
    }

    public IMenuActionAdapter CheckExistStair(Vector2Int targetPos) {
        if (objectDataSet.GetObjectTypeByPosition(targetPos) == "Stair") {
            return objectDataSet.GetObjectByPosition(targetPos).GetComponent<IMenuActionAdapter>();
        }
        return null;
    }

    //aisleとnothingのみ。MapChipTypeとは別
    public int GetTileType(Vector2Int vector) {
        return tileSet.GetTileTypeByPosition(vector);
    }

    //TileTypeとは別。
    public int GetMapChipType(Vector2Int vector) {
        return tileSet.GetMapChipTypeByPosition(vector);
    }

    //自身のpositionから自身が存在するRoomを特定。通路の場合はゼロ。
    public int LookupRoomNum(Vector2Int selfPos) {
        // if (roomNumCache.TryGetValue(selfPos, out var cachedRoomNum)) {
        //     return cachedRoomNum;
        // }

        // TileInfoのメソッドを使用してroomNumを取得
        int roomNumber = tileSet.GetRoomNumByPosition(selfPos);
        //roomNumCache[selfPos] = roomNumber;
        return roomNumber;
    }

    //Room内のジョイントポジションを検索する
    public List<Vector2Int> ExtractJointPosInRoom(Vector2Int selfPos) {
        int roomNum = LookupRoomNum(selfPos);
        Room room = tileSet.GetRoomByNum(roomNum);
        if (room == null || room.jointPositions == null) return null;

        return room.jointPositions;
    }

    public List<Vector2Int> ExtractJointPosByRoomNum(int roomNum) {
        Room room = tileSet.GetRoomByNum(roomNum);
        if (room == null || room.jointPositions == null){            
            return null;
        }        

        return room.jointPositions;
    }

    public bool CheckExistJoint(Vector2Int targetPos) {
        int roomNum = LookupRoomNum(targetPos);
        List<Vector2Int> jointPositions = ExtractJointPosByRoomNum(roomNum);
        if (jointPositions == null || jointPositions.Count == 0) return false;
        return jointPositions.Contains(targetPos);
    }

    public bool CheckExitsAisle(Vector2Int targetPos) {
        if (GetTileType(targetPos) == (int)Constants.TileType.Aisle) {
            return true;
        }
        return false;
    }

    //部屋の中の視野検索用
    public List<Vector2Int> ExtractAllRoomPositions(int roomNum) {
        // 無効なルーム番号はnullを返す
        if (roomNum == 0) return null;

        // TileInfoの辞書を使用してタイルを取得
        List<Vector2Int> roomPositions = tileSet.GetTilePositionsByRoomNum(roomNum);
        if (roomPositions == null || roomPositions.Count == 0) return null;

        // room内の全ポジションを取得
        List<Vector2Int> allRoomPositions = roomPositions;

        // 接続点（joint）を取得し、接続点から1マス隣の通路を探索
        List<Vector2Int> joints = ExtractJointPosInRoom(allRoomPositions.First());
        if (joints == null || joints.Count == 0) return allRoomPositions; // 接続点がなければそのまま返す

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

    //周囲１マスにある通路タイルを検出する
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

    //周囲１マスのポジションを取得
    public List<Vector2Int> GetSurroundingPositions(Vector2Int selfPos) {
        List<Vector2Int> surroundingPositions = new List<Vector2Int>();
        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int neighborPos = selfPos + DungeonConstants.ToVector2Int[direction];
            surroundingPositions.Add(neighborPos);
        }
        return surroundingPositions;
    }

    //指定のポジションが隣接しているか確認する
    public bool IsAdjacentTo(Vector2Int selfPos, Vector2Int targetPos) {
        return GetSurroundingPositions(selfPos).Contains(targetPos);
    }

    //周囲8マスのオブジェクトを取得する
    public List<GameObject> GetSurroundingObjects(Vector2Int selfPos) {
        List<Vector2Int> surroundingPositions = GetSurroundingPositions(selfPos);
        List<GameObject> surroundingObjects = new List<GameObject>();
        foreach (var position in surroundingPositions) {
            if (objectDataSet.GetObjectTypeByPosition(position) != null) {
                surroundingObjects.Add(objectDataSet.GetObjectByPosition(position));
            }
        }
        return surroundingObjects;
    }

    //Character自動配置用
    //ランダムでRoomを選択して、そのRoom内のランダムなポジションを返す
    public Vector2Int GetRandomPosition() {
        // Fieldの中からランダムでRoomを選択
        if (tileSet.rooms.Count == 0) return Vector2Int.zero;

        int roomNum = Random.Range(1, tileSet.rooms.Count + 1);
        return GetRandomRoomPositions(roomNum);

    }

    //指定されたRoomの中からランダムなポジションを返す
    private Vector2Int GetRandomRoomPositions(int roomNum) {
        List<Vector2Int> roomPositions = tileSet.GetTilePositionsByRoomNum(roomNum);

        if (roomPositions == null || roomPositions.Count == 0) {
            Debug.LogError("roomPositions is null or empty");
            return Vector2Int.zero;
        }

        Vector2Int randomPosition = roomPositions[Random.Range(0, roomPositions.Count)];
        return randomPosition;
    }

    //前方nマスまでのキャラクターを取得する
    public Vector2Int GetCharactersInFront(Vector2Int selfPos, Vector2Int direction, int distance) {
        Vector2Int objectPos2 = Vector2Int.zero;
        for (int i = 1; i <= distance; i++) {
            Vector2Int objectPos = selfPos + direction * i;
            //壁だった場合は一つ前のポジションを返す            
            if (GetMapChipType(objectPos) == (int)Constants.MapChipType.Wall) {
                Debug.Log(objectPos + "に壁があります");
                return objectPos;
            }
            //キャラクターがいた場合はそのポジションを返す
            if (objectDataSet.GetObjectTypeByPosition(objectPos) != null) {
                string objectType = objectDataSet.GetObjectTypeByPosition(objectPos);
                //アイテムの場合は処理しない
                if (objectType == "Item") {
                    continue;
                }
                Debug.Log(objectPos + "に" + objectType + "がいます");
                return objectPos;
            }
            objectPos2 = selfPos + direction * i;
        }
        Debug.Log("前方にキャラクターがいません");
        return objectPos2;
    }

    //特定のポジションのオブジェクトを取得する
    public GameObject GetObjectByPosition(Vector2Int position) {
        return objectDataSet.GetObjectByPosition(position);
    }

    //Playerの足元のオブジェクトを取得する
    public GameObject GetPlayerFootObject() {        
        return objectDataSet.GetObjectByPositionExceptPlayerAndEnemy(objectDataSet.GetPlayerPosition());
    }

    public void Initialize() {
        if (_i == null) {
            _i = this;
        }                
    }

}
