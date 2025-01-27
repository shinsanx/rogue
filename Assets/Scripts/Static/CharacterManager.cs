using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _i;
    public static CharacterManager i{
        get {
            if(_i == null) {
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

    public static int GetUniqueID(){
        return ++_idCounter;
    }

    //IDをリセットする（必要があれば）
    public static void ResetID(){
        _idCounter = 0;
    }

    private void OnEnable() {
        /* //CharacterManagerが生成されたときに、全キャラクターがいない可能性がある
        // 各キャラクターの更新イベントを購読
        foreach (var obj in allObjectData)
        {
            obj.OnObjectUpdated += UpdateObjectInfo;
        }
        */
    }
    
    
    // オブジェクトの情報を更新する
    private void UpdateObjectInfo(IObjectData objectData){
        //IDが一致するオブジェクトを探して更新する
        //if(objectData.Id == )
    }

    // キャラクターを追加するメソッド
    public void AddCharacter(IObjectData character)
    {
        allObjectData.Add(character);
        character.OnObjectUpdated += UpdateObjectInfo;
        Debug.Log($"registered: {character.Name}");
        Debug.Log($"ID: {character.Id}");
    }

    // キャラクターを削除するメソッド
    public void RemoveCharacter(IObjectData character)
    {
        character.OnObjectUpdated -= UpdateObjectInfo;
        allObjectData.Remove(character);
        Debug.Log($"unregistered: {character.Name}");
    }
}
