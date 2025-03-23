using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ThrowItemSO", menuName = "Menu/ThrowItemSO", order = 0)]
public class ThrowItemSO : BaseSubmitMenu
{
    public ObjectDataRuntimeSet objectDataRuntimeSet;
    public async override void Submit() {
        await MenuManager.Instance.ThrowItem(currentSelectedObject.Item, objectDataRuntimeSet.GetPlayerPosition());
    }    
}
