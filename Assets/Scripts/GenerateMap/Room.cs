using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RandomDungeonWithBluePrint {
    public class Joint {
        public int Direction;
        public Vector2Int Position;
        public bool Connected;
    }

    public class Room {
        public RectInt Rect;
        public readonly List<Vector2Int> Positions = new List<Vector2Int>();
        public readonly Dictionary<int, List<Vector2Int>> Edge = new Dictionary<int, List<Vector2Int>>();
        public readonly List<Joint> Joints = new List<Joint>();

        public List<Vector2Int> jointPositions { get{return ExtractConnectedJointPositions();} set{jointPositions = value;} } //自作。接続情報の位置
        public int roomNum; //自作
        //public List<GameObject> gameObjects = new List<GameObject>();//自作 CharacterManagerに移管

        public Room(RectInt rect) {
            Rect = rect;
            foreach (var pos in Rect.allPositionsWithin) {
                Positions.Add(pos);
            }

            Edge[Constants.Direction.Left] = Positions.Where(p => p.x == Rect.xMin).ToList();
            Edge[Constants.Direction.Right] = Positions.Where(p => p.x == Rect.xMax - 1).ToList();
            Edge[Constants.Direction.Up] = Positions.Where(p => p.y == Rect.yMin).ToList();
            Edge[Constants.Direction.Down] = Positions.Where(p => p.y == Rect.yMax - 1).ToList();
        }

        public void SetJoint(int direction, Vector2Int position) {
            Joints.Add(new Joint {
                Direction = direction,
                Position = position
            });            
        }

        public IEnumerable<Joint> GetUnconnectedJoints(int direction) {
            return Joints.Where(j => j.Direction == direction && !j.Connected);
        }

        public IEnumerable<Joint> GetConnectedJoints(int direction) {
            return Joints.Where(j => j.Direction == direction && j.Connected);
        }


        //自作
        public List<Vector2Int> ExtractConnectedJointPositions() {
            return Joints.Where(j => j.Connected).Select(j => j.Position).ToList();
        }
    }
}