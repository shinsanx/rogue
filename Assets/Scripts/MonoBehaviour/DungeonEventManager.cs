using System.Collections;
using System.Collections.Generic;
using RandomDungeonWithBluePrint;
using UnityEngine;

public class DungeonEventManager : MonoBehaviour
{
    //Todo: マップ生成
    //プレイヤーをランダムな位置へ召喚
    //アイテムの生成
    //モンスターの生成                    

    [SerializeField] RandomMapGenerator randomMapGenerator;
    [SerializeField] AutoMapping autoMapping;
    [SerializeField] Player player;
    [SerializeField] GameObject enemyParent;
    
    void Start()
    {
        // ミニマップの初期化
        autoMapping.Initialize();
        // マップ生成
        randomMapGenerator.Initialize();

        // プレイヤーをランダムな位置へ召喚
        player.InitializePlayer();
        player.GetComponent<IObjectData>().Position = TileManager.i.GetRandomPosition();
        // モンスターの生成
        // アイテムの生成
        // ミニマップの生成
        randomMapGenerator.CreateMiniMap();
        // StateMachineの初期化

                

    }
    
}
