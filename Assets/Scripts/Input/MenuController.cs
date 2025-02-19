using UnityEngine;

public class MenuController : MonoBehaviour {

    [SerializeField] private GameObject menuUI; // メニューUIのオブジェクト    
    [SerializeField] private InventoryUI inventoryUI; // インベントリUIのオブジェクト
    private bool isMenuOpen = false;
    private Vector2 navigateVector;
    
    private float roundX;
    private float roundY;
    
    
    // ========================================================
    // メニューの開閉
    // ========================================================
    /// <summary>
    /// メニューを表示する
    /// </summary>
    public void OpenMenu() {
        if (menuUI != null) {
            menuUI.SetActive(true);
            isMenuOpen = true;
        } else {
            Debug.LogError("menuUIがアサインされていません。");
        }
    }
    /// <summary>
    /// メニューを非表示にする
    /// </summary>
    public void CloseMenu() {
        if (menuUI != null) {
            menuUI.SetActive(false);
            isMenuOpen = false;
        } else {
            Debug.LogError("menuUIがアサインされていません。");
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
        this.navigateVector = navigateVector;

        roundX = Mathf.Round(navigateVector.x);
        roundY = Mathf.Round(navigateVector.y);

        Vector2Int navigateVectorInt = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理                

        if (inventoryUI != null) {            
            inventoryUI.MoveCursor(navigateVectorInt);
        }
    }
}