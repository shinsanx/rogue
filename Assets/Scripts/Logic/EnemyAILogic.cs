using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RandomDungeonWithBluePrint;
using System;


public class EnemyAILogic {
    // 状態を表すクラスを追加
    private class EnemyAIState {
        public bool IsInRoom { get; set; }
        public bool IsAdjacentToPlayerAtStart { get; set; }
        public bool IsAdjacentToPlayerAtEnd { get; set; }
        public bool CanSeePlayer { get; set; }
        public GameObject Player { get; set; }
        public Vector2Int EnterJointPosition { get; set; }
        public Vector2Int LastKnownPlayerPosition { get; set; }
        public List<Vector2Int> MonsterView { get; set; }
        public List<Vector2Int> RouteCache { get; set; }

        public EnemyAIState() {
            MonsterView = new List<Vector2Int>();
            RouteCache = new List<Vector2Int>();
            LastKnownPlayerPosition = Vector2Int.zero;
        }
    }

    private readonly IObjectData objectData;    
    private readonly EnemyAttackLogic enemyAttackLogic;
    private readonly EnemyMoveLogic enemyMoveLogic;    
    private readonly AStarPathfinding pathfinding;
    private readonly EnemyAIState state;    

    //コンストラクタ
    public EnemyAILogic(
        IObjectData objectData,
        EnemyAnimLogic enemyAnimLogic,
        EnemyAttackLogic enemyAttackLogic,
        EnemyMoveLogic enemyMoveLogic,
        IAnimationAdapter animationAdapter,
        AStarPathfinding pathfinding) {
            
        this.objectData = objectData;        
        this.enemyAttackLogic = enemyAttackLogic;
        this.enemyMoveLogic = enemyMoveLogic;        
        this.pathfinding = pathfinding;
        this.state = new EnemyAIState();
    }

    public void AIStart() {
        UpdateEnemyState();
        ExecuteAction();
        UpdateEndState();
        NotifyTurnComplete();
    }

    private void UpdateEnemyState() {
        state.Player = CharacterManager.i.GetPlayer();
        state.IsInRoom = TileManager.i.LookupRoomNum(objectData.Position) != 0;
        Debug.Log($"state.IsInRoom: {state.IsInRoom}");
        state.IsAdjacentToPlayerAtStart = IsAdjacentToPlayer();
        Debug.Log($"state.IsAdjacentToPlayerAtStart: {state.IsAdjacentToPlayerAtStart}");
        state.MonsterView = GetMonsterView();
        Debug.Log($"state.MonsterView: {state.MonsterView.Count}");
        state.CanSeePlayer = CanSeePlayer();
        Debug.Log($"state.CanSeePlayer: {state.CanSeePlayer}");

        if (state.CanSeePlayer && state.Player != null) {
            state.LastKnownPlayerPosition = new Vector2Int(
                Mathf.RoundToInt(state.Player.transform.position.x),
                Mathf.RoundToInt(state.Player.transform.position.y)
            );
        }
    }

    private bool IsAdjacentToPlayer() {
        if (state.Player == null) return false;

        Vector2 playerPos = state.Player.transform.position;
        Vector2 enemyPos = new Vector2(objectData.Position.x, objectData.Position.y);
        return Vector2.Distance(enemyPos, playerPos) <= 1.5f;
    }

    private List<Vector2Int> GetMonsterView() {
        // 既存のGetMonsterViewロジックを実装
        return TileManager.i.ExtractAllRoomPositions(TileManager.i.LookupRoomNum(objectData.Position));

        //通路の場合は、周囲8マスを視野に入れる？
    }

    //プレイヤーが視野内にいるかどうかを判定する
    private bool CanSeePlayer() {
        Vector2Int playerPos = state.Player.transform.position.ToVector2Int();
        return state.MonsterView.Contains(playerPos);
    }


    // 攻撃と移動をする
    private void ExecuteAction() {
        if (TryAttackPlayer()) return;
        if (TryMove()) return;
    }

    // ターンスタート時にプレイヤーと隣接していた場合はプレイヤーを攻撃。
    private bool TryAttackPlayer() {
        if (!state.IsAdjacentToPlayerAtStart) return false;

        Vector2Int enemyPos = objectData.Position;
        Vector2Int playerPos = new Vector2Int(
            Mathf.RoundToInt(state.Player.transform.position.x),
            Mathf.RoundToInt(state.Player.transform.position.y)
        );
        Debug.Log($"playerPos {playerPos}");
        Vector2Int direction = new Vector2Int(
            playerPos.x - enemyPos.x,
            playerPos.y - enemyPos.y
        );

        if (TileManager.i.CheckAttackableTile(enemyPos, enemyPos + direction)) {                        
            enemyAttackLogic.Attack(state.Player, direction);
            return true;
        }
        return false;
    }

    private bool TryMove() {
        Vector2Int targetPosition = DetermineTargetPosition();
        if (targetPosition == objectData.Position) return false;

        List<Vector2Int> route = MakeRoute(objectData.Position, targetPosition);
        if (route != null && route.Count > 0) {
            Move(objectData.Position, route[0]);
            return true;
        }
        return false;
    }

    private Vector2Int DetermineTargetPosition() {
        if (!state.CanSeePlayer && state.LastKnownPlayerPosition == Vector2Int.zero) {
            return state.IsInRoom ?
                DetermineRoomTargetPosition() :
                DetermineCorridorTargetPosition();
        }
        return state.LastKnownPlayerPosition;
    }

    private void Move(Vector2Int selfPos, Vector2Int targetPos) {
        Vector2Int moveDirection = targetPos - selfPos;
        enemyMoveLogic.Move(targetPos, moveDirection);
    }



    // 使用するルートを選定する
    private List<Vector2Int> MakeRoute(Vector2Int selfPos, Vector2Int targetPos) {
        // プレイヤーが視野内の場合、A*アルゴリズムで詳細なパスを計算する
        if (state.CanSeePlayer) {
            return pathfinding.FindPath(selfPos, targetPos, state.MonsterView);
        }

        // ルートキャッシュが有効で、全ての位置が移動可能ならキャッシュを使用する
        if (state.RouteCache.Count > 0 && DiscernReachable(state.RouteCache)) {
            return state.RouteCache;
        }

        // 一歩だけ簡易検索して移動する（視野外のプレイヤーを追跡する場合など）
        return new List<Vector2Int> { MoveTowardsTarget(selfPos, targetPos) };
    }

    // 目的地までの一歩先を簡易検索する
    private Vector2Int MoveTowardsTarget(Vector2Int selfPos, Vector2Int targetPos) {
        // 移動方向を決定する
        int deltaX = targetPos.x - selfPos.x;
        int deltaY = targetPos.y - selfPos.y;

        // x方向の移動量(-1, 0, 1)
        int moveX = Mathf.Clamp(deltaX, -1, 1);

        // y方向の移動量(-1, 0, 1)
        int moveY = Mathf.Clamp(deltaY, -1, 1);

        // 新しい位置を計算
        Vector2Int newPos = new Vector2Int(selfPos.x + moveX, selfPos.y + moveY);

        // 移動可能か確認
        if (TileManager.i.CheckTileStandable(newPos)) {
            return newPos;
        }

        // 移動できない場合はその場に留まる
        return selfPos;
    }

    private Vector2Int DetermineRoomTargetPosition() {
        int currentRoomNum = TileManager.i.LookupRoomNum(objectData.Position);
        List<Vector2Int> roomPositions = TileManager.i.ExtractAllRoomPositions(currentRoomNum);

        if (roomPositions == null || roomPositions.Count == 0) {
            return objectData.Position;
        }

        // ランダムな位置を選択
        int randomIndex = UnityEngine.Random.Range(0, roomPositions.Count);
        return roomPositions[randomIndex];
    }

    private Vector2Int DetermineCorridorTargetPosition() {
        if (state.EnterJointPosition == Vector2Int.zero) {
            var joints = TileManager.i.ExtractJointPosInRoom(objectData.Position);
            if (joints != null && joints.Count > 0) {
                // 最も近いジョイントポイントを選択
                state.EnterJointPosition = joints
                    .OrderBy(j => Vector2Int.Distance(objectData.Position, j))
                    .First();
            } else {
                return objectData.Position;
            }
        }
        return state.EnterJointPosition;
    }

    // ルートのそれぞれのマスに他オブジェクトが存在していないかチェックする
    private bool DiscernReachable(List<Vector2Int> route) {
        return route.All(position => TileManager.i.CheckTileStandable(position));
    }    

    

    private void UpdateEndState() {
        state.IsAdjacentToPlayerAtEnd = IsAdjacentToPlayer();
    }

    private void NotifyTurnComplete() {
        //GameManager.i.EnemyTurnEnd();
    }

    

    

    

    

    // //自身がRoom内かどうか判定する
    // private bool ExistsInRoom(Vector2Int selfPos) {
    //     int roomNum = TileManager.i.LookupRoomNum(selfPos);
    //     if (roomNum == 0) return false;

    //     return true;
    // }

        // //Moveの目的地決定
    // private void MoveToTarget(Vector2Int selfPos, Vector2Int targetPos) {
    //     List<Vector2Int> path = pathfinding.FindPath(selfPos, targetPos, state.MonsterView);

    //     if (path != null && path.Count > 0) {
    //         Vector2Int nextStep = path[0];
    //         Vector2Int moveDirection = nextStep - selfPos;
    //         enemyMoveLogic.Move(nextStep, moveDirection);
    //     }
    // }

    // //周囲１マスのGameObjectを取り出す
    // private List<GameObject> GetSurroundingObject(Vector2 selfPos) {
    //     List<GameObject> surroundingObjects = new List<GameObject>();
    //     Vector2Int selfPosInt = selfPos.ToVector2Int();

    //     foreach (var direction in DungeonConstants.EightDirections) {
    //         Vector2Int targetPos = selfPosInt + DungeonConstants.ToVector2Int[direction];
    //         GameObject go = CharacterManager.i.GetObjectByPosition(targetPos);
    //         if (go != null) {
    //             surroundingObjects.Add(go);
    //         }
    //     }
    //     return surroundingObjects;
    // }

    //ターンスタートプロセス。自身の状態を判定する
    // private void TurnStartProcess(Vector2Int selfPos) {
    //     state.Player = null;
    //     state.IsAdjacentToPlayerAtStart = false;
    //     state.CanSeePlayer = false;
    //     state.MonsterView = null;

    //     //プレイヤーと隣接しているか確認する
    //     state.Player = GetSurroundingObject(objectData.Position).FirstOrDefault();
    //     if (state.Player != null) {
    //         state.IsAdjacentToPlayerAtStart = true;
    //     }
    // }

    // private void TurnEndProcess(Vector2Int selfPos) {
    //     state.IsAdjacentToPlayerAtEnd = false;
    //     //プレイヤーと隣接しているか確認する
    //     state.Player = GetSurroundingObject(objectData.Position).FirstOrDefault();
    //     if (state.Player != null) {
    //         state.IsAdjacentToPlayerAtEnd = true;
    //         state.LastKnownPlayerPosition = state.Player.transform.position.ToVector2Int();
    //     }
    // }
}
