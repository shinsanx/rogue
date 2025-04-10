using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SleepEffect_SO", menuName = "Item/Effect/SleepEffectSO", order = 0)]
public class SleepEffectSO : StatusEffect {    

    public override void OnStart(IStatusEffectTarget target) {
        Debug.Log(target + "は眠った");
    }

    public override void OnTick(IStatusEffectTarget target, StatusEffectInstance instance) {
        Debug.Log(target + "は眠っている");
    }

    public override void OnEnd(IStatusEffectTarget target) {
        Debug.Log(target + "は目を覚ました");
    }
}



