using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 持続効果専用 (Sleep, Poisonなど)
public abstract class StatusEffectSO : ScriptableObject {
    public StatusEffect effectToApply; // StatusEffectを適用する
    public void ApplyStatusEffect(IStatusEffectTarget target) {
        target.AddStatusEffect(effectToApply);
    }
}
