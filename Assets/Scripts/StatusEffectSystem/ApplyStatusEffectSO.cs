using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffect/ApplyStatusEffectSO")]
public class ApplyStatusEffectSO : BaseApplyEffectSO {
    public BaseStatusEffect statusEffectToApply;

    public override void ApplyEffect(IEffectReceiver receiver) {
        receiver.AddStatusEffect(statusEffectToApply);
    }
}