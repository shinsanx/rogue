using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable_SO", menuName ="Item/ConsumableSO", order =2)]
public class ConsumableSO : BaseItemSO
{
    public BaseApplyEffectSO effect; //インスペクターで効果を設定する

}
