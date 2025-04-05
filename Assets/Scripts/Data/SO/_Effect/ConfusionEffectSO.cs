using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfusionEffect_SO", menuName = "Item/Effect/ConfusionEffectSO", order = 0)]
public class ConfusionEffectSO : EffectSO {    

    public override void ApplyEffect(IEffectReceiver receiver) {

        if (receiver is Player player) {
            player.confusionTurn.Value = 10;
        } else if (receiver is Enemy enemy) {
            enemy.confusionTurn.Value = 10;
        }
    }
}



