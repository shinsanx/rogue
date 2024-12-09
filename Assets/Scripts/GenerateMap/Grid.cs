using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomDungeonWithBluePrint {
    public class Grid {
        public Vector2Int Size => new Vector2Int(grid.First().Count, grid.Count);
        private readonly List<List<int>> grid = new List<List<int>>();
        public int this[int x, int y] => grid[y][x];

        public void Build(Vector2Int size, List<Room> rooms, List<Vector2Int> branches) {
            MakeGrid(size.x, size.y);
            for (var i = 0; i < size.y; i++) {
                for (var j = 0; j < size.x; j++) {
                    grid[i][j] = (int)Constants.MapChipType.Wall;
                }
            }

            foreach (var room in rooms) {
                foreach (var pos in room.Rect.allPositionsWithin) {
                    grid[pos.y][pos.x] = (int)Constants.MapChipType.Floor;
                }
            }

            foreach (var branch in branches.Distinct()) {
                grid[branch.y][branch.x] = (int)Constants.MapChipType.Floor;
            }
        }

        private void MakeGrid(int x, int y) {
            for (var i = 0; i < y; i++) {
                grid.Add(new List<int>());
                for (var j = 0; j < x; j++) {
                    grid.Last().Add(0);
                }
            }
        }

        //自作
        private void CheckSurroundingGridType() {
            int left = -1;
            int right = 1;
            int up = 1;
            int down = -1;
            List<string> directionNoWall = new List<string>();

            for(int i = 0; i <Size.y; i++){
                for(int j = 0; j<Size.x; j++){
                    directionNoWall.Clear();

                    if(grid[i][j] == 1) {
                        if(i + up < Size.y){
                            directionNoWall.Add((grid[i +up][j]==0)? "Up":null);
                        }
                        if(i >0){
                            directionNoWall.Add((grid[i +down][j]==0)? "Down":null);
                        }
                        if(j >0){
                            directionNoWall.Add((grid[i][j+left]==0)? "Left":null);
                        }
                        if(j + right < Size.x &&(grid[i][j + right]==0)){
                            directionNoWall.Add("Right");
                        }
                        string str = string.Join("", directionNoWall.OrderBy(d => d));
                        ConvertGridTypeByWallDirectionForNoWallGrid(i,j,str);
                    }
                }
            }
        }

        private void ConvertGridTypeByWallDirectionForNoWallGrid(int y, int x, string direction){
            switch(direction){
                case "Up":
                grid[y][x] = (int)Constants.MapChipType.Up;
                break;
                case "Right":
                grid[y][x] = (int)Constants.MapChipType.Right;
                break;
                case "Down":
                grid[y][x] = (int)Constants.MapChipType.Down;
                break;
                case "Left":
                grid[y][x] = (int)Constants.MapChipType.Left;
                break;
                case "DownUp":
                grid[y][x] = (int)Constants.MapChipType.DownUp;
                break;
                case "LeftRight":
                grid[y][x] = (int)Constants.MapChipType.LeftRight;
                break;
                case "RightUp":
                grid[y][x] = (int)Constants.MapChipType.RightUp;
                break;
                case "DownRight":
                grid[y][x] = (int)Constants.MapChipType.DownRight;
                break;
                case "DownLeft":
                grid[y][x] = (int)Constants.MapChipType.DownLeft;
                break;
                case "LeftUp":
                grid[y][x] = (int)Constants.MapChipType.LeftUp;
                break;
                case "DownRightUp":
                grid[y][x] = (int)Constants.MapChipType.DownRightUp;
                break;
                case "DownLeftRight":
                grid[y][x] = (int)Constants.MapChipType.DownLeftRight;
                break;
                case "DownLeftUp":
                grid[y][x] = (int)Constants.MapChipType.DownLeftUp;
                break;
                case "LeftRightUp":
                grid[y][x] = (int)Constants.MapChipType.LeftRightUp;
                break;
            }
        }
    }


}