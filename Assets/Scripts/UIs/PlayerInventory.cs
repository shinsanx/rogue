using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
public class PlayerInventory {

    public PlayerInventory(GameEvent OnPlayerStateComplete){
        if(createMessageLogic == null){
            createMessageLogic = new CreateMessageLogic();
        }
        this.OnPlayerStateComplete = OnPlayerStateComplete;
    }
    private const int MAX_ITEMS = 24; 

    // アイテムとその数量を管理する辞書
    private List<ItemSO> items = new List<ItemSO>();
    private CreateMessageLogic createMessageLogic;
    private GameEvent OnPlayerStateComplete;
    // インベントリが更新された時に発行されるイベント
    public event Action OnInventoryUpdated;
    

    // アイテムを追加するメソッド
    public bool AddItem(ItemSO item) {
        if (items.Count >= MAX_ITEMS) {
            Debug.Log("インベントリが満杯です。");
            return false;
        }

        items.Add(item);
        // Debug.Log($"{item.itemName} を追加しました。現在の数量: {items.Count}");

        // インベントリの更新通知（必要に応じてイベントを発行）
        OnInventoryUpdated?.Invoke();
        return true;
    }

    // アイテムを削除するメソッド
    public bool RemoveItem(ItemSO item) {
        if (items.Contains(item)) {
            items.Remove(item);
            OnInventoryUpdated?.Invoke();
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
                OnPlayerStateComplete.Raise();
                //Debug.Log($"{consumable.itemName} を使用しました。");                                
            } else {
                Debug.Log("このアイテムは使用できません。");
            }
        } else {
            Debug.Log("アイテムの使用に失敗しました。");
        }
    }

    //アイテムを置く
    public void PlaceItem(ItemSO item, Vector2Int position) {
        GameObject itemPrefab = CharacterManager.i.GetItemPrefab(item.id);
        ArrangeManager.i.PlaceItem(itemPrefab, position);
        RemoveItem(item);
        OnPlayerStateComplete.Raise();
        MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreatePlaceItemMessage(item.itemName));
    }

    //アイテムを投げる
    public async Task ThrowItem(ItemSO item, Vector2Int position, Vector2Int direction) {
        Debug.Log("ThrowItemDirection:"+direction);
        Vector2Int throwPosition = TileManager.i.GetCharactersInFront(position, direction, 10);
        //throwPositionのタイプが壁だった場合は一つ前のポジションを取得する
        if(TileManager.i.GetMapChipType(throwPosition) == (int)RandomDungeonWithBluePrint.Constants.MapChipType.Wall) {
            throwPosition = throwPosition - direction;
        }
        //throwPositionのタイプがEnemyだった場合はアイテムの効果を適用する
        if(CharacterManager.i.GetObjectTypeByPosition(throwPosition) == "Enemy") {            
            ItemEffectManager.i.ApplyItemEffect(item, CharacterManager.i.GetObjectByPosition(throwPosition));
            await AnimationManager.i.throwItemAnimation(item, position, throwPosition);
            RemoveItem(item);
            OnPlayerStateComplete.Raise();
            return;
        }
        GameObject itemPrefab = CharacterManager.i.GetItemPrefab(item.id);
        await AnimationManager.i.throwItemAnimation(item, position, throwPosition);
        ArrangeManager.i.PlaceItem(itemPrefab, throwPosition);
        RemoveItem(item);
        OnPlayerStateComplete.Raise();
        MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreateThrowItemMessage(item.itemName));
    }

    // インベントリ内の全アイテムを取得
    public List<ItemSO> GetAllItems() {
        return items;
    }

    
}