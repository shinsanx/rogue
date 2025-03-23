using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "RuntimeSet/ObjectData")]
public class ObjectDataRuntimeSet : RuntimeSet<ObjectData> {

    public GameObject GetPlayer() {
        var obj = Items.FirstOrDefault(item => item.Type.Value == "Player");
        if (obj == null) {
            Debug.LogWarning("Playerが見つかりません");
            return null;
        }
        return obj.gameObject;
    }

    public Vector2Int GetPlayerPosition() {
        var obj = Items.FirstOrDefault(item => item.Type.Value == "Player");
        if (obj == null) {
            Debug.LogWarning("Playerが見つかりません");
            return new Vector2Int(0, 0);
        }
        return obj.Position.Value;
    }

    public List<Enemy> GetAllEnemies() {
        var obj = Items.Where(item => item.Type.Value == "Enemy");
        if (obj == null) {
            Debug.LogWarning("Enemyが見つかりません");
            return null;
        }
        return obj.Select(item => item.GetComponent<Enemy>()).ToList();
    }

    public GameObject GetItemFromID(int id) {
        var obj = Items.FirstOrDefault(item => item.Id.Value == id);
        if (obj == null) {
            Debug.LogWarning("アイテムが見つかりません");
            return null;
        }
        return obj.gameObject;
    }

    public List<GameObject> GetObjectsInSameRoom(int roomNum) {
        var obj = Items.Where(item => item.RoomNum.Value == roomNum);
        if (obj == null) {
            Debug.LogWarning("同じ部屋のオブジェクトが見つかりません");
            return null;
        }
        return obj.Select(item => item.gameObject).ToList();
    }

    //EnemyとPlayerを含むオブジェクトを取得する
    public List<GameObject> GetCharacterObjects() {
        var obj = Items.Where(item => item.Type.Value == "Enemy" || item.Type.Value == "Player");
        if (obj == null) {
            Debug.LogWarning("EnemyとPlayerが見つかりません");
            return null;
        }
        return obj.Select(item => item.gameObject).ToList();
    }

    public string GetObjectTypeByPosition(Vector2Int position) {
        var obj = Items.FirstOrDefault(item => item.Position.Value == position);
        if (obj == null) {
            return null;
        }
        return obj.Type.Value;
    }

    public GameObject GetObjectByPosition(Vector2Int position) {
        var obj = Items.FirstOrDefault(item => item.Position.Value == position);
        if (obj == null) {
            return null;
        }
        return obj.gameObject;
    }

    //特定のポジションのPlayerとEnemy以外のオブジェクトを取得する
    public GameObject GetObjectByPositionExceptPlayerAndEnemy(Vector2Int position) {
        var obj = Items.Where(item => item.Position.Value == position && item.Type.Value != "Player" && item.Type.Value != "Enemy");
        if (obj == null) {
            return null;
        }
        return obj.Select(item => item.gameObject).FirstOrDefault();
    }
    
    public ObjectData GetObjectData(int id) {
        return Items.FirstOrDefault(item => item.Id.Value == id);
    }

    public List<ObjectData> GetAllObjectData() {
        return Items;
    }

}
