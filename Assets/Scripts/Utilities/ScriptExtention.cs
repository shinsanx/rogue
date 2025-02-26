using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public static class ScriptExtention {

    public static Dictionary<int, string> lowerUpperNum;

    public static string ConvertNumToUpperString(this string self, int tNum){

        if(lowerUpperNum == null) {
            lowerUpperNum = new Dictionary<int, string>(){
                {0, "０"},
                {1, "１"},
                {2, "２"},
                {3, "３"},
                {4, "４"},
                {5, "５"},
                {6, "６"},
                {7, "７"},
                {8, "８"},
                {9, "９"},
            };
        }

        string updateText = tNum.ToString();
        for(int i = 0; i<10;i++){
            string iString = i.ToString();
            updateText = updateText.Replace(iString, lowerUpperNum[i]);
        }
        return updateText;
    }

    public static Vector2Int ToVector2Int(this Vector2 self){
        Vector2Int vectorInt = new Vector2Int(Mathf.FloorToInt(self.x),Mathf.FloorToInt(self.y));
        return vectorInt;
    }

    public static Vector2Int ToVector2Int(this Vector3 self){
        Vector2Int vectorInt = new Vector2Int(Mathf.FloorToInt(self.x),Mathf.FloorToInt(self.y));
        return vectorInt;
    }

    public static Vector2 ToVector2(this Vector2Int self){
        Vector2 vector = new Vector2(self.x, self.y);
        return vector;
    }
}
