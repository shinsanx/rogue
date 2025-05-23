using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuController : BaseMenuController {
    // シーン上で各項目（TextMeshProUGUI）のリファレンスを設定
    [SerializeField] private TextMeshProUGUI itemMenuText;  // 「持ち物」
    [SerializeField] private TextMeshProUGUI footMenuText;  // 「足元」
    [SerializeField] private TextMeshProUGUI etcMenuText;   // 「その他」

    // シーン上でMainMenuControllerをアタッチしたオブジェクトを取得
    [SerializeField] private GameObject mainMenuObject;

    // ハイライト用の色（選択項目）と通常時の色
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.yellow;

    [SerializeField] private GameEvent openStatusMenu;
    [SerializeField] private GameEvent closeStatusMenu;
    [SerializeField] private CurrentSelectedObjectSO currentSelectedObjectSO;
    /// <summary>
    /// メニュー項目を初期化してリストに登録する（Inspector の設定内容も利用可能）
    /// </summary>
    private void OnEnable() {        
        Debug.Log("MainMenuControllerのInitializeMenuが呼び出されました。");
        menuItems = new List<GameObject>();
        // 各オブジェクトをリストへ追加
        menuItems.Add(itemMenuText.gameObject);
        menuItems.Add(footMenuText.gameObject);
        menuItems.Add(etcMenuText.gameObject);

        currentIndex = 0;
        UpdateHighlights();
    }


    /// <summary>
    /// カーソル位置更新に合わせて、各項目のハイライトも更新する
    /// </summary>
    protected override void UpdateCursorPosition() {
        base.UpdateCursorPosition();
        UpdateHighlights();
    }

    /// <summary>
    /// 各テキストの色を、選択状態に合わせて更新する
    /// </summary>
    private void UpdateHighlights() {
        for (int i = 0; i < menuItems.Count; i++) {
            TextMeshProUGUI textComp = menuItems[i].GetComponent<TextMeshProUGUI>();
            if (textComp != null) {
                textComp.color = (i == currentIndex) ? highlightedColor : normalColor;
            }
        }
    }

    /// <summary>
    /// 決定時の処理。選択中の項目に応じて、必要な処理（たとえば InventoryUI を開いたり）を実行
    /// </summary>
    public override void Submit() {
        switch (currentIndex) {
            case 0: //道具
                // InventoryUI を呼び出す
                MenuManager.Instance.SetActiveMenu<InventoryController>();                                
                break;
            case 1: //足元
                GameObject objectByPosition = TileManager.i.GetPlayerFootObject();                
                currentSelectedObjectSO.Object = objectByPosition;
                if(objectByPosition == null) return;
                IMenuActionAdapter menuActionAdapter = objectByPosition.GetComponent<IMenuActionAdapter>();
                if(menuActionAdapter != null) {
                    Debug.Log("menuActionAdapter:" + menuActionAdapter.GetType().Name);
                    menuActionAdapter.OnSelected();
                } else {
                    Debug.Log("IMenuActionAdapterが見つかりません。");
                }                
                break;
            case 2:
                Debug.Log("その他が選択されました。");
                // その他の処理を実装
                break;
            default:
                Debug.LogError("無効な選択インデックスです。");
                break;
        }
    }

    public override void OpenMenu() {        
        isActive = true;
        mainMenuObject.SetActive(true);
        openStatusMenu.Raise();
    }

    public override void CloseMenu() {        
        isActive = false;
        mainMenuObject.SetActive(false);        
        closeStatusMenu.Raise();
    }
    
    
}