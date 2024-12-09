using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RandomDungeonWithBluePrint {
    public class FieldView : MonoBehaviour {

        [SerializeField] private Tilemap tilemap = default;
        [SerializeField] private Tile floorTile = default;
        [SerializeField] private Tile wallTile = default;
        [SerializeField] private Tile debug = default;
        [SerializeField] private Tile up = default;
        [SerializeField] private Tile down = default;
        [SerializeField] private Tile left = default;
        [SerializeField] private Tile right = default;
        [SerializeField] private Tile downUp = default;
        [SerializeField] private Tile leftRight = default;
        [SerializeField] private Tile rightUp = default;
        [SerializeField] private Tile downRight = default;
        [SerializeField] private Tile downLeft = default;
        [SerializeField] private Tile leftUp = default;
        [SerializeField] private Tile downRightUp = default;
        [SerializeField] private Tile downLeftRight = default;
        [SerializeField] private Tile downLeftUp = default;
        [SerializeField] private Tile leftRightUp = default;




        public void ShowField(Field field) {
            tilemap.ClearAllTiles();

            for (var x = 0; x < field.Grid.Size.x; x++) {
                for (var y = 0; y < field.Grid.Size.y; y++) {
                    switch (field.Grid[x, y]) {
                        case (int)Constants.MapChipType.Debug:
                            tilemap.SetTile(new Vector3Int(x, y, 0), debug);
                            break;
                            case (int)Constants.MapChipType.Wall:
                            tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                            break;
                            case (int)Constants.MapChipType.Floor:
                            tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                            break;
                            case (int)Constants.MapChipType.Up:
                            tilemap.SetTile(new Vector3Int(x, y, 0), up);
                            break;
                            case (int)Constants.MapChipType.Down:
                            tilemap.SetTile(new Vector3Int(x, y, 0), down);
                            break;
                            case (int)Constants.MapChipType.Left:
                            tilemap.SetTile(new Vector3Int(x, y, 0), left);
                            break;
                            case (int)Constants.MapChipType.Right:
                            tilemap.SetTile(new Vector3Int(x, y, 0), right);
                            break;
                            case (int)Constants.MapChipType.DownUp:
                            tilemap.SetTile(new Vector3Int(x, y, 0), downUp);
                            break;
                            case (int)Constants.MapChipType.LeftRight:
                            tilemap.SetTile(new Vector3Int(x, y, 0), leftRight);
                            break;
                            case (int)Constants.MapChipType.RightUp:
                            tilemap.SetTile(new Vector3Int(x, y, 0), rightUp);
                            break;
                            case (int)Constants.MapChipType.DownRight:
                            tilemap.SetTile(new Vector3Int(x, y, 0), downRight);
                            break;
                            case (int)Constants.MapChipType.DownLeft:
                            tilemap.SetTile(new Vector3Int(x, y, 0), downLeft);
                            break;
                            case (int)Constants.MapChipType.LeftUp:
                            tilemap.SetTile(new Vector3Int(x, y, 0), leftUp);
                            break;
                            case (int)Constants.MapChipType.DownRightUp:
                            tilemap.SetTile(new Vector3Int(x, y, 0), downRightUp);
                            break;
                            case (int)Constants.MapChipType.DownLeftRight:
                            tilemap.SetTile(new Vector3Int(x, y, 0), downLeftRight);
                            break;
                            case (int)Constants.MapChipType.DownLeftUp:
                            tilemap.SetTile(new Vector3Int(x, y, 0), downLeftRight);
                            break;
                            case (int)Constants.MapChipType.LeftRightUp:
                            tilemap.SetTile(new Vector3Int(x, y, 0), leftRightUp);
                            break;
                    }
                }
                
            }
            //OutputPosition(tilemap);
        }

        private void OutputPosition(Tilemap map) {
            BoundsInt bound = map.cellBounds;
            for(int y = bound.min.y; y <bound.max.y; ++y){
                for(int x = bound.min.x; x < bound.max.x; ++x){
                    Debug.Log(new Vector3Int(x,y));
                }
            }
        }
    }
}