using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectInstance {
    public BaseStatusEffect Effect { get; }
    public int RemainingTurns { get; private set; }
    private readonly IEffectReceiver target;

    public StatusEffectInstance(BaseStatusEffect effect, IEffectReceiver target) {
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
