using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour {
    public Player player; // プレイヤーのインベントリへの参照
    private PlayerInventory playerInventory;
    public GameObject itemSlotPrefab;       // アイテムスロットのPrefab
    public Transform itemsParent;           // アイテムスロットを配置する親Transform
    public GameObject cursorPrefab;               // カーソルのプレハブ
    public GameObject itemActionMenuPrefab; // アイテムアクションメニューのPrefab    
    private GameObject cursorInstance; // カーソルのインスタンス
    private GameObject itemActionMenuInstance; // サブメニューのインスタンス
    private GameObject menuUI; // メニューUIのオブジェクト

    

    private List<ItemSO> itemSlots = new List<ItemSO>();
    private List<GameObject> slotList = new List<GameObject>();
    private int selectedIndex = 0;

    private bool isSubMenuOpen = false;
    private int subMenuSelectedIndex = 0;
    private List<Button> subMenuButtons = new List<Button>();

    private const int ItemsPerPage = 12; // 1ページに表示するアイテム数
    private int currentPage = 0; // 現在のページ

    // ボタンの情報を管理する構造体
    private struct SubMenuButtonInfo
    {
        public string buttonName;      // ボタンの名前（GameObject検索用）
        public string displayName;     // 表示名
        public System.Action<ItemSO> action;  // ボタンが押されたときの処理

        public SubMenuButtonInfo(string buttonName, string displayName, System.Action<ItemSO> action)
        {
            this.buttonName = buttonName;
            this.displayName = displayName;
            this.action = action;
        }
    }    

    public void Initialize() {
        playerInventory = player.playerInventory;
        if (playerInventory != null) {
            playerInventory.OnInventoryUpdated += UpdateUI; // イベントを購読
            playerInventory.onItemUsed += CloseMenu;
        }
        UpdateUI();
    }

    // インベントリUIを更新するメソッド
    public void UpdateUI() {
        // 既存のスロットをクリア
        foreach (Transform child in itemsParent) {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();
        slotList.Clear();

        // インベントリ内の全アイテムを取得
        List<ItemSO> items = playerInventory.GetAllItems();

        // 現在のページに基づいてアイテムを表示
        int startIndex = currentPage * ItemsPerPage;
        int endIndex = Mathf.Min(startIndex + ItemsPerPage, items.Count);

        for (int i = startIndex; i < endIndex; i++) {
            ItemSO item = items[i];
            // アイテムスロットのインスタンスを生成
            GameObject slot = Instantiate(itemSlotPrefab, itemsParent);
            // アイテムアイコンの設定
            Image icon = slot.transform.Find("Icon").GetComponent<Image>();
            if (item.icon != null) {
                icon.sprite = item.icon;
            } else {
                // デフォルトのアイコンを設定
                icon.sprite = null;
            }

            // アイテム名の設定
            TextMeshProUGUI itemName = slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            itemName.text = item.itemName;

            // スロットをリストに追加
            slotList.Add(slot);
            itemSlots.Add(item);
        }

        // カーソルの初期化または更新
        if (cursorInstance == null) {
            cursorInstance = Instantiate(cursorPrefab, itemsParent);
        }

        // 選択インデックスをリセット
        selectedIndex = 0;
        UpdateCursorPosition();
    }

    // カーソルの位置を更新するメソッド
    public void MoveCursor(Vector2Int direction) {
        if (slotList.Count == 0) return;        

        if (isSubMenuOpen) {
            MoveSubMenuCursor(direction);
            return;
        }

        // ページ切り替えの処理
        int previousPage = currentPage;
        if (direction.x > 0) {
            currentPage++;
        } else if (direction.x < 0) {
            currentPage--;
        }

        int totalPages = Mathf.CeilToInt((float)playerInventory.GetAllItems().Count / ItemsPerPage);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        // ページが変わったらUIを更新し、selectedIndexをリセット
        if (currentPage != previousPage) {
            UpdateUI();
            selectedIndex = 0; // ページが変わったときのみ初期化
        }

        // 垂直移動のみ対応（左右移動不要な場合）
        if (direction.y > 0) {
            selectedIndex--;
        } else if (direction.y < 0) {
            selectedIndex++;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, slotList.Count - 1);

        UpdateCursorPosition();
    }

    // カーソルの位置を更新するメソッド
    private void UpdateCursorPosition() {
        if (slotList.Count == 0 || cursorInstance == null) return;
        if (cursorInstance.GetComponent<Image>().enabled == false) {
            cursorInstance.GetComponent<Image>().enabled = true;
        }

        GameObject selectedSlot = slotList[selectedIndex];
        cursorInstance.transform.SetParent(selectedSlot.transform, false);
        //カーソルを要素の左端に配置
        cursorInstance.transform.localPosition = new Vector3(-selectedSlot.GetComponent<RectTransform>().rect.width / 2, 0, 0);
    }

    private void OnDestroy() {
        PlayerInventory playerInventory = player.playerInventory;
        if (playerInventory != null) {
            playerInventory.OnInventoryUpdated -= UpdateUI; // イベントの購読を解除
        }
    }

    // アイテムを選択してサブメニューを表示するメソッド
    public void Submit() {
        if (isSubMenuOpen) {
            // サブメニューが開いている場合、選択されたアクションを実行
            if (subMenuButtons.Count > 0 && subMenuSelectedIndex >= 0 && subMenuSelectedIndex < subMenuButtons.Count) {
                subMenuButtons[subMenuSelectedIndex].onClick.Invoke();
            }
        } else {
            // メインメニューが開いている場合、サブメニューを表示
            OpenSubMenu();
        }
    }

    // メニューを閉じるメソッド
    public void CloseMenu(){
        if(menuUI != null){
            menuUI.SetActive(false);
        }                
    }

    public void OpenMenu(){
        if(menuUI != null){
            menuUI.SetActive(true);
        }
    }

    // ================================================
    // ================== SubMenu ==================
    // ================================================

    private void OpenSubMenu()
    {
        if (slotList.Count == 0) return;

        ItemSO selectedItem = itemSlots[selectedIndex];
        if (selectedItem == null)
        {
            Debug.LogError("選択されたアイテムが存在しません。");
            return;
        }

        if (itemActionMenuInstance != null) return;

        // サブメニューのインスタンスを生成
        itemActionMenuInstance = Instantiate(itemActionMenuPrefab, itemsParent.parent);
        itemActionMenuInstance.transform.SetAsLastSibling();

        // ボタン情報のリストを作成
        var buttonInfos = new List<SubMenuButtonInfo>
        {
            new SubMenuButtonInfo("UseButton", "使う", UseItem),
            new SubMenuButtonInfo("PlaceButton", "置く", item => {
                Debug.Log("置く機能は未実装です。");
                CloseSubMenu();
            }),
            new SubMenuButtonInfo("ThrowButton", "投げる", item => {
                Debug.Log("投げる機能は未実装です。");
                CloseSubMenu();
            })
        };

        subMenuButtons.Clear();

        // 各ボタンを設定
        foreach (var buttonInfo in buttonInfos)
        {
            Button button = itemActionMenuInstance.transform
                .Find(buttonInfo.buttonName)
                .GetComponent<Button>();

            // ボタンのテキストを設定（必要な場合）
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = buttonInfo.displayName;
            }

            // ボタンのクリックイベントを設定
            button.onClick.AddListener(() => {
                buttonInfo.action.Invoke(selectedItem);
                CloseSubMenu();
            });

            subMenuButtons.Add(button);
        }

        // 最初のボタンを選択
        subMenuSelectedIndex = 0;
        UpdateSubMenuCursor();
        isSubMenuOpen = true;
    }

    // サブメニューを閉じるメソッド
    private void CloseSubMenu() {
        if (itemActionMenuInstance != null) {
            Destroy(itemActionMenuInstance);
            itemActionMenuInstance = null;
        }

        if (cursorInstance != null) {
            Destroy(cursorInstance);
            cursorInstance = null;
        }

        isSubMenuOpen = false;
        subMenuSelectedIndex = 0;
    }

    // サブメニューカーソルの位置を更新するメソッド
    private void UpdateSubMenuCursor() {
        if (cursorInstance == null) {
            cursorInstance = Instantiate(cursorPrefab, itemActionMenuInstance.transform);
            cursorInstance.transform.localPosition = new Vector3(-cursorInstance.GetComponent<RectTransform>().rect.width / 2, 0, 0);            
        }

        if (subMenuButtons.Count == 0 || cursorInstance == null) return;

        Button selectedButton = subMenuButtons[subMenuSelectedIndex];
        cursorInstance.transform.SetParent(selectedButton.transform, false);
        RectTransform cursorRect = cursorInstance.GetComponent<RectTransform>();
        if (cursorRect != null) {
            cursorRect.anchoredPosition = new Vector3(-selectedButton.GetComponent<RectTransform>().rect.width / 2, 0, 0);            
        }
    }

    // サブメニューのカーソルを移動するメソッド
    private void MoveSubMenuCursor(Vector2Int direction) {
        if (subMenuButtons.Count == 0) return;

        // 垂直移動のみ対応（左右移動不要な場合）
        if (direction.y > 0) {
            subMenuSelectedIndex--;
        } else if (direction.y < 0) {
            subMenuSelectedIndex++;
        }

        // インデックスをクランプ
        subMenuSelectedIndex = Mathf.Clamp(subMenuSelectedIndex, 0, subMenuButtons.Count - 1);
        UpdateSubMenuCursor();
    }

    // サブメニューカーソル用のナビゲーション
    public void NavigateSubMenu(Vector2Int direction) {
        MoveSubMenuCursor(direction);
    }

    // アイテムを使用するメソッド
    private void UseItem(ItemSO item) {
        PlayerInventory playerInventory = player.playerInventory;
        if (playerInventory != null) {
            playerInventory.UseItem(item, player);
            UpdateUI();
        } else {
            Debug.LogError("PlayerInventory コンポーネントが見つかりません。");
        }
    }
}