using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MessageModel {
    public event Action OnChanged;
    public event Action OnTimeout;              // 3 秒無入力タイムアウト
    public event Action OnOverFlow;             // 4 件目が来た！

    const int MaxShown = 3;
    const float Interval = 1f;               // 1 秒ごとに表示
    const float TimeoutSec = 3f;               // 3 秒無音なら全消し

    readonly Queue<string> _pending = new();
    readonly Queue<string> _shown = new();
    readonly MonoBehaviour _host;

    float _lastAddTime;                        // 最後に show した時刻
    bool _consuming;

    public IEnumerable<string> Shown => _shown;

    public MessageModel(MonoBehaviour host) => _host = host;

    /* ---- まとめて受信 ---- */
    public void PushMany(List<string> msgs) {
        foreach (var m in msgs) _pending.Enqueue(m);
        if (!_consuming) _host.StartCoroutine(Consume());
    }

    /* ---- 1 秒おきに pending → shown ---- */
    IEnumerator Consume() {
        _consuming = true;
        while (_pending.Count > 0) {
            Show(_pending.Dequeue());
            yield return new WaitForSeconds(Interval);
        }
        _consuming = false;
    }

    /* ---- 実際に表示キューへ ---- */
    void Show(string msg) {
        _shown.Enqueue(msg);
        if (_shown.Count > MaxShown)           // 4 件目が来た！
        {
            _shown.Dequeue();                  // 最古を drop → View がフェード
            OnOverFlow?.Invoke();            // View に「1 件消して！」通知
        }

        _lastAddTime = Time.time;
        OnChanged?.Invoke();

        // タイムアウト監視コルーチンをリセット
        _host.StopCoroutine(nameof(CheckTimeout));
        _host.StartCoroutine(CheckTimeout());
    }

    /* ---- 3 秒無入力 → 全消し ---- */
    IEnumerator CheckTimeout() {
        yield return new WaitForSeconds(TimeoutSec);
        if (Time.time - _lastAddTime >= TimeoutSec && _pending.Count == 0) {
            _shown.Clear();
            OnTimeout?.Invoke();               // View に「全部フェードして！」通知
            OnChanged?.Invoke();               // データも空になったと知らせる
        }
    }
}