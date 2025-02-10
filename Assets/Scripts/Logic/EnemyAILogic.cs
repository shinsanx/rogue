using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RandomDungeonWithBluePrint;
using System;
using TMPro;


public class EnemyAILogic {
    // 状態を表すクラスを追加
    private class EnemyAIState {
        public bool IsInRoomAtStart { get; set; }
        public bool IsInRoomAtEnd { get; set; }
        public bool IsAdjacentToPlayerAtStart { get; set; }
        public bool IsAdjacentToPlayerAtEnd { get; set; }
        public bool CanSeePlayer { get; set; }
        public Vector2Int FacingDirection { get; set; }
        public GameObject Player { get; set; }
        public Vector2Int EnterJointPosition { get; set; }
        public Vector2Int LastKnownPlayerPosition { get; set; }
        public Vector2Int TargetPosition { get; set; }
        public List<Vector2Int> MonsterView { get; set; }
        public List<Vector2Int> RouteCache { get; set; }

        public EnemyAIState() {
            MonsterView = new List<Vector2Int>();
            RouteCache = new List<Vector2Int>();
            LastKnownPlayerPosition = Vector2Int.zero;
            TargetPosition = Vector2Int.zero;
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
        EnemyAttackLogic enemyAttackLogic,
        EnemyMoveLogic enemyMoveLogic,
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
        state.IsInRoomAtStart = TileManager.i.LookupRoomNum(objectData.Position) != 0;
        state.IsAdjacentToPlayerAtStart = IsAdjacentToPlayer();
        state.MonsterView = GetMonsterView();
        state.CanSeePlayer = CanSeePlayer();

        if (state.LastKnownPlayerPosition == objectData.Position) {
            // LastKnownPlayerPositionに辿り着いた場合はリセットする
            state.LastKnownPlayerPosition = Vector2Int.zero;
        }

        if (state.TargetPosition == objectData.Position) {
            // 目的地が自分の位置にある場合はリセットする
            state.TargetPosition = Vector2Int.zero;
        }

        if (state.CanSeePlayer) {
            state.LastKnownPlayerPosition = state.Player.GetComponent<IObjectData>().Position;
        }
    }

    private bool IsAdjacentToPlayer() {
        if (state.Player == null) return false;

        Vector2 playerPos = state.Player.GetComponent<IObjectData>().Position;
        Vector2 enemyPos = new Vector2(objectData.Position.x, objectData.Position.y);
        return Vector2.Distance(enemyPos, playerPos) <= 1.5f;
    }

    private List<Vector2Int> GetMonsterView() {
        // 既存のGetMonsterViewロジックを実装
        List<Vector2Int> views = TileManager.i.ExtractAllRoomPositions(TileManager.i.LookupRoomNum(objectData.Position));
        List<Vector2Int> surroundingPositions = TileManager.i.GetSurroundingPositions(objectData.Position);
        if (views == null || views.Count == 0) {
            return surroundingPositions;
        }
        views.AddRange(surroundingPositions);
        return views;
    }

    //プレイヤーが視野内にいるかどうかを判定する
    private bool CanSeePlayer() {
        Vector2Int playerPos = state.Player.GetComponent<IObjectData>().Position;
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
        Vector2Int playerPos = state.Player.GetComponent<IObjectData>().Position;
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
        // 既に目的地が設定されている場合はそれを返す
        if (state.TargetPosition != Vector2Int.zero) {
            return state.TargetPosition;
        }

        // プレイヤーの最後の位置情報がある場合はそれを返す
        if (state.LastKnownPlayerPosition != Vector2Int.zero) {

            // プレイヤーが角越しに隣接している場合の迂回処理
            if (TileManager.i.IsAdjacentTo(objectData.Position, state.LastKnownPlayerPosition)) {
                if (!TileManager.i.CheckMovableTile(objectData.Position, state.LastKnownPlayerPosition)) {                    
                    return MoveAlternativeTarget(objectData.Position, state.LastKnownPlayerPosition);
                }
            }


            return state.LastKnownPlayerPosition;
        }

        // 新規に目的地を設定する
        return state.IsInRoomAtStart ?
            DetermineRoomTargetPosition() :
            DetermineCorridorTargetPosition();
    }

    private void Move(Vector2Int selfPos, Vector2Int targetPos) {
        state.FacingDirection = targetPos - selfPos;
        enemyMoveLogic.Move(targetPos, state.FacingDirection);
    }



    // 使用するルートを選定する
    private List<Vector2Int> MakeRoute(Vector2Int selfPos, Vector2Int targetPos) {
        // プレイヤーが視野内の場合、A*アルゴリズムで詳細なパスを計算する
        if (state.CanSeePlayer) {
            Debug.Log("プレイヤーが視野内の場合、A*アルゴリズムで詳細なパスを計算する");
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
        if (TileManager.i.CheckMovableTile(selfPos, newPos)) {
            return newPos;
        }

        // 移動できない場合はその場に留まる
        return selfPos;
    }

    private Vector2Int MoveAlternativeTarget(Vector2Int selfPos, Vector2Int targetPos) {
        // 移動方向を決定する
        int deltaX = targetPos.x - selfPos.x;
        int deltaY = targetPos.y - selfPos.y;

        // x方向の移動量(-1, 0, 1)
        int moveX = Mathf.Clamp(deltaX, -1, 1);
        if (TileManager.i.CheckMovableTile(selfPos, new Vector2Int(selfPos.x + moveX, selfPos.y))) {
            return new Vector2Int(selfPos.x + moveX, selfPos.y);
        }

        // y方向の移動量(-1, 0, 1)
        int moveY = Mathf.Clamp(deltaY, -1, 1);
        if (TileManager.i.CheckMovableTile(selfPos, new Vector2Int(selfPos.x, selfPos.y + moveY))) {
            return new Vector2Int(selfPos.x, selfPos.y + moveY);
        }

        // 新しい位置を計算
        Vector2Int newPos = new Vector2Int(selfPos.x + moveX, selfPos.y + moveY);

        // 移動可能か確認
        if (TileManager.i.CheckMovableTile(selfPos, newPos)) {
            return newPos;
        }

        // 移動できない場合はその場に留まる
        return selfPos;
    }

    private Vector2Int DetermineRoomTargetPosition() {
        // 自身がJointPositionにいる場合は、通路に入る
        var jointPositions = TileManager.i.ExtractJointPosInRoom(objectData.Position);
        if (jointPositions.Any(j => j == objectData.Position) && state.EnterJointPosition != objectData.Position) {
            var neighborBranchPositions = TileManager.i.GetNeighborBranchPositions(objectData.Position);
            if (neighborBranchPositions.Count > 0) {
                Debug.Log("通路に入ります");
                return neighborBranchPositions[0];
            }
        }
        return DetermineJointTargetPosition();
    }

    // JointPositionにいる場合の目的地を決める
    private Vector2Int DetermineJointTargetPosition() {
        var joints = TileManager.i.ExtractJointPosInRoom(objectData.Position);

        if (state.EnterJointPosition == Vector2Int.zero) {
            if (joints != null && joints.Count > 0) {
                // 最も近いジョイントポイントを選択
                state.EnterJointPosition = joints
                    .OrderBy(j => Vector2Int.Distance(objectData.Position, j))
                    .First();
            } else {
                Debug.Log("joints is null or empty");
                return objectData.Position;
            }
        }
        Debug.Log("ここで別のジョイントポジションをターゲットにする");
        //jointPositionの中からランダムで選択する
        while (true) {
            int randomIndex = UnityEngine.Random.Range(0, joints.Count);
            var randomJoint = joints[randomIndex];
            if (randomJoint != state.EnterJointPosition) {
                state.TargetPosition = randomJoint;
                return randomJoint;
            }
        }
    }

    // 通路にいる場合の目的地を決める
    private Vector2Int DetermineCorridorTargetPosition() {

        // 自身の向いている方向を取得する
        var facingDirection = GetFacingDirection();

        // 前方5方向で移動可能な方向に移動する
        var directions = DirectionUtils.GetSurroundingFacingTiles(objectData.Position, facingDirection);
        foreach (var direction in directions) {
            if (TileManager.i.CheckTileStandable(direction)) {
                return direction;
            }
        }
        return objectData.Position;
    }

    // ルートのそれぞれのマスに他オブジェクトが存在していないかチェックする
    private bool DiscernReachable(List<Vector2Int> route) {
        return route.All(position => TileManager.i.CheckTileStandable(position));
    }

    private void UpdateEndState() {
        state.IsInRoomAtEnd = TileManager.i.LookupRoomNum(objectData.Position) != 0;
        RecordEnterJointPosition();
        state.IsAdjacentToPlayerAtEnd = IsAdjacentToPlayer();
        if (state.IsAdjacentToPlayerAtEnd) {
            state.LastKnownPlayerPosition = state.Player.GetComponent<IObjectData>().Position;
        }
    }

    private void NotifyTurnComplete() {
        //GameManager.i.EnemyTurnEnd();
    }

    // 自身の向いている方向を取得する
    private DungeonConstants.Direction GetFacingDirection() {
        return DungeonConstants.ToDirection[state.FacingDirection];
    }

    // 自身が入ったJointPositionを記録する
    private void RecordEnterJointPosition() {
        if (!state.IsInRoomAtStart && state.IsInRoomAtEnd) {
            var jointPositions = TileManager.i.ExtractJointPosInRoom(objectData.Position);
            if (jointPositions.Any(j => j == objectData.Position)) {
                state.EnterJointPosition = objectData.Position;
                Debug.Log($"state.EnterJointPosition: {state.EnterJointPosition}");
            }
        }
    }
}
