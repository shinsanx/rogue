using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectReceiver
{
    void ApplyEffect(EffectSO effect);
    void Equip(ItemSO item);
}
    

