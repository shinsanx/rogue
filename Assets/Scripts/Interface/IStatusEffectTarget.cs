using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectTarget {
    void AddStatusEffect(BaseStatusEffect effect);
    void RemoveStatusEffect(BaseStatusEffect effect);
    List<StatusEffectInstance> GetActiveStatusEffects();
}
