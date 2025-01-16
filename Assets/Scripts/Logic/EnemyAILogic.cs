using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RandomDungeonWithBluePrint;
using UnityEngine.Analytics;
using Unity.VisualScripting;
using System;
using JetBrains.Annotations;

public class EnemyAILogic {
    private IPositionAdapter positionAdapter;
    private EnemyAnimLogic enemyAnimLogic;
    private EnemyAttackLogic enemyAttackLogic;
    private EnemyMoveLogic enemyMoveLogic;
    private IAnimationAdapter animationAdapter;
    private TileLogic tileLogic;
    private AStarPathfinding pathfinding;

    Vector2Int postPlayerPos = Vector2Int.zero;

    bool isInRoom;
    bool adjacentPlayer_first = false;
    bool existPlayerWithView = false;
    GameObject player = null;
    Vector2Int enterJointPos;
    List<Vector2Int> monsterView = new List<Vector2Int>();
    List<Vector2Int> routeCache = new List<Vector2Int>(); //ルート探索時のキャッシュ

    //コンストラクタ
    public EnemyAILogic(IPositionAdapter positionAdapter, IAnimationAdapter animationAdapter, IMonsterStatusAdapter monsterStatusAdapter, SpriteRenderer sr) {
        this.positionAdapter = positionAdapter;
        this.animationAdapter = animationAdapter;

        enemyAnimLogic = new EnemyAnimLogic(animationAdapter, sr);
        enemyAttackLogic = new EnemyAttackLogic(enemyAnimLogic, animationAdapter, positionAdapter, monsterStatusAdapter);
        enemyMoveLogic = new EnemyMoveLogic(positionAdapter, enemyAnimLogic);
        tileLogic = new TileLogic();
        pathfinding = new AStarPathfinding(tileLogic);
    }

    public void AIStart() {
        TurnStartProcess(positionAdapter.Position);
        DecideAction();
        TurnEndProcess(positionAdapter.Position);

        //全モンスターの行動が終わったかどうかを確認するメソッドに通知する
        MessageBus.Instance.Publish(DungeonConstants.NotifyEnemyAct, this);
    }

    private void DecideAction() {
        //if(player == null) Debug.Log("player is null");
        //Debug.Log(postPlayerPos + "postPlayerPos ターンスタート時の目的地");
        Vector2Int selfPosInt = positionAdapter.Position;
        if (selfPosInt == postPlayerPos) {
            postPlayerPos = Vector2Int.zero; //postPlayerPosにたどり着いたら目標地点をリセット
        }
        Vector2 direction = Vector2.zero;
        Vector2Int directionInt = Vector2Int.zero;

        //攻撃可能位置にプレイヤーがいた場合は攻撃
        if (adjacentPlayer_first) {
            direction = (Vector2)player.transform.position - (Vector2)positionAdapter.Position;
            directionInt = direction.ToVector2Int();


            if (tileLogic.CheckMovableTile(selfPosInt, selfPosInt + directionInt)) {
                enemyAttackLogic.Attack(player, directionInt);
                return;
            }
        }
        //Debug.Log(isInRoom + ":is in room");

        //視界にPlayerがいない　かつ　目的地がない場合
        if (!existPlayerWithView && postPlayerPos == Vector2Int.zero) {

            if (isInRoom) { //自分が部屋の中の場合
                //JointPositionの取得
                List<Vector2Int> jointPositions = tileLogic.ExtractJointPosInRoom(selfPosInt);

                //自身がJointの上だった場合は通路に入る（EnterJointを除く）
                if (jointPositions.FirstOrDefault(j => j == selfPosInt) != Vector2Int.zero) {
                    Debug.Log(jointPositions.FirstOrDefault(j => j == selfPosInt) + "on joint");
                    Vector2Int aisle = tileLogic.GetNeighborBranchPositions(selfPosInt).FirstOrDefault();
                    if (selfPosInt != enterJointPos) { //Roomの入り口には入らない
                        Debug.Log("target is" + aisle);
                        postPlayerPos = aisle;
                        MoveToTarget(selfPosInt, postPlayerPos);
                        return;
                    }
                }

                //目指すJointを決定する。
                //出口が一つしかない場合は選択肢なし。
                if (jointPositions.Count == 1) {
                    postPlayerPos = jointPositions[0];
                }

                //ランダムの値が毎回同じにならないようにする
                UnityEngine.Random.InitState(DateTime.Now.Millisecond);

                while (true) {
                    int randNum = UnityEngine.Random.Range(0, jointPositions.Count);
                    postPlayerPos = jointPositions[randNum];
                    //Debug.Log(postPlayerPos + "this is postPlayerPos(jointPos)");
                    if (postPlayerPos == enterJointPos) continue;
                    break;
                }
            }

            //自分が通路にいる場合
            if (!isInRoom) {
                Debug.Log("im in aisle!");
                List<Vector2Int> aisles = tileLogic.GetNeighborBranchPositions(selfPosInt);
                Vector2Int faceDirection = animationAdapter.MoveAnimationDirection.ToVector2Int();

                //通路に生まれた場合の回避策。直す必要あり。
                if (faceDirection == Vector2Int.zero) faceDirection = aisles[0] - selfPosInt;

                List<Vector2Int> facingTiles = DirectionUtils.GetSurroundingFacingTiles(selfPosInt, DungeonConstants.ToDirection[faceDirection]);
                List<Vector2Int> intersection = aisles.Intersect(facingTiles).ToList();

                postPlayerPos = intersection[0];
                //分岐に対応できてない。CheckMovableTileを使ってランダムに選べるようにする
            }
        }

        Move(selfPosInt, MakeRoute(selfPosInt, postPlayerPos)[0]);

    }

    private void Move(Vector2Int selfPos, Vector2Int targetPos) {
        Vector2Int moveDirection = targetPos - selfPos;
        enemyMoveLogic.Move(targetPos, moveDirection);
    }

    //Moveの目的地決定
    private void MoveToTarget(Vector2Int selfPos, Vector2Int targetPos) {
        //Debug.Log(targetPos + ":targetPos in MoveToTarget Method");
        //Debug.Log(monsterView.FirstOrDefault(p => p == targetPos) + "targetPosと一致するmonsterview");
        List<Vector2Int> path = pathfinding.FindPath(selfPos, targetPos, monsterView);

        if (path != null && path.Count > 0) {
            Vector2Int nextStep = path[0];
            //Debug.Log(nextStep + "nextStep");
            Vector2Int moveDirection = nextStep - selfPos;
            enemyMoveLogic.Move(nextStep, moveDirection);
        }
    }

    //周囲１マスのGameObjectを取り出す
    private List<GameObject> GetSurroundingObject(Vector2 selfPos) {
        List<GameObject> surroundingObjects = new List<GameObject>();
        Vector2Int selfPosInt = selfPos.ToVector2Int();

        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int targetPos = selfPosInt + DungeonConstants.ToVector2Int[direction];
            GameObject go = tileLogic.GetObjectOnTile(targetPos);
            if (go != null) {
                surroundingObjects.Add(go);
            }
        }
        return surroundingObjects;
    }

    //ターンスタートプロセス。自身の除隊を判定する
    private void TurnStartProcess(Vector2Int selfPos){
        player = null;
        existPlayerWithView = false;
        monsterView = null;

        //プレイヤーと隣接しているか確認する
        player = GetSurroundingObject(positionAdapter.Position).FirstOrDefault();
    }

    private void TurnEndProcess(Vector2Int selfPos){
        
    }



    // 使用するルートを選定する
    private List<Vector2Int> MakeRoute(Vector2Int selfPos, Vector2Int targetPos) {
        // プレイヤーが視野内の場合、A*アルゴリズムで詳細なパスを計算する
        if (existPlayerWithView) {
            return pathfinding.FindPath(selfPos, targetPos, monsterView);
        }

        // ルートキャッシュが有効で、全ての位置が移動可能ならキャッシュを使用する
        if (routeCache.Count > 0 && DiscernReachable(routeCache)) {
            return routeCache;
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
        if (tileLogic.CheckTileStandable(newPos)) {
            return newPos;
        }

        // 移動できない場合はその場に留まる
        return selfPos;
    }

    // ルートのそれぞれのマスに他オブジェクトが存在していないかチェックする
    private bool DiscernReachable(List<Vector2Int> route) {
        return route.All(position => tileLogic.CheckTileStandable(position));
    }

}
