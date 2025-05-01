using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Tilemaps;

namespace RandomDungeonWithBluePrint {
    [System.Serializable]
    public class TileInfo {

        public Vector2Int position; //移動用のTileのポジション
        public int mapChipType; //移動可否判定用のmapChipType
        public int tileType; //タイルのタイプ　Enemyの移動計算用
        public int roomNum; //Roomの番号

    }
}