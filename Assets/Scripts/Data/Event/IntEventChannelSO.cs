using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/IntEventChannelSO")]
public class IntEventChannelSO : ScriptableObject
{
    public event UnityAction<int> OnEventRaised;

    public void RaiseEvent(int value)
    {
        if (OnEventRaised != null) {
            OnEventRaised.Invoke(value);
        }
    }

}
