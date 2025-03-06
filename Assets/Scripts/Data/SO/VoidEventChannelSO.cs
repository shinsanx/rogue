using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/VoidEventChannelSO")]
public class VoidEventChannelSO : ScriptableObject {
    public event UnityAction OnEventRaised;

    public void RaiseEvent() {
        if (OnEventRaised != null) {
            OnEventRaised.Invoke();
        }
    }
}
