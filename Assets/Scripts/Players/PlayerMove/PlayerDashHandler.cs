using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RandomDungeonWithBluePrint;

/// <summary>
/// ダッシュ系（通常ダッシュ & Zダッシュ）を一手に引き受けるハンドラ。
/// MoveHandler の Move() を再利用して実際の位置更新を行う。
/// </summary>
public class PlayerDashHandler {
    // ===== 依存関係 =====
    private readonly IObjectData objectData;
    private readonly TileManager tileManager;
    private readonly PlayerMoveHandler moveHandler;
    private readonly Vector2Variable faceDir;
    private readonly GameEvent dirChangedEvent;

    public PlayerDashHandler(
        IObjectData objectData,
        TileManager tileManager,
        PlayerMoveHandler moveHandler,
        Vector2Variable playerFaceDirection,
        GameEvent onPlayerDirectionChanged)
    {
        this.objectData        = objectData;
        this.tileManager       = tileManager;
        this.moveHandler       = moveHandler;
        this.faceDir           = playerFaceDirection;
        this.dirChangedEvent   = onPlayerDirectionChanged;
    }

    /* ───────────────────────────────────────────────
       通常ダッシュ
    ─────────────────────────────────────────────── */
    public async Task DashByInput(Vector2 inputVec) {
        Vector2Int dir     = new(Mathf.RoundToInt(inputVec.x), Mathf.RoundToInt(inputVec.y));
        Vector2Int currPos = objectData.Position.Value;

        // アイテムの上に乗るだけなら 1 歩で終了
        if (TryGetOnItem(currPos, dir)) return;

        await DashUntilObstacleAsync(currPos, dir);
    }

    private bool TryGetOnItem(Vector2Int currPos, Vector2Int dir) {
        Item item = tileManager.CheckExistItem(currPos + dir);
        if (item == null) return false;

        item.OnGetOnItem();
        moveHandler.Move(currPos, currPos + dir);
        return true;
    }

    private async Task DashUntilObstacleAsync(Vector2Int currPos, Vector2Int dir) {
        while (true) {
            Vector2Int next = currPos + dir;

            if (!tileManager.CheckMovableTile(currPos, next)) break; // 壁
            if (tileManager.CheckExistObject(next))           break; // 障害物

            currPos = next;
            moveHandler.Move(currPos, next);

            if (ShouldStopForEnemies(currPos))  break;
            if (tileManager.CheckExistJoint(currPos)) break;

            await Task.Delay(50);
        }
    }

    private bool ShouldStopForEnemies(Vector2Int pos) =>
        tileManager.GetSurroundingObjects(pos)
                   .Any(o => o.GetComponent<Enemy>() &&
                             tileManager.CheckAttackableTile(pos, o.GetComponent<Enemy>()
                                                                    .objectData.Position.Value));

    /* ───────────────────────────────────────────────
       Z ダッシュ（アイテム自動サーチ付き）
    ─────────────────────────────────────────────── */
    public async Task ZDash(Vector2 inputVec) {
        Vector2Int dir     = new(Mathf.RoundToInt(inputVec.x), Mathf.RoundToInt(inputVec.y));
        Vector2Int currPos = objectData.Position.Value;

        if (tileManager.LookupRoomNum(currPos + dir) != 0)
            await ZDashInRoomAsync(currPos, dir);
        else
            await ZDashInCorridorAsync(currPos, dir);
    }

    private async Task ZDashInRoomAsync(Vector2Int currPos, Vector2Int dir) {
        var objs = FindObjectPosInFanShape(currPos, dir, 5, 60f);
        if (objs.Count == 0) return;

        Vector2Int nearest = GetNearestObjectPos(currPos, objs);

        // 向きをアイテム方向へ
        Vector2Int look = nearest - currPos;
        faceDir.SetValue(new Vector2(look.x, look.y));
        dirChangedEvent.Raise();

        await DashToObjectAsync(currPos, nearest);
    }

    private async Task ZDashInCorridorAsync(Vector2Int currPos, Vector2Int dir) {
        // まず 1 歩
        moveHandler.Move(currPos, currPos + dir);
        currPos += dir;

        while (true) {
            var facing = DirectionUtils.GetSurroundingFacingTiles(
                            currPos,
                            DungeonConstants.ToDirection[faceDir.Value.ToVector2Int()]);

            var movable = facing.Where(t => tileManager.CheckMovableTile(currPos, t)).ToList();
            if (movable.Count == 0) break;

            // 部屋の入口など
            if (movable.Count > 1 && tileManager.LookupRoomNum(currPos + dir) != 0) break;

            Vector2Int next = movable[0];
            moveHandler.Move(currPos, next);
            currPos = next;

            await Task.Delay(50);
        }
    }

    /* ───────────────────────────────────────────────
       サーチ / 経路ユーティリティ
    ─────────────────────────────────────────────── */
    private List<Vector2Int> FindObjectPosInFanShape(Vector2Int origin, Vector2Int dir,
                                                     int maxDist, float angleDeg) {
        List<Vector2Int> list = new();
        float halfRad = angleDeg * Mathf.Deg2Rad * 0.5f;

        for (int d = 1; d <= maxDist; d++) {
            int width = Mathf.CeilToInt(d * Mathf.Tan(halfRad));
            Vector2Int perp = CalculatePerpendicularVector(dir);
            bool diag = dir.x != 0 && dir.y != 0;

            Vector2Int main = origin + dir * d;
            CheckPositionForObject(main, list);

            if (diag) CheckDiagonalPositions(origin, dir, d, list);
            CheckSurroundingPositions(origin, main, perp, width, d, list);
        }
        return list;
    }

    private Vector2Int CalculatePerpendicularVector(Vector2Int dir) =>
        dir.x == 0 ? Vector2Int.right
        : dir.y == 0 ? Vector2Int.up
        : new Vector2Int(-dir.y, dir.x);

    private void CheckDiagonalPositions(Vector2Int origin, Vector2Int dir, int dist,
                                        List<Vector2Int> list) {
        CheckPositionForObject(origin + new Vector2Int(dir.x * dist, 0), list);
        CheckPositionForObject(origin + new Vector2Int(0, dir.y * dist), list);
    }

    private void CheckSurroundingPositions(Vector2Int origin, Vector2Int main, Vector2Int perp,
                                           int width, int dist, List<Vector2Int> list) {
        Vector2Int mainDir = dist > 0 ? (main - origin) / dist : Vector2Int.zero;

        for (int w = 1; w <= width; w++) {
            CheckPositionForObject(main + perp * w, list);
            CheckPositionForObject(main - perp * w, list);

            if (dist <= 1) continue;
            for (int d = 1; d < dist; d++) {
                CheckPositionForObject(origin + mainDir * d + perp * w, list);
                CheckPositionForObject(origin + mainDir * d - perp * w, list);
            }
        }
    }

    private void CheckPositionForObject(Vector2Int pos, List<Vector2Int> list) {
        if (tileManager.GetMapChipType(pos) == (int)Constants.MapChipType.Wall) return;

        if (tileManager.CheckExistItem(pos)   != null) list.Add(pos);
        else if (tileManager.CheckExistStair(pos) != null) list.Add(pos);
        else if (tileManager.CheckExistJoint(pos))         list.Add(pos);
    }

    private Vector2Int GetNearestObjectPos(Vector2Int origin, List<Vector2Int> objs) =>
        objs.OrderBy(p => Vector2Int.Distance(origin, p)).First();

    private async Task DashToObjectAsync(Vector2Int start, Vector2Int target) {
        var path = new AStarPathfinding()
                   .FindPath(start, target,
                             tileManager.ExtractAllRoomPositions(objectData.RoomNum.Value));

        Vector2Int curr = start;
        foreach (var p in path) {
            moveHandler.Move(curr, p);
            curr = p;
            await Task.Delay(50);
        }
    }
}
