using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameEventListener : MonoBehaviour {
    [Tooltip("Event to register with.")]
    public GameEvent Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public UnityEvent Response;

    private void OnEnable() {

        // // ① まず、現在登録されとる Persistent リスナー数をログ出し
        // int count = Response.GetPersistentEventCount();
        // Debug.Log($"[GameEventListener] Persistent listeners count: {count}");

        // // ② どのオブジェクトのどのメソッドが登録されとるかを列挙
        // for (int i = 0; i < count; i++)
        // {
        //     var target   = Response.GetPersistentTarget(i);
        //     var method   = Response.GetPersistentMethodName(i);
        //     Debug.Log($"  Listener #{i}: Target={target?.name ?? "null"}, Method={method}");
        // }

        Event.RegisterListener(this);
    }

    private void OnDisable() {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised() {
        Response.Invoke();
    }
}
