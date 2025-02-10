using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class DungeonConstants {
    public enum ObjTypelnTile {
        Nothing,
        Player,
        Enemy,
        Item,
        Trap,
        Stairs
    }

    //メッセージバスのPublish用string
    public const string sendMessage = "sendMessage";
    //メッセージダイアログを送信する。PlayerとEnemyが使用
    public const string UpdateHPText = "UpdateHPText";
    //PlayerのHP変更時のUIテキストを変更する。 UILogicが使用 
    public const string UpdateLvText = "UpdateLvText";
    //PlayerのレベルUP時のUIテキストを変更する。UILogicが使用
    public const string GetExp = "GetExp";
    //敵が倒れたとき、PlayerにExpが入るようにする。Enemyが使用 
    public const string NotifyEnemyAct = "NotifyEnemyAct";
    //敵が行動したときにDungeonManagerLogicに通知する。
    public const string CreateTakeDamageMessage = "CreateTakeDamageMessage";
    //プレイヤーがダメージを受けた時のメッセージ
    public const string GetCurrentField = "GetCurrentField"; //Random MapTestより。Fieldを返す. 
    public const string GetPlayerPosition = "GetPlayerPosition"; //DungeonEventLogic プレイヤーのポジションを取得する

    public const int Right = 0;
    public const int UpRight = 45;
    public const int Up = 90;
    public const int UpLeft = 135;
    public const int Left = 180;
    public const int DownLeft = 225; public const int Down = 270;
    public const int DownRight = 315;

    public enum Direction {
        Stay,
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest
    }


    public static readonly Dictionary<Vector2Int, Direction> ToDirection = new Dictionary<Vector2Int, Direction> {
        {new Vector2Int(0, 0), Direction.Stay},
        {new Vector2Int(0,1), Direction.North},
        {new Vector2Int(0, -1), Direction.South},
        {new Vector2Int(1, 0), Direction.East},
        {new Vector2Int(-1, 0), Direction.West},
        {new Vector2Int(1, 1), Direction.NorthEast},
        {new Vector2Int(-1, 1), Direction.NorthWest},
        {new Vector2Int(1, -1), Direction.SouthEast},
        {new Vector2Int(-1, -1), Direction.SouthWest}
    };

    public static readonly int[] EightDirections = {
        Right,
        Up,
        Left,
        Down,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    };

    public static readonly Dictionary<int, Vector2Int> ToVector2Int = new Dictionary<int, Vector2Int> {
        {Left, new Vector2Int(-1, 0)},
        {Down, new Vector2Int(0, -1)},
        {Up, new Vector2Int(0, 1)},
        {Right, new Vector2Int(1, 0)},
        {UpLeft, new Vector2Int(-1, 1)},
        {UpRight, new Vector2Int(1, 1)},
        {DownLeft, new Vector2Int(-1, -1)},
        {DownRight, new Vector2Int(1, -1)},
    };
    public static Dictionary<int, int> necessarryExp = new Dictionary<int, int>(){
        {1, 0},
        {2, 10},
        {3, 40},
        {4, 100},
        {5, 200},
        {6, 400},
        {7, 700}, 
        {8, 1000}, 
        {9, 1300},
        {10, 1800}, 
        {11, 2400}, 
        {12, 3000}, 
        {13, 3600},

    };
}