using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffect/SleepStatusEffect")]
public class SleepStatusEffect : BaseStatusEffect {
    [SerializeField] BoolVariable canHandleInput;

    public override void OnStart(IEffectReceiver target) {
        if(target is Player){
        canHandleInput.Value = false;        
        } else if(target is Enemy enemy){
            enemy.isSleeping.Value = true;
        } else {
            Debug.LogError("Unknown target type");
            return;
        }
        Debug.Log($"{target} は眠りについた！");
    }

    public override void OnTick(IEffectReceiver target, StatusEffectInstance instance) {
        if(target is Player){
            canHandleInput.Value = false;        
        } 
        Debug.Log($"{target} は眠っている（残り {instance.RemainingTurns} ターン）");                
    }

    public override void OnEnd(IEffectReceiver target) {
        if(target is Player){
            canHandleInput.Value = true;        
        } else if(target is Enemy enemy){
            enemy.isSleeping.Value = false;
        } else {
            Debug.LogError("Unknown target type");
            return;
        }
        Debug.Log($"{target} は目を覚ました！");        
    }
}