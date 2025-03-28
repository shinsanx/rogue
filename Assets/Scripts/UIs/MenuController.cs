using UnityEngine;
using UnityEngine.Events;

public class MenuController : MonoBehaviour {
        
    private bool isMenuOpen = false;
    //public GameEvent OnToggleActionMap; //UserInputのOnToggleActionMapが登録
        

    private float roundX;
    private float roundY;

    void Start() {                
    }


    // ========================================================
    // メニューの開閉
    // ========================================================
    /// <summary>
    /// メニューを表示する
    /// </summary>
    public void OpenMenu() {        
        MenuManager.Instance.SetActiveMenu<MainMenuController>();
        // if(MenuManager.Instance.activeMenus.Count >= 1){
        //     isMenuOpen = true;
        //     OnToggleActionMap.Raise();
        // }
    }
    /// <summary>
    /// メニューを非表示にする
    /// </summary>
    public void CloseMenu() {        
        MenuManager.Instance.CloseMenu();

        // //アクティブメニューがゼロになると、移動可能にする
        // if(MenuManager.Instance.activeMenus.Count == 0){
        //     isMenuOpen = false;
        //     OnToggleActionMap.Raise();
        // }
    }

    


    /// <summary>
    /// メニューの開閉をトグルする
    /// </summary>
    public void ToggleMenu() {        
        if (MenuManager.Instance.isMenuOpen) {
            CloseMenu();
        } else {            
            OpenMenu();
        }
    }

    // ========================================================
    // カーソル移動
    // ========================================================
    public void Navigate(Vector2 navigateVector) {                

        roundX = Mathf.Round(navigateVector.x);
        roundY = Mathf.Round(navigateVector.y);

        Vector2Int navigateVectorInt = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理                

        MenuManager.Instance.Navigate(navigateVectorInt);
        
        
    }

    public void Submit() {        
        MenuManager.Instance.Submit();
    }
}