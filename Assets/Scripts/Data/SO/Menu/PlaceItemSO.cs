using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceItemSO", menuName = "Menu/PlaceItemSO", order = 0)]
public class PlaceItemSO : BaseSubmitMenu
{
    public ObjectDataRuntimeSet objectDataRuntimeSet;
    public override void Submit() {
        MenuManager.Instance.PlaceItem(currentSelectedObject.Item, objectDataRuntimeSet.GetPlayerPosition());
    }    
}
