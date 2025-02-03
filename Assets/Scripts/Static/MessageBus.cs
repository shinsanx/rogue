using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst.Intrinsics;
using System;

public class MessageBus {


    private static MessageBus instance;
    public static MessageBus Instance {
        get {
            if (instance == null) {
                instance = new MessageBus();
            }
            return instance;
        }
    }

    //戻り値なしの場合
    private Dictionary<string, Action<object>> subscribers = new Dictionary<string, Action<object>>();

    public void Subscribe(string message, Action<object> callback) {
        if (!subscribers.ContainsKey(message)) {
            subscribers.Add(message, null);
        }
        subscribers[message] += callback;
    }

    public void Unsubscribe(string message, Action<object> callback) {
        if (subscribers.ContainsKey(message)) {
            subscribers[message] -= callback;
        }
    }

    public void Publish(string message, object data = null) {
        if (subscribers.ContainsKey(message) && subscribers[message] != null) {
            subscribers[message]?.Invoke(data);
        }
    }

    //=====Delegate=====

    //戻り値が欲しい場合はこっち
    public delegate T MethodWithReturnValue<T>(object data);

    private Dictionary<string, Delegate> delegateSubscribers = new Dictionary<string, Delegate>();

    public void Subscribe<T>(string message, MethodWithReturnValue<T> callback) {
        if (!delegateSubscribers.ContainsKey(message)) {
            delegateSubscribers.Add(message, callback);
        } else {
            delegateSubscribers[message] = Delegate.Combine(delegateSubscribers[message], callback);
        }
    }

    public T Publish<T>(string message, object data) {
        T returnValue = default(T);

        //登録されているハンドラを呼び出し、戻り値を集約する
        if (delegateSubscribers.ContainsKey(message)) {
            Delegate[] handlers = delegateSubscribers[message].GetInvocationList();

            foreach (Delegate handler in handlers) {
                MethodWithReturnValue<T> methodWithReturnValue = handler as MethodWithReturnValue<T>;
                
                if(methodWithReturnValue != null){
                    returnValue = methodWithReturnValue.Invoke(data);
                }
            }
        }
        return returnValue;
    }

}