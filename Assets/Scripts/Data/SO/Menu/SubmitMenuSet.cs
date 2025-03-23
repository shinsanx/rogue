using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SubmitMenuSet", menuName = "Menu/SubmitMenuSet", order = 0)]
public class SubmitMenuSet : ScriptableObject
{
    public BaseSubmitMenu[] submitMenus;
}
