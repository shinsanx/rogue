using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAIState
{
    bool IsInRoomAtStart { get; set; }
    bool IsInRoomAtEnd { get; set; }
    bool IsAdjacentToPlayerAtStart { get; set; }
    bool IsAdjacentToPlayerAtEnd { get; set; }    
    bool CanSeePlayer { get; set; }
    Vector2Int FacingDirection { get; set; }
    Vector2Int EnterJointPosition { get; set; }
    Vector2Int LastKnownPlayerPosition { get; set; }
    Vector2Int TargetPosition { get; set; }
    Vector2Int StartPosition { get; set; }
    Vector2Int EndPosition { get; set; }
    List<Vector2Int> MonsterView { get; set; }
    List<Vector2Int> RouteCache { get; set; }   
}
