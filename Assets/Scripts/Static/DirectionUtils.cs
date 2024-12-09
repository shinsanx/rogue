using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DirectionUtils {

    public static List<Vector2Int> GetSurroundingFacingTiles(Vector2Int currentPos, DungeonConstants.Direction direction){
        List<Vector2Int> surroundingTiles = new List<Vector2Int>();

        switch(direction) {
            case DungeonConstants.Direction.North:
            surroundingTiles.Add(currentPos + Vector2Int.up); //前
            surroundingTiles.Add(currentPos + Vector2Int.up + Vector2Int.left); //前
            surroundingTiles.Add(currentPos + Vector2Int.up); //前
            surroundingTiles.Add(currentPos + Vector2Int.up); //前

        }
    }
}
