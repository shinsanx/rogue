using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/ItemEventChannelSO")]
public class ItemEventChannelSO : ScriptableObject
{
    public UnityEvent<BaseItemSO> OnEventRaised = new UnityEvent<BaseItemSO>();    
    public void RaiseEvent(BaseItemSO item)
    {        
        OnEventRaised.Invoke(item);        
    }

}
