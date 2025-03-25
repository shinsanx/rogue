using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UseItemSO", menuName = "Menu/UseItemSO", order = 0)]
public class UseItemSO : BaseSubmitMenu
{
    public ObjectDataRuntimeSet objectDataRuntimeSet;
    //草を飲む
    public override void Submit() {
        Player player = objectDataRuntimeSet.GetPlayer().GetComponent<Player>();
        MenuManager.Instance.UseItem(currentSelectedObject.Item, player);
    }
    
}
