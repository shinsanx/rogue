using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace RandomDungeonWithBluePrint {
    public class TileInfo {

        public Vector2 position; //移動用のTileのポジション
        public int mapChipType; //移動可否判定用のmapChipType
        public int tileType; //タイルのタイプ　Enemyの移動計算用
        public int objType; //タイル上にいる物体のGameObject
        public GameObject objectOnTile; //自作。タイル上にいる物体のGameObject
        private List<List<TileInfo>> tiles = new List<List<TileInfo>>(); //xとy
        public TileInfo this[int x, int y] => tiles[y][x]; //インデクサ。インスタンスを生成した時にListみたいに取得できる。
        public Vector2Int mapSize;

        public void Build(Vector2Int size, List<Room> rooms, List<Vector2Int> branches) {
            mapSize = size;
            MakeTileList(size.x, size.y);

            for (var i = 0; i < size.y; i++) {
                for (var j = 0; j < size.x; j++) {
                    tiles[i][j].mapChipType = (int)Constants.MapChipType.Wall;//いったんすべてwallにする
                }
            }

            int roomCount = 1;
            foreach (Room room in rooms) {
                foreach (Vector2Int pos in room.Rect.allPositionsWithin) {//矩形領域内のすべての座標を表示
                    tiles[pos.y][pos.x].mapChipType = (int)Constants.MapChipType.Floor;//Roomの座標をFloorにする
                }
                room.roomNum = roomCount; //自作。roomNum生成のために使用する。
                roomCount++;
            }

            //道の作成。branchとして保存されている座標をFloorにする
            foreach (var branch in branches.Distinct()) {//重複を削除した座標
                tiles[branch.y][branch.x].mapChipType = (int)Constants.MapChipType.Floor;
                tiles[branch.y][branch.x].tileType = (int)Constants.TileType.Aisle;

            }
            MakePosition(size.x, size.y);
            ResetObjType();
        }

        //自作 Tileの盤面を作成。
        private void MakeTileList(int x, int y) {
            for (var i = 0; i < y; i++) {
                tiles.Add(new List<TileInfo>());
                for (var j = 0; j < x; j++) {
                    tiles.Last().Add(new TileInfo());
                }
            }
        }

        //positionを生成。
        private void MakePosition(int x, int y) {
            for (int i = 0; i < y; i++) {
                for (int j = 0; j < x; j++) {
                    tiles[i][j].position = new Vector2Int(j, i);
                }
            }
        }

        //ObjTypeを0に統一
        public void ResetObjType() {
            int x = mapSize.x;
            int y = mapSize.y;
            for (int i = 0; i < y; i++) {
                for (int j = 0; j < x; j++) {
                    tiles[i][j].objType = (int)DungeonConstants.ObjTypelnTile.Nothing;
                }
            }
        }
    }
}