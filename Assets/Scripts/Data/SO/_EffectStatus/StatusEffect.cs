using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : ScriptableObject {
    public string effectName;
    public int remainingTurns;

    public abstract bool BlocksAction(); // trueなら行動スキップ
    public abstract void OnTick(IStatusEffectTarget target); // 毎ターン呼ばれる
}

