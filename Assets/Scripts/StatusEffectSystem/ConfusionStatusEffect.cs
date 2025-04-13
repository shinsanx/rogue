using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffect/ConfusionStatusEffect")]
public class ConfusionStatusEffect : BaseStatusEffect {    

    public override void OnStart(IEffectReceiver target) {
        Debug.Log($"{target} は混乱した！");
        target.isConfusion.Value = true;
    }

    public override void OnTick(IEffectReceiver target, StatusEffectInstance instance) {
        Debug.Log($"{target} は混乱している（残り {instance.RemainingTurns} ターン）");        
        target.isConfusion.Value = true;
    }

    public override void OnEnd(IEffectReceiver target) {
        Debug.Log($"{target} は正気にもどった！");
        target.isConfusion.Value = false;
    }
}