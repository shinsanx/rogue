using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class MessageBox : MonoBehaviour {
    public TextMeshProUGUI firstText;
    public TextMeshProUGUI secondText;
    public TextMeshProUGUI thirdText;
    public TextMeshProUGUI fourthText;
    public Vector2 initialTextsPos;

    [SerializeField] private GameObject texts;
    [SerializeField] private GameObject box;
    
    private readonly Queue<List<string>> messageQueue = new Queue<List<string>>();
    private bool isProcessing = false;
    private CancellationTokenSource cancellationTokenSource;

    private void Start() {
        MessageBus.Instance.Subscribe("sendMessage", OnMessageReceived);
        initialTextsPos = texts.transform.position;
    }

    /// <summary>
    /// MessageBusからメッセージを受信したときに呼び出されます。
    /// </summary>
    /// <param name="data">受信したメッセージ</param>
    /// <returns>非同期タスク</returns>
    async void OnMessageReceived(object data) {
        await EnqueueMessageAsync((List<string>)data);
    }

    /// <summary>
    /// メッセージをキューに追加し、処理を開始します。
    /// 処理中の場合は現在の処理をキャンセルして新しいメッセージで上書きします。
    /// </summary>
    /// <param name="messages">表示するメッセージのリスト</param>
    /// <returns>非同期タスク</returns>
    public async Task EnqueueMessageAsync(List<string> messages) {        
        if (messages == null || messages.Count == 0) return;

        // 既存の処理がある場合はキャンセル
        if (isProcessing && cancellationTokenSource != null) {
            await Task.Delay(1000);
            cancellationTokenSource.Cancel();
            messageQueue.Clear();
        }

        messageQueue.Enqueue(messages);
        if (!isProcessing) {
            await ProcessMessagesAsync();
        }
    }

    /// <summary>
    /// キューにあるメッセージを順次処理します。
    /// </summary>
    /// <returns>非同期タスク</returns>
    private async Task ProcessMessagesAsync() {
        isProcessing = true;
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        try {
            while (messageQueue.Count > 0) {
                var messages = messageQueue.Dequeue();
                if (!box.activeSelf) {
                    await ShowAsync(true, token);
                }
                await DisplayMessagesAsync(messages, token);
            }
        }
        catch (OperationCanceledException) {
            // Debug.Log("メッセージ処理がキャンセルされました。");
        }
        catch (Exception ex) {
            Debug.LogError($"予期せぬエラーが発生しました: {ex.Message}");
        }
        finally {
            isProcessing = false;
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            if (!token.IsCancellationRequested && messageQueue.Count == 0) {
                await ShowAsync(false, CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// メッセージを非同期的に表示します。
    /// </summary>
    /// <param name="messages">表示するメッセージのリスト</param>
    /// <param name="token">キャンセレーショントークン</param>
    /// <returns>非同期タスク</returns>
    public async Task DisplayMessagesAsync(List<string> messages, CancellationToken token) {
        ClearAllTexts();
        texts.transform.DOLocalMove(initialTextsPos, 0.0f).SetUpdate(true);

        TextMeshProUGUI[] textFields = { firstText, secondText, thirdText, fourthText };
        for (int i = 0; i < messages.Count && i < textFields.Length; i++) {            
            token.ThrowIfCancellationRequested();

            textFields[i].text = messages[i];            
            if (i >= 0) {
                await Task.Delay(300, token);
            }
        }

        if (messages.Count >= 4) {
            await AnimateTextPositionAsync(token);
        }

        await Task.Delay(3000, token);

        // メッセージキューに他のメッセージがない場合は非表示にする
        if (messageQueue.Count == 0) {
            await ShowAsync(false, token);
        }
    }

    /// <summary>
    /// メッセージボックスの表示状態を変更します。
    /// </summary>
    /// <param name="visible">表示する場合は true、非表示にする場合は false</param>
    /// <param name="token">キャンセレーショントークン</param>
    /// <returns>非同期タスク</returns>
    public async Task ShowAsync(bool visible, CancellationToken token) {
        if (token.IsCancellationRequested) {
            return;
        }
        box.SetActive(visible);
        await Task.CompletedTask;
    }

    /// <summary>
    /// すべてのテキストフィールドをクリアします。
    /// </summary>
    private void ClearAllTexts() {
        firstText.text = string.Empty;
        secondText.text = string.Empty;
        thirdText.text = string.Empty;
        fourthText.text = string.Empty;
    }

    /// <summary>
    /// テキストの位置をアニメーションで変更します。
    /// </summary>
    /// <param name="token">キャンセレーショントークン</param>
    /// <returns>非同期タスク</returns>
    private async Task AnimateTextPositionAsync(CancellationToken token) {
        texts.transform.DOLocalMove(new Vector3(0, 23, 0), 0.3f).SetEase(Ease.Linear).SetUpdate(true);
        await Task.Delay(500, token);
        texts.transform.DOLocalMove(new Vector3(0, 52, 0), 0.3f).SetEase(Ease.Linear).SetUpdate(true);
    }
}