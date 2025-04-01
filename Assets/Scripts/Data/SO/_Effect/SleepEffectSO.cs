using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SleepEffect_SO", menuName = "Item/Effect/SleepEffectSO", order = 0)]
public class SleepEffectSO : EffectSO {
    [SerializeField] IntVariable sleepTurn;

    public override void ApplyEffect(IEffectReceiver receiver) {

        if (receiver is Player player) {
            sleepTurn.Value = 5;
        } else if (receiver is Enemy enemy) {
            
        }
    }
}



