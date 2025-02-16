using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Tilemaps;

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
        private Dictionary<int, List<Vector2Int>> tilePositionsByRoomNum;
        private Dictionary<Vector2Int, int> roomNumByPosition;
        private Dictionary<int, Room> roomByNum;

        // コンストラクタで辞書を初期化（オプション）
        public TileInfo() {
            mapChipTypeByPosition = new Dictionary<Vector2Int, int>();
            tileTypeByPosition = new Dictionary<Vector2Int, int>();
            tilePositionsByRoomNum = new Dictionary<int, List<Vector2Int>>();
            roomNumByPosition = new Dictionary<Vector2Int, int>();
            roomByNum = new Dictionary<int, Room>();
        }

        public void Build(Vector2Int size, List<Room> rooms, List<Vector2Int> branches) {
            mapSize = size;
            MakeTileList(size.x, size.y);
            MakePosition(size.x, size.y);            

            // 辞書の初期化（コンストラクタで初期化していない場合）
            if (mapChipTypeByPosition == null) {
                mapChipTypeByPosition = new Dictionary<Vector2Int, int>();
            }
            if (tileTypeByPosition == null) {
                tileTypeByPosition = new Dictionary<Vector2Int, int>();
            }
            if (tilePositionsByRoomNum == null) {
                tilePositionsByRoomNum = new Dictionary<int, List<Vector2Int>>();
            }
            if (roomNumByPosition == null) {
                roomNumByPosition = new Dictionary<Vector2Int, int>();
            }
            if (roomByNum == null) {
                roomByNum = new Dictionary<int, Room>();
            }

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
                    tiles[pos.y][pos.x].roomNum = roomCount; //自作。roomNum生成のために使用する。
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

            // roomNumごとのタイルポジションListを辞書に格納
            foreach (TileInfo tile in tiles.SelectMany(row => row)) {
                if (!tilePositionsByRoomNum.ContainsKey(tile.roomNum)) {
                    tilePositionsByRoomNum[tile.roomNum] = new List<Vector2Int>();
                }                
                tilePositionsByRoomNum[tile.roomNum].Add(tile.position);
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

        // roomNumから全タイルポジションを取得するメソッド
        public List<Vector2Int> GetTilePositionsByRoomNum(int roomNum) {
            if (tilePositionsByRoomNum.TryGetValue(roomNum, out var tilePositions)) {
                return tilePositions;
            }
            return new List<Vector2Int>();
        }

        //positionからroomNumを取得するメソッド
        public int GetRoomNumByPosition(Vector2Int position) {
            if (roomNumByPosition.TryGetValue(position, out var roomNum)) {
                return roomNum;
            }
            return 0;
        }

        //roomNumからRoomを取得するメソッド
        public Room GetRoomByNum(int roomNum) {
            if (roomByNum.TryGetValue(roomNum, out var room)) {
                return room;
            }
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