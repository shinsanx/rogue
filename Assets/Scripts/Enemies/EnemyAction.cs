using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Move,
    Attack,
    // 他のアクションタイプを追加
}

public class EnemyAction
{
    public ActionType Type { get; set; }
    public Vector2Int TargetPosition { get; set; }
    public GameObject Target { get; set; }
    public Vector2Int Direction { get; set; }
}

