using UnityEngine;
using UnityEngine.Events;

public class BoolEventListener : MonoBehaviour {
    public BoolEventChannelSO channel;
    public UnityEvent<bool> response;

    private void OnEnable() {
        if (channel != null) {
            // デバッグ情報を追加            
            channel.OnEventRaised.AddListener(Respond);
        } else {
            Debug.LogError("BoolEventListener has no channel assigned!", this);
        }
    }

    private void OnDisable() {
        if (channel != null) {
            channel.OnEventRaised.RemoveListener(Respond);
        }
    }

    private void Respond(bool success) {
        response.Invoke(success);
    }
}