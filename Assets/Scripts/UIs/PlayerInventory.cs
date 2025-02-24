using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory {

    public PlayerInventory(){
        if(createMessageLogic == null){
            createMessageLogic = new CreateMessageLogic();
        }
    }

    private const int MAX_ITEMS = 24; 

    

    // アイテムとその数量を管理する辞書
    private List<ItemSO> items = new List<ItemSO>();
    private CreateMessageLogic createMessageLogic;

    // インベントリが更新された時に発行されるイベント
    public event Action OnInventoryUpdated;
    // アイテムを使用した時に発行されるイベント
    public Action onItemUsed;

    // アイテムを追加するメソッド
    public bool AddItem(ItemSO item) {
        if (items.Count >= MAX_ITEMS) {
            Debug.Log("インベントリが満杯です。");
            return false;
        }

        items.Add(item);
        Debug.Log($"{item.itemName} を追加しました。現在の数量: {items.Count}");

        // インベントリの更新通知（必要に応じてイベントを発行）
        OnInventoryUpdated?.Invoke();
        return true;
    }

    // アイテムを削除するメソッド
    public bool RemoveItem(ItemSO item) {
        if (items.Contains(item)) {
            items.Remove(item);
            Debug.Log($"{item.itemName} を削除しました。現在の数量: {items.Count}");
            return true;
        } else {
            Debug.LogError("指定されたアイテムはインベントリに存在しません。");
            return false;
        }
    }

    // アイテムを使用するメソッド
    public void UseItem(ItemSO item, IEffectReceiver receiver) {
        if (RemoveItem(item)) {
            // アイテムの使用処理
            if (item is ConsumableSO consumable) {
                MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreateUseItemMessage(consumable.itemName));
                consumable.effect.ApplyEffect(receiver);
                Debug.Log($"{consumable.itemName} を使用しました。");

                //インベントリを閉じる
                onItemUsed?.Invoke();
            } else {
                Debug.Log("このアイテムは使用できません。");
            }
        } else {
            Debug.Log("アイテムの使用に失敗しました。");
        }
    }

    // インベントリ内の全アイテムを取得
    public List<ItemSO> GetAllItems() {
        return items;
    }

    
}