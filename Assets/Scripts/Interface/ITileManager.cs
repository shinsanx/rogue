using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileManager
{
    int LookupRoomNum(Vector2Int position);
    bool CheckMovableTile(Vector2Int currentPos, Vector2Int targetPos);
    bool CheckAttackableTile(Vector2Int currentPos, Vector2Int targetPos);
    bool CheckWalkableTile(Vector2Int currentPos, Vector2Int targetPos);
    bool CheckTileStandable(Vector2Int targetPos);
    Item CheckExistItem(Vector2Int targetPos);
    bool CheckExistObject(Vector2Int targetPos);
    GameObject CheckExistStair(Vector2Int targetPos);
    int GetTileType(Vector2Int vector);
    int GetMapChipType(Vector2Int vector);
    List<Vector2Int> ExtractJointPosInRoom(Vector2Int selfPos);
    List<Vector2Int> ExtractJointPosByRoomNum(int roomNum);
    bool CheckExistJoint(Vector2Int targetPos);
    bool CheckExitsAisle(Vector2Int targetPos);
    List<Vector2Int> ExtractAllRoomPositions(int roomNum);
    List<Vector2Int> GetNeighborBranchPositions(Vector2Int selfPos);
    List<Vector2Int> GetSurroundingPositions(Vector2Int selfPos);
    bool IsAdjacentTo(Vector2Int selfPos, Vector2Int targetPos);
    List<GameObject> GetSurroundingObjects(Vector2Int selfPos);
    Vector2Int GetRandomPosition();
    Vector2Int GetCharactersInFront(Vector2Int selfPos, Vector2Int direction, int distance);
    GameObject GetObjectByPosition(Vector2Int position);
    GameObject GetPlayerFootObject();
}
