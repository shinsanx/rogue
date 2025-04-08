using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects/Sleep")]
public class SleepEffect : StatusEffect {
    public override bool BlocksAction() => true;

    public override void OnTick(IStatusEffectTarget target) {
        remainingTurns--;
        Debug.Log($"Sleepターン残り: {remainingTurns}");
    }
}

