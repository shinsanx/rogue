using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffect/DoubleSpeedStatusEffect")]
public class DoubleSpeedStatusEffect : BaseStatusEffect {
    //[SerializeField] BoolVariable canHandleInput;

    public override void OnStart(IEffectReceiver target) {
        target.actionRate = 100;
        Debug.Log($"{target} は倍速状態になった！");
    }

    public override void OnTick(IEffectReceiver target, StatusEffectInstance instance) {
        Debug.Log($"{target} は倍速（残り {instance.RemainingTurns} ターン）");
    }

    public override void OnEnd(IEffectReceiver target) {
        target.actionRate = 50;
        Debug.Log($"{target} は倍速状態が解除された！");
    }
}