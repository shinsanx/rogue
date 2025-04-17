using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStairHandler {
    private TileManager tileManager;
    public PlayerStairHandler(TileManager tm){ tileManager = tm; }

    public void TryUseStair(Vector2Int pos){
        var stair = tileManager.CheckExistStair(pos);
        stair?.GetComponent<IMenuActionAdapter>()?.OnSelected();
    }
}

