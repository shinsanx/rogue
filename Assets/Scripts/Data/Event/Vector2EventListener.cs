using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Vector2EventListener : MonoBehaviour
{
    public Vector2EventChannelSO eventChannel;
    public UnityEvent<Vector2> onEventRaised;
    private void OnEnable()
    {
        eventChannel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        eventChannel.OnEventRaised -= Respond;
    }

    private void Respond(Vector2 value)
    {
        onEventRaised.Invoke(value);
    }
}