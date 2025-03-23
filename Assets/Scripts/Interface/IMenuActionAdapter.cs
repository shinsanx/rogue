using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenuActionAdapter
{
    SubmitMenuSet submitMenuSet{get;}
    void OnSelected();    
}
