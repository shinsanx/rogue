using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler
{
    private CurrentSelectedObjectSO currentSelectedObjectSO;
    private ItemEventChannelSO onItemPicked;
    private GameObject currentItemObject;
    private TileManager tileManager;

    public PlayerItemHandler(CurrentSelectedObjectSO currentSelectedObjectSO, ItemEventChannelSO onItemPicked, TileManager tileManager) {
        this.currentSelectedObjectSO = currentSelectedObjectSO;
        this.onItemPicked = onItemPicked;
        this.tileManager = tileManager;
    }

    public void TryPickupItem(Vector2Int targetPos) {
        Item item = tileManager.CheckExistItem(targetPos);
        if (item != null) {
            Debug.Log(targetPos + "にアイテムがあります");
            currentSelectedObjectSO.Object = item.gameObject;

            if (item.itemSO != null) {
                currentItemObject = item.gameObject;
                onItemPicked.RaiseEvent(item.itemSO);
            } else {
                Debug.LogError("Item " + item.name + " has no ItemSO assigned!");
            }
        }
    }

    public void HandleItemPicked(bool success) {
        if (success && currentItemObject != null) {
            Item item = currentItemObject.GetComponent<Item>();
            if (item != null) {
                item.OnPicked();
            }
        } else {
            Debug.Log("アイテムを拾えませんでした。");
        }
    }
   
}
