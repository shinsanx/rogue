using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipSO", menuName = "Menu/EquipSO", order = 0)]
public class EquipSO : BaseSubmitMenu
{
    public ObjectDataRuntimeSet objectDataRuntimeSet;
    //装備する
    public override void Submit() {
        Player player = objectDataRuntimeSet.GetPlayer().GetComponent<Player>();
        MenuManager.Instance.Equip(currentSelectedObject.Item, player);
    }
    
}
