using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectReceiver
{
    public IntVariable sleepTurn { get; set; }
    void ApplyEffect(EffectSO effect);
    void Equip(ItemSO item);
}
    

