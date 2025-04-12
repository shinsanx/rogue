using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffect/SleepStatusEffect")]
public class SleepStatusEffect : BaseStatusEffect {

    public override void OnStart(IEffectReceiver target) {
        Debug.Log($"{target} は眠りについた！");
    }

    public override void OnTick(IEffectReceiver target, StatusEffectInstance instance) {
        Debug.Log($"{target} は眠っている（残り {instance.RemainingTurns} ターン）");
    }

    public override void OnEnd(IEffectReceiver target) {
        Debug.Log($"{target} は目を覚ました！");
    }
}