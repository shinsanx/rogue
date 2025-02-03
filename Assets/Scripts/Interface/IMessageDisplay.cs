using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public interface IMessageDisplay 
{
    /// <summary>
    /// メッセージを非同期的に表示する
    /// </summary>
    /// <param name="messages">表示するメッセージのリスト</param>
    Task DisplayMessagesAsync(List<string> messages);

    /// <summary>
    /// メッセージボックスの表示状態を変更する
    /// </summary>
    /// <param name="visible">表示する場合はTrue、非表示はFalse</param>
    Task ShowAsync(bool visible);
}
