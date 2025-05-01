using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageModel {
    public event Action OnChanged;

    const int MaxLines = 3;
    const float Life = 3f;   // 行が消えるまで
    const float Interval = 1f; // 次の行を出すまで

    readonly Queue<string> _shown = new(); // 画面に出てる 0〜3 行
    readonly Queue<string> _pending = new(); // まだ出してへん行
    readonly MonoBehaviour _host;

    bool _consuming; // pending を流すコルーチンが動いてるか

    public IEnumerable<string> Entries => _shown;

    public MessageModel(MonoBehaviour host) => _host = host;

    /* ==== まとめて受け取り ==== */
    public void PushMany(List<string> msgs) {
        foreach (var m in msgs) _pending.Enqueue(m);
        if (!_consuming) _host.StartCoroutine(ConsumePending());
    }

    /* ==== pending → shown へ１行ずつ移すコルーチン ==== */
    IEnumerator ConsumePending() {
        _consuming = true;

        while (_pending.Count > 0) {
            Show(_pending.Dequeue());       // １行表示
            yield return new WaitForSeconds(Interval);
        }

        _consuming = false;
    }

    /* ==== 実際に表示キューへ入れる（１行単位） ==== */
    void Show(string msg) {
        _shown.Enqueue(msg);
        while (_shown.Count > MaxLines) _shown.Dequeue();

        OnChanged?.Invoke();
        _host.StartCoroutine(ExpireAfter(Life, msg));
    }

    IEnumerator ExpireAfter(float sec, string msg) {
        yield return new WaitForSeconds(sec);

        if (_shown.Contains(msg)) {
            var list = new List<string>(_shown);
            list.Remove(msg);
            _shown.Clear();
            foreach (var s in list) _shown.Enqueue(s);

            OnChanged?.Invoke();
        }
    }
}