using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class PlayerInventory : MonoBehaviour {
    
    private const int MAX_ITEMS = 24; 
    [SerializeField] bool ResetInventory = false;

    // アイテムとその数量を管理する辞書
    [SerializeField]private CreateMessageLogic createMessageLogic;
    [SerializeField] private InventorySO inventorySO;
    [SerializeField] private BoolEventChannelSO successItemPicked;
    [SerializeField] private BoolEventChannelSO successItemRemoved; //MenuManagerで購読

    // インベントリが更新された時に発行されるイベント
    public event Action OnInventoryUpdated;    

    void OnEnable() {
        if(ResetInventory){
            inventorySO.ResetInventory();
        }        
    }

    // アイテムを追加するメソッド
    public void AddItem(ItemSO item) {
        if (inventorySO.items.Count >= MAX_ITEMS) {
            Debug.Log("インベントリが満杯です。");            
            successItemPicked.RaiseEvent(false);
            return;
        }

        inventorySO.items.Add(item);
        // Debug.Log($"{item.itemName} を追加しました。現在の数量: {items.Count}");

        // インベントリの更新通知（必要に応じてイベントを発行）
        //OnInventoryUpdated?.Invoke();
        successItemPicked.RaiseEvent(true);
    }

    // アイテムを削除するメソッド
    public void RemoveItem(ItemSO item) {
        if (inventorySO.items.Contains(item)) {
            inventorySO.items.Remove(item);
            OnInventoryUpdated?.Invoke();
            Debug.Log($"{item.itemName} を削除しました。現在の数量: {inventorySO.items.Count}");
            
            successItemRemoved.RaiseEvent(true);
        } else {
            Debug.LogError("指定されたアイテムはインベントリに存在しません。");
            
            successItemRemoved.RaiseEvent(false);
        }
    }



    
}