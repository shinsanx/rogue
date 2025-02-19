using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory {
    // アイテムとその数量を管理する辞書
    private Dictionary<ItemSO, int> items = new Dictionary<ItemSO, int>();

    // インベントリが更新された時に発行されるイベント
    public event Action OnInventoryUpdated;

    // アイテムを追加するメソッド
    public void AddItem(ItemSO item, int quantity = 1) {
        if (items.ContainsKey(item)) {
            items[item] += quantity;
        } else {
            items[item] = quantity;
        }
        Debug.Log($"{item.itemName} を {quantity} つ追加しました。現在の数量: {items[item]}");

        // インベントリの更新通知（必要に応じてイベントを発行）
        OnInventoryUpdated?.Invoke();
    }

    // アイテムを削除するメソッド
    public bool RemoveItem(ItemSO item, int quantity = 1) {
        if (items.ContainsKey(item)) {
            if (items[item] >= quantity) {
                items[item] -= quantity;
                Debug.Log($"{item.itemName} を {quantity} つ削除しました。現在の数量: {items[item]}");
                if (items[item] <= 0) {
                    items.Remove(item);
                }
                return true;
            } else {
                Debug.LogError($"削除しようとした数量が所持数を超えています。所持数: {items[item]}, 削除数: {quantity}");
            }
        } else {
            Debug.LogError("指定されたアイテムはインベントリに存在しません。");
        }
        return false;
    }

    // アイテムを使用するメソッド
    public void UseItem(ItemSO item, IEffectReceiver receiver) {
        if (RemoveItem(item)) {
            // アイテムの使用処理
            if (item is ConsumableSO consumable) {
                consumable.effect.ApplyEffect(receiver);
                Debug.Log($"{consumable.itemName} を使用しました。");
            } else {
                Debug.Log("このアイテムは使用できません。");
            }
        } else {
            Debug.Log("アイテムの使用に失敗しました。");
        }
    }

    // インベントリ内の全アイテムを取得
    public Dictionary<ItemSO, int> GetAllItems() {
        return new Dictionary<ItemSO, int>(items);
    }
}