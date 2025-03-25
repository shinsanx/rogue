using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveOnSO", menuName = "Menu/MoveOnSO", order = 0)]
public class MoveOnSO : BaseSubmitMenu
{
    [SerializeField] GameEvent MoveOnFloor;
    //階層を移動する    
    public override void Submit() {
        MoveOnFloor.Raise();
    }    
}
