using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class SubMenuController : BaseMenuController {
    [SerializeField] private GameObject subMenuWindow; // サブメニュー        
    [SerializeField] private GameObject useMenu;
    [SerializeField] private GameObject placeMenu;
    [SerializeField] private GameObject throwMenu;
    [SerializeField] private Player player;    
    
    private ItemSO selectedItem;

    void OnEnable() {
        Debug.Log("SubMenuControllerのOnEnableが呼び出されました。");
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
                ThrowItem();
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

    //アイテムを使用する
    private void UseItem() {
        MenuManager.Instance.UseItem(selectedItem, player);
        MenuManager.Instance.CloseAllMenus();
    }

    //アイテムを置く
    private void PlaceItem() {
        MenuManager.Instance.PlaceItem(selectedItem, player.GetComponent<IObjectData>().Position.Value);
        MenuManager.Instance.CloseAllMenus();
    }

    //アイテムを投げる
    private async Task ThrowItem() {
        MenuManager.Instance.CloseAllMenus();                
        await MenuManager.Instance.ThrowItem(selectedItem, player.GetComponent<IObjectData>().Position.Value);
    }
}