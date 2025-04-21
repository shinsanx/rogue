using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDirectionHandler {
    private readonly Vector2Variable faceDir;
    private readonly GameEvent dirChangedEvent;

    public PlayerDirectionHandler(Vector2Variable playerFaceDirection,
                                  GameEvent onDirChanged) {
        faceDir = playerFaceDirection;
        dirChangedEvent = onDirChanged;
    }

    /* ───── 手動ターン ───── */
    public void ManualTurn(Vector2 rawInput) {
        Vector2 rounded = new(Mathf.Round(rawInput.x), Mathf.Round(rawInput.y));
        faceDir.SetValue(rounded);
        dirChangedEvent.Raise();
    }

    /* ───── オートターン ───── */
    public void AutoTurn(Vector2Int playerPos, TileManager tile) {
        foreach (var obj in tile.GetSurroundingObjects(playerPos)) {
            var enemy = obj.GetComponent<Enemy>();
            if (enemy == null) continue;

            Vector2 dir = enemy.objectData.Position.Value - playerPos;
            faceDir.SetValue(dir);
            dirChangedEvent.Raise();
            break;              // １体見つけたら終了
        }
    }
}
