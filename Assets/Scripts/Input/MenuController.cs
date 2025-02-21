using UnityEngine;
using UnityEngine.Events;

public class MenuController : MonoBehaviour {
    
    [SerializeField] private InventoryUI inventoryUI; // インベントリUIのオブジェクト
    private bool isMenuOpen = false;

    private float roundX;
    private float roundY;
    
    
    // ========================================================
    // メニューの開閉
    // ========================================================
    /// <summary>
    /// メニューを表示する
    /// </summary>
    public void OpenMenu() {
        Debug.Log("OpenMenu");
        if (inventoryUI != null) {
            inventoryUI.OpenMenu();
            isMenuOpen = true;
        } else {
            Debug.LogError("menuUIがアサインされていません。");
        }
    }
    /// <summary>
    /// メニューを非表示にする
    /// </summary>
    public void CloseMenu() {
        if (inventoryUI != null) {
            inventoryUI.CloseMenu();
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

        roundX = Mathf.Round(navigateVector.x);
        roundY = Mathf.Round(navigateVector.y);

        Vector2Int navigateVectorInt = new Vector2Int((int)roundX, (int)roundY); //四捨五入処理                

        if (inventoryUI != null) {            
            inventoryUI.MoveCursor(navigateVectorInt);
        }
    }

    public void Submit() {
        if (inventoryUI != null) {
            inventoryUI.Submit();
        }
    }
}