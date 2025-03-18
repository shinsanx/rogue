using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/BoolEventChannelSO")]
public class BoolEventChannelSO : ScriptableObject
{
    public UnityEvent<bool> OnEventRaised = new UnityEvent<bool>();    
    public void RaiseEvent(bool success)
    {        
        OnEventRaised.Invoke(success);        
    }    
}
