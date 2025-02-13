using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace RandomDungeonWithBluePrint {
    public class TileInfo {

        public Vector2Int position; //移動用のTileのポジション
        public int mapChipType; //移動可否判定用のmapChipType
        public int tileType; //タイルのタイプ　Enemyの移動計算用  
        public int roomNum; //Roomの番号
        private List<List<TileInfo>> tiles = new List<List<TileInfo>>(); //xとy
        public TileInfo this[int x, int y] => tiles[y][x]; //インデクサ。インスタンスを生成した時にListみたいに取得できる。
        public Vector2Int mapSize;

        // 新たに追加する辞書
        private Dictionary<Vector2Int, int> mapChipTypeByPosition;
        private Dictionary<Vector2Int, int> tileTypeByPosition;
        private Dictionary<int, List<TileInfo>> tilesByRoomNum;
        private Dictionary<Vector2Int, int> roomNumByPosition;
        private Dictionary<int, Room> roomByNum;

        public void Build(Vector2Int size, List<Room> rooms, List<Vector2Int> branches) {
            mapSize = size;
            MakeTileList(size.x, size.y);

            for (var i = 0; i < size.y; i++) {
                for (var j = 0; j < size.x; j++) {
                    tiles[i][j].mapChipType = (int)Constants.MapChipType.Wall;//いったんすべてwallにする
                    mapChipTypeByPosition[tiles[i][j].position] = tiles[i][j].mapChipType; //辞書に格納
                }
            }

            int roomCount = 1;
            foreach (Room room in rooms) {
                foreach (Vector2Int pos in room.Rect.allPositionsWithin) {//矩形領域内のすべての座標を表示
                    tiles[pos.y][pos.x].mapChipType = (int)Constants.MapChipType.Floor;//Roomの座標をFloorにする
                    
                    //キーが重複した場合は上書き
                    if (mapChipTypeByPosition.ContainsKey(pos)) {
                        mapChipTypeByPosition[pos] = tiles[pos.y][pos.x].mapChipType; //辞書に格納
                    } else {
                        mapChipTypeByPosition.Add(pos, tiles[pos.y][pos.x].mapChipType); //辞書に格納
                    }
                }
                room.roomNum = roomCount; //自作。roomNum生成のために使用する。
                roomByNum[roomCount] = room;
                roomCount++;
            }

            //道の作成。branchとして保存されている座標をFloorにする
            foreach (var branch in branches.Distinct()) {//重複を削除した座標
                tiles[branch.y][branch.x].mapChipType = (int)Constants.MapChipType.Floor;
                tiles[branch.y][branch.x].tileType = (int)Constants.TileType.Aisle;
                mapChipTypeByPosition[branch] = tiles[branch.y][branch.x].mapChipType; //辞書に格納
                tileTypeByPosition[branch] = tiles[branch.y][branch.x].tileType; //辞書に格納
            }

            //辞書にroomNumを設定
            foreach (Room room in rooms) {
                foreach (Vector2Int pos in room.Rect.allPositionsWithin) {                    
                    roomNumByPosition[pos] = room.roomNum;
                }
            }

            MakePosition(size.x, size.y);            

            // roomNumごとのタイルListを辞書に格納
            tilesByRoomNum = new Dictionary<int, List<TileInfo>>();
            foreach (var tile in tiles.SelectMany(row => row)) {
                if (!tilesByRoomNum.ContainsKey(tile.roomNum)) {
                    tilesByRoomNum[tile.roomNum] = new List<TileInfo>();
                }
                tilesByRoomNum[tile.roomNum].Add(tile);
            }
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

        //ここから辞書を使用したメソッド
        //TileManagerで使用するメソッド

        // roomNumからタイルを取得するメソッド
        public List<TileInfo> GetTilesByRoomNum(int roomNum) {
            if (tilesByRoomNum.TryGetValue(roomNum, out var tileList)) {
                return tileList;
            }
            return new List<TileInfo>();
        }

        //positionからroomNumを取得するメソッド
        public int GetRoomNumByPosition(Vector2Int position) {
            if (roomNumByPosition.TryGetValue(position, out var roomNum)) {
                return roomNum;
            }
            Debug.Log("roomNumが見つかりませんでした。position:" + position);
            return 0;
        }

        //roomNumからRoomを取得するメソッド
        public Room GetRoomByNum(int roomNum) {
            if (roomByNum.TryGetValue(roomNum, out var room)) {
                return room;
            }
            Debug.Log("roomNumが見つかりませんでした。roomNum:" + roomNum);
            return null;
        }

        //positionからmapChipTypeを取得するメソッド
        public int GetMapChipTypeByPosition(Vector2Int position) {
            if (mapChipTypeByPosition.TryGetValue(position, out var mapChipType)) {
                return mapChipType;
            }
            return 0;
        }

        //positionからtileTypeを取得するメソッド
        public int GetTileTypeByPosition(Vector2Int position) {
            if (tileTypeByPosition.TryGetValue(position, out var tileType)) {
                return tileType;
            }
            return 0;
        }
    }
}