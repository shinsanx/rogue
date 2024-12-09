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

    
}