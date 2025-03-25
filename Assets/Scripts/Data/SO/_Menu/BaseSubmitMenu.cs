using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSubmitMenu : ScriptableObject
{
    public abstract void Submit();
    public string menuName;
    public CurrentSelectedObjectSO currentSelectedObject;
}
