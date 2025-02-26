using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubMenuController : BaseMenuController {
    [SerializeField] private GameObject subMenuWindow; // サブメニュー        
    [SerializeField] private GameObject useMenu;
    [SerializeField] private GameObject placeMenu;
    [SerializeField] private GameObject throwMenu;
    [SerializeField] private Player player;
    
    private ItemSO selectedItem;

    protected override void InitializeMenu() {
        menuItems = new List<GameObject>();
        menuItems.Add(useMenu);
        menuItems.Add(placeMenu);
        menuItems.Add(throwMenu);
        currentIndex = 0;
    }

    public override void OpenMenu() {        
        subMenuWindow.SetActive(true);
        isActive = true;
    }

    public override void CloseMenu() {
        isActive = false;
        subMenuWindow.SetActive(false);        
        if(cursorInstance != null) {
            Destroy(cursorInstance);
            cursorInstance = null;
        }        
    }

    public void SetSubMenu(ItemSO selectedItem) {        
        this.selectedItem = selectedItem;
    }


    /// <summary>
    /// サブメニューで決定したときの処理
    /// </summary>
    public override void Submit() {
        switch (currentIndex) {
            case 0:                
                UseItem();
                break;
            case 1:
                PlaceItem();
                break;
            case 2:
                Debug.Log("throwMenu");
                break;
            default:
                Debug.LogError("無効な選択インデックスです。");
                break;
        }
    }


    protected override void UpdateCursorPosition() {
        if (cursorInstance == null) {
            cursorInstance = Instantiate(cursor, transform);
        }
        cursorInstance.transform.SetParent(menuItems[currentIndex].transform, false);
    }

    private void UseItem() {
        player.playerInventory.UseItem(selectedItem, player);
        MenuManager.Instance.CloseAllMenus();
    }

    private void PlaceItem() {
        GameObject itemPrefab = CharacterManager.i.GetItemPrefab(selectedItem.id);
        ArrangeManager.i.PlaceItem(itemPrefab, player.GetComponent<IObjectData>().Position);
        player.playerInventory.RemoveItem(selectedItem);
        MessageBus.Instance.Publish(DungeonConstants.sendMessage, GameAssets.i.createMessageLogic.CreatePlaceItemMessage(selectedItem.itemName));
        MenuManager.Instance.CloseAllMenus();
    }
}