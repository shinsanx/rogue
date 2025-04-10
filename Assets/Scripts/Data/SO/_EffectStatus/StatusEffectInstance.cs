using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectInstance {
    public StatusEffect Effect { get; }
    public int RemainingTurns { get; private set; }
    private readonly IStatusEffectTarget target;

    public StatusEffectInstance(StatusEffect effect, IEffectReceiver target) {
        Effect = effect;
        RemainingTurns = effect.duration;
        this.target = target;
        Effect.OnStart(target);
    }

    public void Tick() {
        Effect.OnTick(target, this);
        RemainingTurns--;
    }

    public void EndEffect() {
        Effect.OnEnd(target);
    }

    public bool IsExpired => RemainingTurns <= 0;
}
