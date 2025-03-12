using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace RandomDungeonWithBluePrint {


    [CreateAssetMenu(fileName = "TileSet", menuName = "RuntimeSet/TileSet")]
    public class TileSet : ScriptableObject {
        [SerializeField] public List<List<TileInfo>> tileInfos;
        public Vector2Int mapSize;
        public List<Room> rooms;
        public List<Vector2Int> branches;

        public void Clear() {
            tileInfos.Clear();
        }

        // 新たに追加する辞書
        private Dictionary<Vector2Int, int> mapChipTypeByPosition = new Dictionary<Vector2Int, int>();
        private Dictionary<Vector2Int, int> tileTypeByPosition = new Dictionary<Vector2Int, int>();
        private Dictionary<int, List<Vector2Int>> tilePositionsByRoomNum = new Dictionary<int, List<Vector2Int>>();
        private Dictionary<Vector2Int, int> roomNumByPosition = new Dictionary<Vector2Int, int>();
        private Dictionary<int, Room> roomByNum = new Dictionary<int, Room>();

        public void Build(Vector2Int size, List<Room> rooms, List<Vector2Int> branches) {
            Debug.Log("BuildTileSet");
            tileInfos = new List<List<TileInfo>>();
            mapSize = size;
            this.rooms = rooms;
            this.branches = branches;
            MakeTileList(size.x, size.y);
            MakePosition(size.x, size.y);

            // 辞書の初期化            
            mapChipTypeByPosition = new Dictionary<Vector2Int, int>();
            tileTypeByPosition = new Dictionary<Vector2Int, int>();
            tilePositionsByRoomNum = new Dictionary<int, List<Vector2Int>>();
            roomNumByPosition = new Dictionary<Vector2Int, int>();
            roomByNum = new Dictionary<int, Room>();

            for (var i = 0; i < size.y; i++) {
                for (var j = 0; j < size.x; j++) {
                    tileInfos[i][j].mapChipType = (int)Constants.MapChipType.Wall;//いったんすべてwallにする
                    mapChipTypeByPosition[tileInfos[i][j].position] = tileInfos[i][j].mapChipType; //辞書に格納
                }
            }

            int roomCount = 1;
            foreach (Room room in rooms) {
                foreach (Vector2Int pos in room.Rect.allPositionsWithin) {//矩形領域内のすべての座標を表示
                    tileInfos[pos.y][pos.x].mapChipType = (int)Constants.MapChipType.Floor;//Roomの座標をFloorにする

                    //キーが重複した場合は上書き
                    if (mapChipTypeByPosition.ContainsKey(pos)) {
                        mapChipTypeByPosition[pos] = tileInfos[pos.y][pos.x].mapChipType; //辞書に格納                        
                    } else {
                        mapChipTypeByPosition.Add(pos, tileInfos[pos.y][pos.x].mapChipType); //辞書に格納
                    }
                    tileInfos[pos.y][pos.x].roomNum = roomCount; //自作。roomNum生成のために使用する。
                }
                room.roomNum = roomCount; //自作。roomNum生成のために使用する。
                roomByNum[roomCount] = room;
                roomCount++;
            }

            //道の作成。branchとして保存されている座標をFloorにする
            foreach (var branch in branches.Distinct()) {//重複を削除した座標
                tileInfos[branch.y][branch.x].mapChipType = (int)Constants.MapChipType.Floor;
                tileInfos[branch.y][branch.x].tileType = (int)Constants.TileType.Aisle;
                mapChipTypeByPosition[branch] = tileInfos[branch.y][branch.x].mapChipType; //辞書に格納
                tileTypeByPosition[branch] = tileInfos[branch.y][branch.x].tileType; //辞書に格納
            }

            //辞書にroomNumを設定
            foreach (Room room in rooms) {
                foreach (Vector2Int pos in room.Rect.allPositionsWithin) {
                    roomNumByPosition[pos] = room.roomNum;
                }
            }

            // roomNumごとのタイルポジションListを辞書に格納
            foreach (TileInfo tile in tileInfos.SelectMany(row => row)) {
                if (!tilePositionsByRoomNum.ContainsKey(tile.roomNum)) {
                    tilePositionsByRoomNum[tile.roomNum] = new List<Vector2Int>();
                }
                tilePositionsByRoomNum[tile.roomNum].Add(tile.position);
            }
        }



        //自作 Tileの盤面を作成。
        private void MakeTileList(int x, int y) {
            for (var i = 0; i < y; i++) {
                tileInfos.Add(new List<TileInfo>());
                for (var j = 0; j < x; j++) {
                    tileInfos.Last().Add(new TileInfo());
                }
            }
        }

        //positionを生成。
        private void MakePosition(int x, int y) {
            for (int i = 0; i < y; i++) {
                for (int j = 0; j < x; j++) {
                    tileInfos[i][j].position = new Vector2Int(j, i);
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



