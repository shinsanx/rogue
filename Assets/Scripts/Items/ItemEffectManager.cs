using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectManager : MonoBehaviour {
    private static ItemEffectManager _instance;
    public static ItemEffectManager i {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<ItemEffectManager>();
                if (_instance == null) {
                    Debug.LogError("ItemEffectManagerがシーンに存在しません。");
                }
            }
            return _instance;
        }
    }



    public void ApplyItemEffect(ItemSO item, GameObject target) {
        Debug.Log("アイテムの効果を適用します。");
        if (item.id == itemDictionary["薬草"]) {
            ConsumableSO consumable = item as ConsumableSO;
            consumable.effect.ApplyEffect(target.GetComponent<IEffectReceiver>());
        }
    }

    public void HealPlayer(Player player, int amount) {
        //体力MAXの場合
        if (player.playerMaxHealth.Value == player.playerCurrentHealth.Value) {
            player.playerMaxHealth.SetValue(player.playerMaxHealth.Value + 1);
            player.playerCurrentHealth.SetValue(player.playerMaxHealth.Value);
            MessageBus.Instance.Publish(DungeonConstants.sendMessage, GameAssets.i.createMessageLogic.CreateMaxHpUpMessage(1));
            return;
        }
        int healAmount = Mathf.Clamp(amount, 0, player.playerMaxHealth.Value - player.playerCurrentHealth.Value);
        player.playerCurrentHealth.SetValue(player.playerCurrentHealth.Value + healAmount);
        MessageBus.Instance.Publish(DungeonConstants.sendMessage, GameAssets.i.createMessageLogic.CreateHealMessage(healAmount, player.playerObjectData.Name.Value));
    }

    public void HealEnemy(Enemy enemy, int amount) {
        //体力MAXの場合
        if (enemy.MaxHealth == enemy.HP) {
            return;
        }
        int healAmount = Mathf.Clamp(amount, 0, enemy.MaxHealth - enemy.HP);
        enemy.HP += healAmount;
        //MessageBus.Instance.Publish(DungeonConstants.sendMessage, GameAssets.i.createMessageLogic.CreateHealMessage(healAmount, enemy.Name));
    }



    private Dictionary<string, int> itemDictionary = new Dictionary<string, int>
        {
            {"こん棒", 1},
            {"金の剣", 2},
            {"銅の剣", 3},
            {"鉄の斧", 4},
            {"ドラゴンキラー", 5},
            {"はぐれメタルの剣", 6},
            {"正義のソロバン", 7},
            {"皮の盾", 8},
            {"青銅の盾", 9},
            {"うろこの盾", 10},
            {"みかがみの盾", 11},
            {"鋼鉄の盾", 12},
            {"ドラゴンシールド", 13},
            {"はぐれメタルの盾", 14},
            {"ザメハの指輪", 15},
            {"ちからの指輪", 16},
            {"ルーラの指輪", 17},
            {"眠らずの指輪", 18},
            {"ハラペコの指輪", 19},
            {"毒けしの指輪", 20},
            {"シャドーの指輪", 21},
            {"人形よけの指輪", 22},
            {"とうぞくの指輪", 23},
            {"ワナ抜けの指輪", 24},
            {"ハラヘラズの指輪", 25},
            {"きれいな指輪", 26},
            {"木の矢", 27},
            {"鉄の矢", 28},
            {"銀の矢", 29},
            {"くさったパン", 30},
            {"パン", 31},
            {"大きいパン", 32},
            {"すばやさの種", 33},
            {"ちからの種", 34},
            {"幸せの種", 35},
            {"目つぶし草", 36},
            {"ルーラ草", 37},
            {"メダパニ草", 38},
            {"目薬草", 39},
            {"まどわし草", 40},
            {"毒草", 41},
            {"薬草", 42},
            {"毒けし草", 43},
            {"ラリホー草", 44},
            {"火炎草", 45},
            {"弟切草", 46},
            {"くちなしの巻物", 47},
            {"ワナの巻物", 48},
            {"インパスの巻物", 49},
            {"メッキの巻物", 50},
            {"地獄耳の巻物", 51},
            {"千里眼の巻物", 52},
            {"レミーラの巻物", 53},
            {"バイキルトの巻物", 54},
            {"スカラの巻物", 55},
            {"シャナクの巻物", 56},
            {"パンの巻物", 57},
            {"祈りの巻物", 58},
            {"かなしばりの巻物", 59},
            {"イオの巻物", 60},
            {"時の砂の巻物", 61},
            {"リレミトの巻物", 62},
            {"聖域の巻物", 63},
            {"パルプンテの巻物", 64},
            {"さいごの巻物", 65},
            {"証明の巻物", 66},
            {"大損の杖", 67},
            {"レムオルの杖", 68},
            {"ピオリムの杖", 69},
            {"いかずちの杖", 70},
            {"メダパニの杖", 71},
            {"分裂の杖", 72},
            {"ラリホーの杖", 73},
            {"封印の杖", 74},
            {"変化の杖", 75},
            {"バシルーラの杖", 76},
            {"ボミオスの杖", 77},
            {"もろ刃の杖", 78},
            {"転ばぬ先の杖", 79},
            {"ザキの杖", 80}
        };

}


