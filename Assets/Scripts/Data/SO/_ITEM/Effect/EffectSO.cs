using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectSO : ScriptableObject
{
    [TextArea] public string effectDescription;

    // 効果を実行するための抽象メソッド
    public abstract void ApplyEffect(IEffectReceiver receiver);
}
