using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DirectionUtils {

    public static List<Vector2Int> GetSurroundingFacingTiles(Vector2Int currentPos, DungeonConstants.Direction direction){
        List<Vector2Int> surroundingTiles = new List<Vector2Int>();

        switch(direction) {
            case DungeonConstants.Direction.North:
            surroundingTiles.Add(currentPos + Vector2Int.up); //前
            surroundingTiles.Add(currentPos + Vector2Int.up + Vector2Int.left); //斜め前左
            surroundingTiles.Add(currentPos + Vector2Int.up + Vector2Int.right); //斜め前右
            surroundingTiles.Add(currentPos + Vector2Int.left); //左
            surroundingTiles.Add(currentPos + Vector2Int.right); //右
            break;

            case DungeonConstants.Direction.South:
            surroundingTiles.Add(currentPos + Vector2Int.down); //前
            surroundingTiles.Add(currentPos + Vector2Int.down + Vector2Int.left); //斜め前左
            surroundingTiles.Add(currentPos + Vector2Int.down + Vector2Int.right); //斜め前右
            surroundingTiles.Add(currentPos + Vector2Int.left); //右
            surroundingTiles.Add(currentPos + Vector2Int.right); //左
            break;

            case DungeonConstants.Direction.East:
            surroundingTiles.Add(currentPos + Vector2Int.right); //前
            surroundingTiles.Add(currentPos + Vector2Int.right + Vector2Int.up); //斜め前左
            surroundingTiles.Add(currentPos + Vector2Int.right + Vector2Int.down); //斜め前右
            surroundingTiles.Add(currentPos + Vector2Int.up); //左
            surroundingTiles.Add(currentPos + Vector2Int.down); //右
            break;

            case DungeonConstants.Direction.West:
            surroundingTiles.Add(currentPos + Vector2Int.left); //前
            surroundingTiles.Add(currentPos + Vector2Int.left + Vector2Int.up); //斜め前左
            surroundingTiles.Add(currentPos + Vector2Int.left + Vector2Int.down); //斜め前右
            surroundingTiles.Add(currentPos + Vector2Int.up); //右
            surroundingTiles.Add(currentPos + Vector2Int.down); //左
            break;
        }
        return surroundingTiles;
    }
}
