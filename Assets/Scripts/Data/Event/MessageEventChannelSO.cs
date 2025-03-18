using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/MessageEventChannelSO")]
public class MessageEventChannelSO : ScriptableObject
{
    public event UnityAction<List<string>> OnEventRaised;

    public void RaiseEvent(List<string> value)
    {
        if (OnEventRaised != null) {
            OnEventRaised.Invoke(value);
        }
    }

}
