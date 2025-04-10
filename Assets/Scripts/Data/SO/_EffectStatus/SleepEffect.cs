using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects/Sleep")]
public class SleepEffect : StatusEffect {
    
    public override void OnStart(IEffectReceiver target) {
        Debug.Log($"{target} が眠り状態になった！");
    }

    public override void OnTick(IEffectReceiver target, StatusEffectInstance instance) {
        Debug.Log($"{target}: Sleep残り{instance.RemainingTurns}ターン");
    }

    public override void OnEnd(IEffectReceiver target) {
        Debug.Log($"{target} の眠りが解除された！");
    }
}

