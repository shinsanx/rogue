using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : ScriptableObject {
    public string effectName;
    public int duration;

    // 毎ターン呼ばれる
    public virtual void OnTick(IEffectReceiver target, StatusEffectInstance instance){
        
    }

    public virtual void OnStart(IEffectReceiver target) {
        // 行動前に呼ばれる
    }

    public virtual void OnEnd(IEffectReceiver target) {
        // 行動後に呼ばれる
    }    
    
}

