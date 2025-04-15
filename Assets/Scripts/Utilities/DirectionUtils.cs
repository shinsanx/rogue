using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DirectionUtils {

    private static readonly Vector2Int north = new Vector2Int(0, 1);
    private static readonly Vector2Int south = new Vector2Int(0, -1);
    private static readonly Vector2Int east = new Vector2Int(1, 0);
    private static readonly Vector2Int west = new Vector2Int(-1, 0);
    private static readonly Vector2Int northeast = new Vector2Int(1, 1);
    private static readonly Vector2Int northwest = new Vector2Int(-1, 1);
    private static readonly Vector2Int southeast = new Vector2Int(1, -1);
    private static readonly Vector2Int southwest = new Vector2Int(-1, -1);

    private static readonly List<Vector2Int> directions = new List<Vector2Int> {
        north, south, east, west, northeast, northwest, southeast, southwest
    };


    // 方向ベクトル
    private static readonly Dictionary<DungeonConstants.Direction, (Vector2Int forward, Vector2Int left, Vector2Int right)> DirectionVectors
     = new Dictionary<DungeonConstants.Direction, (Vector2Int, Vector2Int, Vector2Int)> {
        { DungeonConstants.Direction.Stay,      (Vector2Int.zero,   Vector2Int.zero,  Vector2Int.zero) },
        { DungeonConstants.Direction.North,     (Vector2Int.up,    Vector2Int.left,  Vector2Int.right) },
        { DungeonConstants.Direction.South,     (Vector2Int.down,  Vector2Int.left,  Vector2Int.right) },
        { DungeonConstants.Direction.East,      (Vector2Int.right, Vector2Int.up,    Vector2Int.down) },
        { DungeonConstants.Direction.West,      (Vector2Int.left,  Vector2Int.up,    Vector2Int.down) },
        { DungeonConstants.Direction.NorthEast, (new Vector2Int(1, 1),  new Vector2Int(-1, 1), new Vector2Int(1, -1)) },
        { DungeonConstants.Direction.NorthWest, (new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, 1)) },
        { DungeonConstants.Direction.SouthEast, (new Vector2Int(1, -1), new Vector2Int(1, 1), new Vector2Int(-1, -1)) },
        { DungeonConstants.Direction.SouthWest, (new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1)) }
    };

    public static List<Vector2Int> GetSurroundingFacingTiles(Vector2Int currentPos, DungeonConstants.Direction direction) {
        var (forward, left, right) = DirectionVectors[direction];
        return new List<Vector2Int> {
            currentPos + forward,                    // 前            
            currentPos + forward + left,             // 斜め前左
            currentPos + forward + right,            // 斜め前右
            currentPos + left,                       // 左
            currentPos + right                       // 右
        };
    }

    public static Vector2Int GetRandomDirection() {        
        int randomIndex = UnityEngine.Random.Range(0, directions.Count);
        return directions[randomIndex];
    }
}
