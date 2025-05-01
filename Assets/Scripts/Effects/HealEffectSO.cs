using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect_SO", menuName = "Item/Effect/HealEffectSO", order = 0)]
public class HealEffectSO : BaseApplyEffectSO {    
    public int healAmount;
    public int maxUpAmount;

    public override void ApplyEffect(IEffectReceiver receiver) {
        receiver.Heal(healAmount, maxUpAmount);
    }
}
