using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntEventListener : MonoBehaviour
{
    public IntEventChannelSO eventChannel;
    public UnityEvent<int> onEventRaised;
    private void OnEnable()
    {
        eventChannel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        eventChannel.OnEventRaised -= Respond;
    }

    private void Respond(int value)
    {
        onEventRaised.Invoke(value);
    }
}