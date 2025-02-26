using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    // 現在アクティブなメニュー（BaseMenuControllerを継承している各メニュー）
    private BaseMenuController activeMenu;
    public List<BaseMenuController> activeMenus = new List<BaseMenuController>();
    public UnityEvent onMenuClosed;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 各メニューが有効になったタイミング（OnEnableなど）で呼び出して、
    /// アクティブなメニューとして登録します。
    /// </summary>
    public void RegisterMenu(BaseMenuController menu) {
        activeMenu = menu;
    }

    /// <summary>
    /// EnterキーなどによるSubmit入力時に、現在アクティブなメニューのSubmitを呼び出す。
    /// </summary>
    public void Submit() {
        if (activeMenu != null) {
            activeMenu.Submit();
        } else {
            Debug.LogWarning("アクティブなメニューが登録されていません。");
        }
    }

    /// <summary>
    /// 移動キーによる入力時に、現在アクティブなメニューのNavigateを呼び出す。  
    /// </summary>
    /// <param name="direction"></param>
    public void Navigate(Vector2Int direction) {
        if (activeMenu != null) {
            activeMenu.Navigate(direction);
        }
    }

    public void OpenMenu(){
        activeMenu.OpenMenu();
        RegisterActiveMenu(activeMenu);
        RegisterMenu(activeMenu);
        // Debug.Log(activeMenu.GetType().Name + "を開きました。");
    }

    public void CloseMenu(){
        // 現在の activeMenu を閉じる
        activeMenu.CloseMenu();
        
        // activeMenus から閉じたメニューを削除
        UnregisterActivesMenu(activeMenu);
        // Debug.Log(activeMenu.GetType().Name + "を閉じました。");

        // 残っているメニューがあれば、リストの末尾（最後に登録されたメニュー）を activeMenu に設定
        if (activeMenus.Count > 0) {
            activeMenu = activeMenus[activeMenus.Count - 1];
            // Debug.Log(activeMenu.GetType().Name + "が新しいアクティブメニューに設定されました。");
        } else {
            activeMenu = null;
            // Debug.Log("アクティブなメニューが存在しません。");
        }
    }

    public void CloseAllMenus(){
        int roopCount = activeMenus.Count;
        for(int i = 0; i < roopCount; i++){                                    
            CloseMenu(); 
        }
        activeMenus.Clear();
        activeMenu = null;
        onMenuClosed?.Invoke();
    }

    public void UnregisterActivesMenu(BaseMenuController menu){
        activeMenus.Remove(menu);
        // Debug.Log(menu.GetType().Name + "を解除しました。");        
    }

    public void RegisterActiveMenu(BaseMenuController menu){
        activeMenus.Add(menu);
        // Debug.Log(menu.GetType().Name + "を登録しました。");        
    }

    public T SetActiveMenu<T>() where T : BaseMenuController {
        activeMenu = GetComponent<T>();
        OpenMenu();
        return activeMenu as T;
    }

    //特定のメニューを閉じる
    public void CloseSpecificMenu<T>() where T : BaseMenuController {
        BaseMenuController specificMenu = GetComponent<T>();
        if(specificMenu != null){
            specificMenu.CloseMenu();
            UnregisterActivesMenu(specificMenu);
        }
    }
}