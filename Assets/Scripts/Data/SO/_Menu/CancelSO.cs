using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CancelSO", menuName = "Menu/CancelSO", order = 0)]
public class CancelSO : BaseSubmitMenu
{
    
    //キャンセルする
    public override void Submit() {
        MenuManager.Instance.CloseAllMenus();
    }
    
}
