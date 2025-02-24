using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Data.Common;

public class CharacterManager : MonoBehaviour {
    private static CharacterManager _i;
    public static CharacterManager i {
        get {
            if (_i == null) {
                var obj = new GameObject("CharacterManager");
                _i = obj.AddComponent<CharacterManager>();
            }
            return _i;
        }
    }

    //ここにすべてのオブジェクトデータが格納される    
    public List<IObjectData> allObjectData = new List<IObjectData>();    

    //IDの管理
    private static int _idCounter = 0;

    public static int GetUniqueID() {
        return ++_idCounter;
    }

    //IDをリセットする（必要があれば）
    public static void ResetID() {
        _idCounter = 0;

    }



    // オブジェクトの情報を更新する
    private void UpdateObjectInfo(IObjectData objectData) {
        for (int i = 0; i < allObjectData.Count; i++) {
            if (allObjectData[i].Id == objectData.Id) {
                allObjectData[i] = objectData;
                //  Debug.Log($"Updated object: {objectData.Name} with Pos: {objectData.Position}");                
                return;
            }
        }
        Debug.LogWarning($"Object with ID: {objectData.Id} not found.");
    }

    // キャラクターを追加するメソッド
    public void AddCharacter(IObjectData character) {
        allObjectData.Add(character);
        character.OnObjectUpdated += UpdateObjectInfo;
        //   Debug.Log($"registered: {character.Name}");
        //   Debug.Log($"ID: {character.Id}");
    }

    // キャラクターを削除するメソッド
    public void RemoveCharacter(IObjectData character) {
        character.OnObjectUpdated -= UpdateObjectInfo;
        allObjectData.Remove(character);
        Debug.Log($"unregistered: {character.Name}");
    }


    // 指定された位置にあるオブジェクトを取得する    
    public GameObject GetObjectByPosition(Vector2Int position) {
        // allObjectData から一致する IObjectData を検索
        var matchingObject = allObjectData.FirstOrDefault(obj => obj.Position == position);

        if (matchingObject != null) {
            // IObjectData から GameObject を取得 (キャストが必要)
            MonoBehaviour matchingGameObject = matchingObject as MonoBehaviour;
            if (matchingGameObject != null) {
                return matchingGameObject.gameObject;
            }
        }

        // 一致するオブジェクトがない場合は null を返す
        // Debug.LogWarning($"No object found at position: {position}");
        return null;
    }

    //positionから存在するオブジェクトのタイプを返す
    public string GetObjectTypeByPosition(Vector2Int position) {
        var obj = allObjectData.FirstOrDefault(obj => obj.Position == position);
        if (obj == null) {
            // Debug.Log($"{position}にはタイプが見つかりません");
            return null;
        }
        return obj.Type;
    }

    //指定された部屋番号のオブジェクトを取得する
    public List<GameObject> GetObjectsInSameRoom(int roomNum) {        
        IEnumerable<IObjectData> objectsInRoom = allObjectData.Where(obj => obj.RoomNum == roomNum);

        // MonoBehaviourにキャストしてGameObjectを取得
        IEnumerable<GameObject> gameObjects = objectsInRoom.Select(data => {
            MonoBehaviour monoBehaviour = data as MonoBehaviour;
            return monoBehaviour?.gameObject;
        });

        // nullでないものだけをリストにして返す
        return gameObjects.Where(obj => obj != null).ToList();
    }

    //プレイヤーを取得する
    public GameObject GetPlayer() {
        var obj = allObjectData.FirstOrDefault(obj => obj.Type == "Player");
        if(obj != null) {
            MonoBehaviour monoBehaviour = obj as MonoBehaviour;            
            return monoBehaviour?.gameObject;
        }
        Debug.LogWarning("Playerが見つかりません");
        return null;
    }

    //すべてのEnemyを取得する
    public List<Enemy> GetAllEnemies() {
        // Enemyタイプのオブジェクトのみを抽出
        var enemyObjects = allObjectData.Where(obj => obj.Type == "Enemy");
        
        // Enemyクラスにキャスト
        var enemies = enemyObjects.Select(obj => obj as Enemy);
        
        // nullでないものだけを取得
        var validEnemies = enemies.Where(enemy => enemy != null);
        
        return validEnemies.ToList();
    }

    public void Initialize() {
        allObjectData = new List<IObjectData>();
    }

}