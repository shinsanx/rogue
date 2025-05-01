using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectReceiver
{
    // 即時系    
    void Heal(int amount, int maxUpAmount);
    void MuscleHeal();
    void Equip(BaseItemSO item);


    // ステータス系
    void AddStatusEffect(BaseStatusEffect effect);
    void RemoveStatusEffect(BaseStatusEffect effect);
    List<StatusEffectInstance> GetActiveStatusEffects();

    [SerializeField] BoolVariable isConfusion{ get; set; }
    [SerializeField] BoolVariable isSleeping{get;set;}   
    int actionRate{get;set;} 

}

