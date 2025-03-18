using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMenuController : MonoBehaviour {
    // メニュー項目（たとえば各ボタンやアイテムスロットなどの GameObject ）のリスト
    [SerializeField]
    protected List<GameObject> menuItems = new List<GameObject>();

    // 現在選択中のインデックス（初期値は 0）
    protected int currentIndex = 0;

    // カーソルを表す GameObject（必要に応じて設定）
    [SerializeField]
    protected GameObject cursor;
    protected GameObject cursorInstance;

    public bool isActive { get; set;}

    /// <summary>
    /// 派生クラスでメニュー項目の初期化を行う
    /// （たとえばシーン上の Text や Button、スロットを menuItems リストに追加する）
    /// </summary>
    //protected abstract void InitializeMenu();

    /// <summary>
    /// Start で初期化し、カーソル位置を更新する
    /// </summary>
    // protected virtual void Start() {
    //     InitializeMenu();
    //     UpdateCursorPosition();
    // }

    /// <summary>
    /// 共通のカーソル移動処理（上下入力のみ対応の例）
    /// </summary>
    /// <param name="direction">Vector2Int (x はページ切り替えなど、y で上下移動)</param>
    public virtual void Navigate(Vector2Int direction) {        
        if (menuItems.Count == 0) return;

        if (direction.y > 0) {
            currentIndex--;
        } else if (direction.y < 0) {
            currentIndex++;
        }
        // 項目数を超えないよう Clamp（または必要に応じラップアラウンドに変更）
        currentIndex = Mathf.Clamp(currentIndex, 0, menuItems.Count - 1);
        UpdateCursorPosition();
    }

    /// <summary>
    /// カーソルの位置更新処理  
    /// 基本実装としては、カーソルの親（またはアンカー位置）を現在選択中の項目に合わせる
    /// </summary>
    protected virtual void UpdateCursorPosition() {
        if (cursor == null || menuItems.Count == 0) return;

        if(cursorInstance == null) {            
            cursorInstance = Instantiate(cursor, transform);
        }

        if(cursorInstance == null) {
            Debug.LogError("cursorInstance is null");
            return;
        }

        GameObject selectedItem = menuItems[currentIndex];

        if(selectedItem == null) {
            Debug.LogError("selectedItem is null");
            return;
        }
        
        cursorInstance.transform.SetParent(selectedItem.transform, false);        

        // 例：カーソルを項目の左端に配置（必要に応じ調整）
        RectTransform selectedRect = selectedItem.GetComponent<RectTransform>();
        if (selectedRect != null) {
            cursorInstance.transform.localPosition = new Vector3(-selectedRect.rect.width / 2, 0f, 0f);
        }
    }

    /// <summary>
    /// 決定（Submit）時の処理は、各メニューで固有の実装を行うため抽象メソッドにしておく
    /// </summary>
    public abstract void Submit();

    public abstract void OpenMenu();

    public abstract void CloseMenu();
    
    
}