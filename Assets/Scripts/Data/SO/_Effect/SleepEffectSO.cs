using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SleepEffect_SO", menuName = "Item/Effect/SleepEffectSO", order = 0)]
public class SleepEffectSO : InstantEffectSO {    

    public override void ApplyInstantEffect(IEffectReceiver receiver) {

        if (receiver is Player player) {
            player.sleepTurn.Value = 5;
        } else if (receiver is Enemy enemy) {
            enemy.sleepTurn.Value = 5;
        }
    }
}



