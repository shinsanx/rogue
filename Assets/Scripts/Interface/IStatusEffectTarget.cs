using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectTarget {
    void AddStatusEffect(StatusEffect effect);
    void RemoveStatusEffect(StatusEffect effect);
    List<StatusEffectInstance> GetActiveStatusEffects();
}
