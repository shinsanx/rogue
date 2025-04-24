using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoisonEffectSO", menuName = "Item/Effect/PoisonEffectSO", order = 0)]
public class PoisonEffectSO : BaseApplyEffectSO {    

    public override void ApplyEffect(IEffectReceiver receiver) {

       if (receiver is Player player) {
            player.MuscleHeal();
        } else if (receiver is Enemy enemy) {
            enemy.TakeDamage(5, "");
        }
    }
}



