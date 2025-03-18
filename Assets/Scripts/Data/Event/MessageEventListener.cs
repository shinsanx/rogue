using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MessageEventListener : MonoBehaviour
{
    public MessageEventChannelSO eventChannel;
    public UnityEvent<List<string>> onEventRaised;
    private void OnEnable()
    {
        eventChannel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        eventChannel.OnEventRaised -= Respond;
    }

    private void Respond(List<string> value)
    {
        onEventRaised.Invoke(value);
    }
}