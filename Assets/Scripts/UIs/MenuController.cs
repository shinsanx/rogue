using UnityEngine;
using UnityEngine.Events;

public class MenuController : MonoBehaviour {
    
    //[SerializeField] private InventoryController inventoryUI; // インベントリUIのオブジェクト
    private bool isMenuOpen = false;
    public UnityEvent onToggleActionMap; //UserInputのOnTggleActionMapが登録
    
    // [SerializeField] private GameObject menuUI;

    private float roundX;
    private float roundY;

    void Start() {
        //MenuManagerのCloseAllMenusが呼び出されたら、CheckMenuClosedを呼び出す
        MenuManager.Instance.onMenuClosed.AddListener(CheckMenuClosed);
    }


    // ========================================================
    // メニューの開閉
    // ========================================================
    /// <summary>
    /// メニューを表示する
    /// </summary>
    public void OpenMenu() {        
        MenuManager.Instance.SetActiveMenu<MainMenuController>();
        if(MenuManager.Instance.activeMenus.Count >= 1){
            isMenuOpen = true;
            onToggleActionMap?.Invoke();
        }
    }
    /// <summary>
    /// メニューを非表示にする
    /// </summary>
    public void CloseMenu() {        
        MenuManager.Instance.CloseMenu();

        //アクティブメニューがゼロになると、移動可能にする
        if(MenuManager.Instance.activeMenus.Count == 0){
            isMenuOpen = false;
            onToggleActionMap?.Invoke();
        }
    }

    //MenuOpenの状態を確認してActionMapを変更する
    //MenuManagerのCloseAllMenusが呼び出されたら、CheckMenuClosedを呼び出す
    public void CheckMenuClosed() {        
        if(MenuManager.Instance.activeMenus.Count == 0){
            isMenuOpen = false;
            onToggleActionMap?.Invoke();
        }
    }


    /// <summary>
    /// メニューの開閉をトグルする
    /// </summary>
    public void ToggleMenu() {        
        if (isMenuOpen) {            
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