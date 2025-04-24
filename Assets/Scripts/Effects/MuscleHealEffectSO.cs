using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MuscleHealEffect_SO", menuName = "Item/Effect/MuscleHealEffectSO", order = 0)]
public class MuscleHealEffectSO : BaseApplyEffectSO {    

    public override void ApplyEffect(IEffectReceiver receiver) {

       if (receiver is Player player) {
            player.MuscleHeal();
        } else if (receiver is Enemy enemy) {
            enemy.TakeDamage(1, "");
        }
    }
}



