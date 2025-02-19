using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory {
    // アイテムとその数量を管理する辞書
    private List<ItemSO> items = new List<ItemSO>();

    // インベントリが更新された時に発行されるイベント
    public event Action OnInventoryUpdated;

    // アイテムを追加するメソッド
    public void AddItem(ItemSO item) {
        items.Add(item);
        Debug.Log($"{item.itemName} を追加しました。現在の数量: {items.Count}");

        // インベントリの更新通知（必要に応じてイベントを発行）
        OnInventoryUpdated?.Invoke();
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
    public List<ItemSO> GetAllItems() {
        return items;
    }
}