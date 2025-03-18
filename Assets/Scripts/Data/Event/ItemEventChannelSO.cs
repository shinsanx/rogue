using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/ItemEventChannelSO")]
public class ItemEventChannelSO : ScriptableObject
{
    public UnityEvent<ItemSO> OnEventRaised = new UnityEvent<ItemSO>();    
    public void RaiseEvent(ItemSO item)
    {        
        OnEventRaised.Invoke(item);        
    }

}
