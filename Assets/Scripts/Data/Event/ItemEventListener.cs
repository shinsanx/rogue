using UnityEngine;
using UnityEngine.Events;

public class ItemEventListener : MonoBehaviour
{
    [SerializeField] private ItemEventChannelSO channel;
    [SerializeField] private UnityEvent<ItemSO> response;

    private void OnEnable()
    {
        if (channel != null)
        {
            // デバッグ情報を追加
            Debug.Log($"Registering listener to channel: {channel.name}");
            channel.OnEventRaised.AddListener(OnEventRaised);
        }
        else
        {
            Debug.LogError("ItemEventListener has no channel assigned!", this);
        }
    }

    private void OnDisable()
    {
        if (channel != null)
        {
            channel.OnEventRaised.RemoveListener(OnEventRaised);
        }
    }

    private void OnEventRaised(ItemSO item)
    {
        // デバッグ情報を追加
        if (item == null)
        {
            Debug.LogError("Received null ItemSO in ItemEventListener!", this);
            return;
        }
        
        try
        {
            response.Invoke(item);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ItemEventListener response: {e.Message}\n{e.StackTrace}", this);
        }
    }
}