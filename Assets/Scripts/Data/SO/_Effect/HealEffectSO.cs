using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect_SO", menuName = "Item/Effect/HealEffectSO", order = 0)]
public class HealEffectSO : InstantEffectSO {
    public int healAmount;

    public override void ApplyInstantEffect(IEffectReceiver receiver) {

        if (receiver is Player player) {
            player.Heal(healAmount);
        } else if (receiver is Enemy enemy) {
            enemy.Heal(healAmount);
        }
    }
}



