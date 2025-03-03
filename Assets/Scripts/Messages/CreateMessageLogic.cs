using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CreateMessageLogic {




    private string upperDamageText = null;
    private string playerName = "トルネコ";


    //Enemyがダメージを受けたときのメッセージ
    public List<string> CreateAttackMessage(List<string> strings, int damage, string dealerName, string takerName) {
        string firstText = dealerName + "は、" + takerName + "に";
        string secondText = upperDamageText.ConvertNumToUpperString(damage) + "ポイントのダメージを与えた。";

        strings.Add(firstText);
        strings.Add(secondText);
        return strings;
    }

    //プレイヤーによって倒されたときのメッセージ（Expつき）
    public List<string> CreateDefeatedMessage(List<string> strings, string takerName, int exp) {
        string firstText = takerName + "をやっつけた。";
        string secondText = upperDamageText.ConvertNumToUpperString(exp) + "ポイントの経験値を得た。";

        strings.Add(firstText);
        strings.Add(secondText);
        return strings;
    }

    //レベルが上がったとき
    public List<string> LvUppedMessage(string playerName, int lv) {
        string firstText = playerName + "はレベル" + upperDamageText.ConvertNumToUpperString(lv) + "に上がった。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    //プレイヤーがダメージを受けたとき
    public List<string> CreateTakeDamageMessage(List<string> strings, int damage, string dealerName) {
        string firstText = dealerName + "から";
        string secondText = upperDamageText.ConvertNumToUpperString(damage) + "ポイントのダメージを受けた。";

        strings.Add(firstText);
        strings.Add(secondText);
        return strings;
    }

    //アイテムを使用したとき
    public List<string> CreateUseItemMessage(string itemName) {
        string firstText = playerName + "は" + itemName + "を使用した。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    //アイテムを拾ったとき
    public List<string> CreateGetItemMessage(string itemName) {
        string firstText = itemName + "を拾った。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    //アイテムに乗った時
    public List<string> CreateRideItemMessage(string itemName) {
        string firstText = itemName + "に乗った。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    //回復したとき
    public List<string> CreateHealMessage(int heal, string takerName) {
        string firstText = takerName + "のHPが、" + upperDamageText.ConvertNumToUpperString(heal) + "ポイント回復した。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    //最大HPが上がったとき
    public List<string> CreateMaxHpUpMessage(int amount) {
        string firstText = "最大HPが" + upperDamageText.ConvertNumToUpperString(amount) + "ポイント上がった。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    public List<string> CreatePickUpMessage(string itemName) {
        string firstText = itemName + "を拾った。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    public List<string> CreateDropMessage(string itemName) {
        string firstText = itemName + "を捨てた。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    public List<string> CreateEquipMessage(string itemName) {
        string firstText = itemName + "を装備した。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    public List<string> CreateUnequipMessage(string itemName) {
        string firstText = itemName + "を外した。";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    public List<string> CreateCantPickUpMessage(string itemName) {
        string firstText = "持ち物がいっぱいでひろえない。";
        string secondText = itemName + "に乗った。";

        List<string> strings = new List<string>{
            firstText,
            secondText,
        };
        return strings;
    }

    public List<string> CreatePlaceItemMessage(string itemName) {
        string firstText = itemName + " をおいた";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }

    public List<string> CreateThrowItemMessage(string itemName) {
        string firstText = itemName + " を投げた";

        List<string> strings = new List<string>{
            firstText,
        };
        return strings;
    }
}