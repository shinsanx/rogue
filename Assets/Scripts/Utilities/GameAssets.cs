using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameAssets : MonoBehaviour {
    private static GameAssets _i;
    public static GameAssets i {
        get {
            if (_i == null) {
                var prefab = Resources.Load<GameAssets>("GameAssets");
                _i = Instantiate(prefab);
                DontDestroyOnLoad(_i.gameObject);
            }
            return _i;
        }
    }

    public StateMachine stateMachine;
    public State playerState;
    public State enemyState;
    public RandomDungeonWithBluePrint.TileSet tileSet;

    private void Awake() {
        // 予期せぬシーン内重複対策（保険）
        if (_i != null && _i != this) {
            Destroy(gameObject);
            return;
        }
        _i = this;
    }
}