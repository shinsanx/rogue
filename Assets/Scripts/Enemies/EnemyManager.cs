using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System;

public class EnemyManager : MonoBehaviour {
    private List<Enemy> enemies = new List<Enemy>();
    private StateMachine stateMachine;
    private State enemyState;
    private IEnemyAIState enemyAIState;
    private GameObject player;
    [SerializeField] private Vector2IntVariable _playerPos;
    private Vector2Int playerPos;
    private Vector2Int enemyCurrentPos;
    private Vector2Int enemyTargetPos;
    private EnemyAction enemyAction;
    private IObjectData objectData;
    private AStarPathfinding pathfinding;
    [SerializeField] private ObjectDataRuntimeSet objectDataSet;
    [SerializeField] GameEvent onCompleteEnemyTurn;

    public void Initialize() {
        stateMachine = GameAssets.i.stateMachine;
        enemyState = GameAssets.i.enemyState;
        player = objectDataSet.GetPlayer();
        // MainThreadHelperのインスタンスを確実に生成
        var helper = MainThreadHelper.Instance;
    }


    // StateLogicで呼び出される
    // 敵の行動をスタートする
    public async void ProcessEnemies() {

        //最初にプレイヤーの位置を取得
        playerPos = _playerPos.Value;
        enemies = objectDataSet.GetAllEnemies();

        foreach (var enemy in enemies) {
            var action = await MainThreadHelper.RunOnMainThread<EnemyAction>(() => {
                var result = AIStart(enemy);
                return result;
            });

            await MainThreadHelper.RunOnMainThread(() => {
                enemy.ExecuteAction(action);
            });
            await Task.Yield();
        }
        onCompleteEnemyTurn.Raise();
    }


    public EnemyAction AIStart(Enemy enemy) {
        ResetEnemyAction();
        if (enemy.sleepTurn.Value > 0) {
            enemy.sleepTurn.Value--;
            return null;
        }
        this.objectData = enemy.GetComponent<IObjectData>();
        enemyAIState = enemy.GetComponent<IEnemyAIState>();
        UpdateEnemyState();
        ExecuteAction();
        UpdateEndState();
        return enemyAction;
    }

    // ========================================================
    // UpdateEnemyState
    // ========================================================

    private void UpdateEnemyState() {
        enemyCurrentPos = objectData.Position.Value;
        enemyAIState.IsInRoomAtStart = TileManager.i.LookupRoomNum(enemyCurrentPos) != 0;
        enemyAIState.StartPosition = enemyCurrentPos;
        enemyTargetPos = enemyAIState.TargetPosition;
        enemyAIState.IsAdjacentToPlayerAtStart = IsAdjacentToPlayer();
        enemyAIState.MonsterView = GetMonsterView();
        enemyAIState.CanSeePlayer = CanSeePlayer();

        if (enemyAIState.LastKnownPlayerPosition == enemyCurrentPos) {
            // LastKnownPlayerPositionに辿り着いた場合はリセットする
            enemyAIState.LastKnownPlayerPosition = Vector2Int.zero;
            Debug.Log("enemyAIState.LastKnownPlayerPosition == enemyCurrentPos");
        }

        if (enemyTargetPos == enemyCurrentPos) {
            // 目的地が自分の位置にある場合はリセットする
            enemyTargetPos = Vector2Int.zero;
            Debug.Log("enemyTargetPos == enemyCurrentPos");
        }

        if (enemyAIState.CanSeePlayer) {
            enemyAIState.LastKnownPlayerPosition = playerPos;
        }
    }

    //プレイヤーが隣接しているかどうかを判定する
    private bool IsAdjacentToPlayer() {
        Vector2 enemyPos = new Vector2(enemyCurrentPos.x, enemyCurrentPos.y);
        return Vector2.Distance(enemyPos, playerPos) <= 1.5f;
    }

    //敵の視野を取得する
    private List<Vector2Int> GetMonsterView() {
        // 既存のGetMonsterViewロジックを実装
        List<Vector2Int> views = TileManager.i.ExtractAllRoomPositions(TileManager.i.LookupRoomNum(enemyCurrentPos));
        List<Vector2Int> surroundingPositions = TileManager.i.GetSurroundingPositions(enemyCurrentPos);
        if (views == null || views.Count == 0) {
            return surroundingPositions;
        }
        views.AddRange(surroundingPositions);
        return views;
    }

    //プレイヤーが視野内にいるかどうかを判定する
    private bool CanSeePlayer() {
        return enemyAIState.MonsterView.Contains(playerPos);
    }

    // ========================================================
    // ExecuteAction
    // ========================================================

    // 攻撃と移動をする
    private void ExecuteAction() {
        if (TryAttackPlayer()) return;
        //GetRandomPositionInSurrounding(enemyCurrentPos); //デバッグ用
        if (TryMove()) return;
    }

    // ターンスタート時にプレイヤーと隣接していた場合はプレイヤーを攻撃。
    private bool TryAttackPlayer() {
        if (!enemyAIState.IsAdjacentToPlayerAtStart) return false;

        Vector2Int direction = new Vector2Int(
            playerPos.x - enemyCurrentPos.x,
            playerPos.y - enemyCurrentPos.y
        );

        if (TileManager.i.CheckAttackableTile(enemyCurrentPos, enemyCurrentPos + direction)) {
            enemyAction.Direction = direction;
            enemyAction.Type = ActionType.Attack;
            enemyAction.Target = player;
            return true;
        }
        return false;
    }

    // 移動をする
    private bool TryMove() {
        Vector2Int targetPosition = DetermineTargetPosition();
        //Vector2Int targetPosition = GetRandomPositionInSurrounding(enemyCurrentPos); //デバッグ用

        if (targetPosition == enemyCurrentPos) return false;

        // 目的地が0の場合は移動しない ただしこのままだと永遠に移動しないので別の目的地を探索するメソッドが必要になる。
        if (targetPosition == Vector2Int.zero) {
            enemyAction.TargetPosition = objectData.Position.Value;
            return false;
        }

        List<Vector2Int> route = MakeRoute(enemyCurrentPos, targetPosition);
        if (route != null && route.Count > 0) {
            enemyAction.Type = ActionType.Move;
            enemyAction.TargetPosition = route[0];
            enemyAction.Direction = route[0] - enemyCurrentPos;
            enemyAIState.FacingDirection = route[0] - enemyCurrentPos;
            enemyAIState.EndPosition = targetPosition;
            return true;
        }
        return false;
    }

    // ========================================================
    // Move系処理
    // ========================================================

    // 目的地を決める
    private Vector2Int DetermineTargetPosition() {
        // 既に目的地が設定されている場合はそれを返す
        if (enemyTargetPos != Vector2Int.zero) {
            return enemyTargetPos;
        }

        // プレイヤーの最後の位置情報がある場合はそれを返す
        if (enemyAIState.LastKnownPlayerPosition != Vector2Int.zero) {
            // プレイヤーが角越しに隣接している場合の迂回処理
            if (TileManager.i.IsAdjacentTo(enemyCurrentPos, enemyAIState.LastKnownPlayerPosition)) {
                if (!TileManager.i.CheckMovableTile(enemyCurrentPos, enemyAIState.LastKnownPlayerPosition)) {
                    return MoveAlternativeTarget(enemyCurrentPos, enemyAIState.LastKnownPlayerPosition);
                }
            }
            // プレイヤーの最後の位置情報を返す
            return enemyAIState.LastKnownPlayerPosition;
        }

        // 目的地がない場合、新規に目的地を設定する
        // 部屋にいる場合、通路にいる場合で処理を分ける
        return enemyAIState.IsInRoomAtStart ?
            DetermineRoomTargetPosition() :
            DetermineCorridorTargetPosition();
        //return GetRandomPositionInSurrounding(enemyCurrentPos); //デバッグ用
    }

    // プレイヤーが角越しに隣接している場合の迂回処理
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

    // 使用するルートを選定する
    private List<Vector2Int> MakeRoute(Vector2Int selfPos, Vector2Int targetPos) {
        // プレイヤーが視野内の場合、A*アルゴリズムで詳細なパスを計算する        
        if (enemyAIState.CanSeePlayer) {
            Debug.Log("AstarPathfinding");
            //return pathfinding.FindPath(selfPos, targetPos, enemyAIState.MonsterView);
        }

        // // ルートキャッシュが有効で、全ての位置が移動可能ならキャッシュを使用する
        // if (state.RouteCache.Count > 0 && DiscernReachable(state.RouteCache)) {
        //     return state.RouteCache;
        // }

        // 一歩だけ簡易検索して移動する（視野外のプレイヤーを追跡する場合など）
        return new List<Vector2Int> { MoveTowardsTarget(selfPos, targetPos) };
    }

    private Vector2Int DetermineRoomTargetPosition() {
        // 自身がJointPositionにいる場合は、通路に入る
        var jointPositions = TileManager.i.ExtractJointPosInRoom(enemyCurrentPos);
        if (jointPositions.Any(j => j == enemyCurrentPos) && enemyAIState.EnterJointPosition != enemyCurrentPos) {
            var neighborBranchPositions = TileManager.i.GetNeighborBranchPositions(enemyCurrentPos);
            if (neighborBranchPositions.Count > 0) {
                return neighborBranchPositions[0];
            }
        }
        return DetermineJointTargetPosition();
    }

    // JointPositionの目的地を決める
    private Vector2Int DetermineJointTargetPosition() {
        var joints = TileManager.i.ExtractJointPosByRoomNum(objectData.RoomNum.Value);

        //EnterJointPositionが0の場合は、最も近いジョイントポイントを選択
        if (enemyAIState.EnterJointPosition == Vector2Int.zero) {

            //jointsが一つ以上ある場合
            if (joints != null && joints.Count > 0) {
                // 最も近いジョイントポイントを選択
                //enemyAIState.EnterJointPosition = joints
                return joints
                    .OrderBy(j => Vector2Int.Distance(enemyCurrentPos, j))
                    .First();
            } else {
                Debug.Log("joints is null or empty");
                return enemyCurrentPos;
            }
        }

        //EnterJointPositionがJointPositionに含まれている場合は削除
        if (joints.Contains(enemyAIState.EnterJointPosition)) {
            joints.Remove(enemyAIState.EnterJointPosition);
        }

        //もしJointPositionが1つの場合はその位置を返す
        if (joints.Count == 1) {
            return joints[0];
        }

        //もしJointPositionが0の場合は、とりあえず一歩進む
        if (joints.Count == 0) {
            enemyAIState.EnterJointPosition = Vector2Int.zero;
            Debug.Log("joints.Count == 0");
            return DetermineCorridorTargetPosition();
        }

        //ランダムなJointPositionを選択する
        foreach (var joint in joints) {
            int randomIndex = UnityEngine.Random.Range(0, joints.Count);
            var randomJoint = joints[randomIndex];
            if (randomJoint != enemyAIState.EnterJointPosition) {
                enemyTargetPos = randomJoint;
                return randomJoint;
            }
        }
        Debug.Log("JointTargetPositionが見つかりませんでした");
        return enemyCurrentPos;
    }

    // 通路にいる場合の目的地を決める
    private Vector2Int DetermineCorridorTargetPosition() {

        // 自身の向いている方向を取得する        
        var facingDirection = GetFacingDirection();

        // 前方5方向で移動可能な方向に移動する
        var facingPositions = DirectionUtils.GetSurroundingFacingTiles(enemyCurrentPos, facingDirection);

        foreach (var facePos in facingPositions) {
            if (TileManager.i.CheckMovableTile(enemyCurrentPos, facePos)) {
                return facePos;
            }
        }
        Debug.Log("目の前の5マスに移動可能な目的地が見つかりませんでした enemyCurrentPos:" + enemyCurrentPos);
        // 移動できない場合はその場に留まる        
        return Vector2Int.zero;
    }

    // 自身の向いている方向を取得する
    private DungeonConstants.Direction GetFacingDirection() {
        return DungeonConstants.ToDirection[enemyAIState.FacingDirection];
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

    // ========================================================
    // UpdateEndState
    // ========================================================

    private void UpdateEndState() {
        enemyAIState.IsInRoomAtEnd = TileManager.i.LookupRoomNum(enemyAIState.EndPosition) != 0;
        RecordEnterJointPosition();
        enemyAIState.IsAdjacentToPlayerAtEnd = IsAdjacentToPlayer();
        if (enemyAIState.IsAdjacentToPlayerAtEnd) {
            enemyAIState.LastKnownPlayerPosition = playerPos;
        }
    }

    // 自身が入ったJointPositionを記録する
    private void RecordEnterJointPosition() {
        if (!enemyAIState.IsInRoomAtStart && enemyAIState.IsInRoomAtEnd) {
            var jointPositions = TileManager.i.ExtractJointPosInRoom(enemyAIState.EndPosition);
            if (jointPositions == null) {
                Debug.Log("jointPositions is null");
                return;
            }
            if (jointPositions.Any(j => j == enemyAIState.EndPosition)) {
                enemyAIState.EnterJointPosition = enemyAIState.EndPosition;
            }
        }
    }

    private void ResetEnemyAction() {
        enemyAction = new EnemyAction();
    }

    // 周囲8マスのランダムな位置を取得する
    // デバッグ用
    private Vector2Int GetRandomPositionInSurrounding(Vector2Int selfPos) {
        var surroundingPositions = TileManager.i.GetSurroundingPositions(selfPos);
        enemyAction.Type = ActionType.Move;
        enemyAction.TargetPosition = surroundingPositions[UnityEngine.Random.Range(0, surroundingPositions.Count)];
        enemyAction.Direction = surroundingPositions[UnityEngine.Random.Range(0, surroundingPositions.Count)] - selfPos;
        enemyAIState.FacingDirection = surroundingPositions[UnityEngine.Random.Range(0, surroundingPositions.Count)] - selfPos;
        enemyAIState.EndPosition = surroundingPositions[UnityEngine.Random.Range(0, surroundingPositions.Count)];
        return surroundingPositions[UnityEngine.Random.Range(0, surroundingPositions.Count)];
    }








}
