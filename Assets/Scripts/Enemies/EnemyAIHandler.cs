using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ―――――――――――――――――――――――――――――――――――――――――
/// １体の Enemy が「次の行動」を決めるだけのクラス。
/// ・EnemyManager から呼び出される “脳みそ”
/// ・依存：Enemy / ObjectDataRuntimeSet / Player 座標
/// ・将来的に State パターンへ差し替えやすい構造にしてある
/// ―――――――――――――――――――――――――――――――――――――――――
/// </summary>
public class EnemyAIHandler
{
    // ─────────────────────────────
    // 依存オブジェクト
    // ─────────────────────────────
    private readonly Enemy                      enemy;
    private readonly ObjectDataRuntimeSet       objectDataSet;
    private readonly Vector2Int                 playerPos;
    private readonly IObjectData                objectData;
    private readonly IEnemyAIState              aiState;

    // ─────────────────────────────
    // 作業用キャッシュ
    // ─────────────────────────────
    private EnemyAction  action          = new();
    private Vector2Int   currentPos;
    private Vector2Int   targetPos;

    // コンストラクタ ---------------------------------------------------
    public EnemyAIHandler(Enemy enemy, ObjectDataRuntimeSet set, Vector2Int playerPos)
    {
        this.enemy         = enemy;
        this.objectDataSet = set;
        this.playerPos     = playerPos;

        objectData = enemy.GetComponent<IObjectData>();
        aiState    = enemy.GetComponent<IEnemyAIState>();
    }

    // =================================================================
    //  外部 API  : 1ターンぶんの行動を決定して返す
    // =================================================================
    public EnemyAction DecideAction(bool allowMove = true, bool allowAttack = true)
    {
        ResetAction();

        // 1. ステータス異常 ―――――――――――――――――――――――――――――
        if (IsSleeping())         return SetSleepAction();   // 睡眠
        if (IsConfused())         HandleConfusion();         // 混乱（※上書きするので return しない）

        // 2. AI ステート更新 ―――――――――――――――――――――――――
        UpdateState();

        // 3. 通常ロジック（攻撃 ⇒ 移動） ―――――――――――――――――
        ExecuteMainLogic();

        // 4. EndState 処理
        UpdateEndState();

        // 5. Move/Attack フィルタ
        ApplyFilters(allowMove, allowAttack);

        return action;
    }

    // =================================================================
    // 1) ステータス異常判定
    // =================================================================
    private bool IsSleeping()  => enemy.isSleeping.Value;
    private bool IsConfused()  => enemy.isConfusion.Value;

    private EnemyAction SetSleepAction()
    {
        action.Type          = ActionType.Sleep;
        action.TargetPosition = objectData.Position.Value;
        return action;
    }

    private void HandleConfusion()
    {
        // 既に行動が決定済みでも、混乱は最優先で上書きする
        Vector2Int dir = Vector2Int.zero;
        for (int i = 0; i < 10; i++)
        {
            dir = DirectionUtils.GetRandomDirection();
            if (TileManager.i.CheckMovableTile(objectData.Position.Value,
                                               objectData.Position.Value + dir))
                break;
        }

        action.Type           = ActionType.Move;
        action.Direction      = dir;
        action.TargetPosition = objectData.Position.Value + dir;
        action.Target         = objectDataSet.GetObjectByPosition(action.TargetPosition);
    }

    // =================================================================
    // 2) ステート更新
    // =================================================================
    private void UpdateState()
    {
        currentPos                       = objectData.Position.Value;
        aiState.StartPosition            = currentPos;
        aiState.IsInRoomAtStart          = TileManager.i.LookupRoomNum(currentPos) != 0;
        aiState.IsAdjacentToPlayerAtStart= IsAdjacentToPlayer();
        aiState.MonsterView              = GetMonsterView();
        aiState.CanSeePlayer             = aiState.MonsterView.Contains(playerPos);

        if (aiState.CanSeePlayer)
            aiState.LastKnownPlayerPosition = playerPos;
    }

    // =================================================================
    // 3) 攻撃 → 移動
    // =================================================================
    private void ExecuteMainLogic()
    {
        if (TryAttack()) return;
        if (TryMove())   return;
        // どれも出来なければ ActionType.None のまま
    }

    private bool TryAttack()
    {
        if (!aiState.IsAdjacentToPlayerAtStart) return false;

        var dir = playerPos - currentPos;
        if (!TileManager.i.CheckAttackableTile(currentPos, currentPos + dir)) return false;

        action.Type      = ActionType.Attack;
        action.Direction = dir;
        action.Target    = objectDataSet.GetPlayer();
        return true;
    }

    private bool TryMove()
    {
        targetPos = DetermineTargetPosition();

        if (targetPos == currentPos || targetPos == Vector2Int.zero)
            return false;

        var route = MakeRoute(currentPos, targetPos);
        if (route == null || route.Count == 0) return false;

        var next = route[0];
        action.Type           = ActionType.Move;
        action.TargetPosition = next;
        action.Direction      = next - currentPos;
        aiState.FacingDirection = action.Direction;
        aiState.EndPosition     = targetPos;
        return true;
    }

    // =================================================================
    // 4) EndState 更新
    // =================================================================
    private void UpdateEndState()
    {
        aiState.IsAdjacentToPlayerAtEnd = IsAdjacentToPlayer();
        if (aiState.IsAdjacentToPlayerAtEnd)
            aiState.LastKnownPlayerPosition = playerPos;
    }

    // =================================================================
    // 5) フィルタ
    // =================================================================
    private void ApplyFilters(bool allowMove, bool allowAttack)
    {
        if (!allowMove   && action.Type == ActionType.Move)   action.Type = ActionType.None;
        if (!allowAttack && action.Type == ActionType.Attack) action.Type = ActionType.None;
    }

    // =================================================================
    // ヘルパー : 判定・経路
    // =================================================================
    private bool IsAdjacentToPlayer()
        => Vector2Int.Distance(currentPos, playerPos) <= 1.5f;

    private List<Vector2Int> GetMonsterView()
    {
        var room  = TileManager.i.ExtractAllRoomPositions(TileManager.i.LookupRoomNum(currentPos));
        var around= TileManager.i.GetSurroundingPositions(currentPos);
        if (room == null || room.Count == 0) return around;
        room.AddRange(around);
        return room;
    }

    // ------------------ 移動ターゲット決定 ----------------------------
    private Vector2Int DetermineTargetPosition()
    {
        // 1) 既存ターゲット
        if (aiState.TargetPosition != Vector2Int.zero)
            return aiState.TargetPosition;

        // 2) 最後に見たプレイヤー位置
        if (aiState.LastKnownPlayerPosition != Vector2Int.zero)
        {
            if (TileManager.i.IsAdjacentTo(currentPos, aiState.LastKnownPlayerPosition) &&
                !TileManager.i.CheckMovableTile(currentPos, aiState.LastKnownPlayerPosition))
            {
                return MoveAlternativeTarget(currentPos, aiState.LastKnownPlayerPosition);
            }
            return aiState.LastKnownPlayerPosition;
        }

        // 3) 部屋 or 通路
        return aiState.IsInRoomAtStart
             ? DetermineRoomTarget()
             : DetermineCorridorTarget();
    }

    private Vector2Int MoveAlternativeTarget(Vector2Int self, Vector2Int target)
    {
        int dx = Mathf.Clamp(target.x - self.x, -1, 1);
        int dy = Mathf.Clamp(target.y - self.y, -1, 1);

        var cand = new Vector2Int(self.x + dx, self.y);
        if (TileManager.i.CheckMovableTile(self, cand)) return cand;

        cand = new Vector2Int(self.x, self.y + dy);
        if (TileManager.i.CheckMovableTile(self, cand)) return cand;

        cand = new Vector2Int(self.x + dx, self.y + dy);
        if (TileManager.i.CheckMovableTile(self, cand)) return cand;

        return self; // 迂回失敗
    }

    private Vector2Int DetermineRoomTarget()
    {
        var joints = TileManager.i.ExtractJointPosInRoom(currentPos);
        if (joints.Any(j => j == currentPos) &&
            aiState.EnterJointPosition != currentPos)
        {
            var branches = TileManager.i.GetNeighborBranchPositions(currentPos);
            if (branches.Count > 0) return branches[0];
        }
        return DetermineJointTarget();
    }

    private Vector2Int DetermineJointTarget()
    {
        var joints = TileManager.i.ExtractJointPosByRoomNum(objectData.RoomNum.Value);
        if (joints == null || joints.Count == 0) return currentPos;

        // 既に入った Joint は除く
        joints.Remove(aiState.EnterJointPosition);

        // 最寄り
        return joints.OrderBy(j => Vector2Int.Distance(currentPos, j)).First();
    }

    private Vector2Int DetermineCorridorTarget()
    {
        var facingDir = GetFacingDirection(aiState.FacingDirection);
        var forwardTiles = DirectionUtils.GetSurroundingFacingTiles(currentPos, facingDir);

        foreach (var pos in forwardTiles)
            if (TileManager.i.CheckMovableTile(currentPos, pos)) return pos;

        return Vector2Int.zero;
    }

    // 自身の向いている方向を取得する
    private DungeonConstants.Direction GetFacingDirection(Vector2Int dir) {
        return DungeonConstants.ToDirection[dir];
    }

    // ------------------ 経路計算 ----------------------------
    private List<Vector2Int> MakeRoute(Vector2Int from, Vector2Int to)
    {
        // 視界内なら A* を本来は呼ぶ
        if (aiState.CanSeePlayer)
        {
            // 例 : return pathfinding.FindPath(from, to, aiState.MonsterView);
        }
        // 1歩だけ近づく簡易版
        return new List<Vector2Int> { MoveTowardsTarget(from, to) };
    }

    private Vector2Int MoveTowardsTarget(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Clamp(to.x - from.x, -1, 1);
        int dy = Mathf.Clamp(to.y - from.y, -1, 1);
        var next = new Vector2Int(from.x + dx, from.y + dy);

        return TileManager.i.CheckMovableTile(from, next) ? next : from;
    }

    // =================================================================
    // リセット
    // =================================================================
    private void ResetAction() => action = new EnemyAction();
}
