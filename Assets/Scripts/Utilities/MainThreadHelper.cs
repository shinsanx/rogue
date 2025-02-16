using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MainThreadHelper : MonoBehaviour {
    private static SynchronizationContext unitySynchronizationContext;
    private static MainThreadHelper _instance;

    // 静的コンストラクタでインスタンスを生成
    static MainThreadHelper()
    {
        var obj = new GameObject("MainThreadHelper");
        _instance = obj.AddComponent<MainThreadHelper>();
        DontDestroyOnLoad(obj);
    }

    private void Awake()
    {
        // UnityのメインスレッドのSynchronizationContextを取得
        unitySynchronizationContext = SynchronizationContext.Current;
    }

    public static MainThreadHelper Instance {
        get {
            if (_instance == null) {
                var obj = new GameObject("MainThreadHelper");
                _instance = obj.AddComponent<MainThreadHelper>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    /// <summary>
    /// メインスレッドでアクションを実行します。
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <returns>タスク</returns>
    public static Task RunOnMainThread(Action action) {
        var tcs = new TaskCompletionSource<bool>();

        unitySynchronizationContext.Post(_ =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }

    /// <summary>
    /// メインスレッドでアクションを実行し、結果を返します。
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する関数</param>
    /// <returns>関数の実行結果</returns>
    public static Task<T> RunOnMainThread<T>(Func<T> func) {
        var tcs = new TaskCompletionSource<T>();

        unitySynchronizationContext.Post(_ =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);

        return tcs.Task;
    }
}