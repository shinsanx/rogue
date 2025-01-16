using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMessageLogic {
    string upperDamageText = null;
    public CreateMessageLogic() {

    }

    //ダメージを受けたときのメッセージ
    public List<string> CreateAttackMessage(List<string> strings, int damage, string dealerName, string takerName){
        string firstText = dealerName + "は、" + takerName + "に";
        string secondText = upperDamageText.ConvertNumToUpperString(damage) + "ポイントのダメージを与えた。";

        strings.Add(firstText);
        strings.Add(secondText);
        return strings;
    }

    //プレイヤーによって倒されたときのメッセージ（Expつき）
    public List<string> CreateDefeatedMessage(List<string> strings, string takerName, int exp){
        string firstText = takerName + "をやっつけた。";
        string secondText = upperDamageText.ConvertNumToUpperString(exp) + "ポイントの経験値を得た。";

        strings.Add(firstText);
        strings.Add(secondText);
        return strings;
    }

    //レベルが上がったとき
    public List<string> LvUppedMessage(string playerName, int lv){
        string firstText = playerName+"はレベル"+upperDamageText.ConvertNumToUpperString(lv) + "に上がった。";

        List<string>strings = new List<string>{
            firstText,
        };
        return strings;
    }

    //プレイヤーがダメージを受けたとき
    public List<string> CreateTakeDamageMessage(List<string>strings, int damage, string dealerName){
        string firstText = dealerName + "から";
        string secondText = upperDamageText.ConvertNumToUpperString(damage) + "ポイントのダメージを受けた。";

        strings.Add(firstText);
        strings.Add(secondText);
        return strings;
    }

}
