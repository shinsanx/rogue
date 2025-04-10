using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemEffectSO : ScriptableObject {
    public string effectName;
    public abstract void ApplyEffect(IEffectReceiver receiver);
}
