using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class SubMenuController : BaseMenuController {
    [SerializeField] private GameObject subMenuWindow; // サブメニュー            
    [SerializeField] private GameObject menuPrefab;
    [SerializeField] private GameObject submenuParent;
    [SerializeField] private CurrentSelectedObjectSO currentSelectedObject;

    void OnEnable() {
        Debug.Log("SubMenuControllerのOnEnableが呼び出されました。");
        currentIndex = 0;
    }

    public override void OpenMenu() {
        subMenuWindow.SetActive(true);
        ClearMenu();
        GenerateMenuObject();
        isActive = true;
    }

    public override void CloseMenu() {
        isActive = false;
        subMenuWindow.SetActive(false);
        MenuManager.Instance.CloseSpecificMenu<FootMenuController>();
        DestroyCursor();
    }

    private void DestroyCursor() {
        if (cursorInstance != null) {
            Destroy(cursorInstance);
            cursorInstance = null;
        }
    }

    private void GenerateMenuObject() {
        if (currentSelectedObject.Object == null) {
            GenerateMenuFromSubmitMenuSet();
            return;
        }
        
        TryGetSubmitMenuSetFromObject();
        GenerateMenuFromSubmitMenuSet();
    }

    private void TryGetSubmitMenuSetFromObject() {
        IMenuActionAdapter menuActionAdapter = currentSelectedObject.Object.GetComponent<IMenuActionAdapter>();
        if (menuActionAdapter != null) {
            // 対象がアイテムではない場合（階段など）
            currentSelectedObject.SubmitMenuSet = menuActionAdapter.submitMenuSet;
        }
    }

    private void GenerateMenuFromSubmitMenuSet() {
        if (currentSelectedObject.SubmitMenuSet == null) { 
            Debug.Log("submitMenuSetがnullです。"); 
            return; 
        }
        
        menuItems = new List<GameObject>();
        foreach (BaseSubmitMenu menu in currentSelectedObject.SubmitMenuSet.submitMenus) {
            CreateMenuItem(menu);
        }
    }

    private void CreateMenuItem(BaseSubmitMenu menu) {
        GameObject menuObject = Instantiate(menuPrefab, submenuParent.transform);
        SubMenuPrefab subMenuPrefab = menuObject.GetComponent<SubMenuPrefab>();
        subMenuPrefab.SetMenuText(menu.menuName);
        menuItems.Add(menuObject);
    }

    /// <summary>
    /// サブメニューで決定したときの処理
    /// </summary>
    public override void Submit() {
        currentSelectedObject.SubmitMenuSet.submitMenus[currentIndex].Submit();
    }

    protected override void UpdateCursorPosition() {
        if (cursorInstance == null) {
            cursorInstance = Instantiate(cursor, transform);
        }
        cursorInstance.transform.SetParent(menuItems[currentIndex].transform, false);
    }

    void ClearMenu() {
        foreach (GameObject menu in menuItems) {
            Destroy(menu);
        }
        menuItems.Clear();
    }
}