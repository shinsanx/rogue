using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MuscleUpEffect_SO", menuName = "Item/Effect/MuscleUpEffectSO", order = 0)]
public class MuscleUpEffectSO : BaseApplyEffectSO {
    public int muscleUpAmount;

    public override void ApplyEffect(IEffectReceiver receiver) {

        if (receiver is Player player) {
            player.MuscleUp(muscleUpAmount);
        } else if (receiver is Enemy enemy) {
            enemy.TakeDamage(muscleUpAmount, "");
        }
    }
}



