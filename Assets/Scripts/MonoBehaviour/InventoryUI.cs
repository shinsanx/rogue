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
    public GameObject subMenuCursorPrefab;  // サブメニューカーソルのプレハブ
    private GameObject cursorInstance; // カーソルのインスタンス
    private GameObject itemActionMenuInstance; // サブメニューのインスタンス
    private GameObject subMenuCursorInstance;    // サブメニューカーソルのインスタンス

    private List<ItemSO> itemSlots = new List<ItemSO>();
    private List<GameObject> slotList = new List<GameObject>();
    private int selectedIndex = 0;

    private bool isSubMenuOpen = false;
    private int subMenuSelectedIndex = 0;
    private List<Button> subMenuButtons = new List<Button>();

    private void Start() {
        Initialize();
    }

    public void Initialize() {
        playerInventory = player.playerInventory;
        if (playerInventory != null) {
            playerInventory.OnInventoryUpdated += UpdateUI; // イベントを購読
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

        foreach (ItemSO item in items) {
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

        int columns = GetColumns();

        // 行数を計算
        int rows = slotList.Count;        

        // 現在の行と列を計算
        int currentRow = selectedIndex;
        int currentCol = selectedIndex % columns;
        // 新しい行と列を計算
        //int newCol = currentCol + direction.x;
        int newRow = currentRow + direction.y * -1;

        // 新しい行と列をクランプ
        //newCol = Mathf.Clamp(newCol, 0, columns - 1);
        newRow = Mathf.Clamp(newRow, 0, rows - 1);

        //int newIndex = newRow * columns + newCol;
        int newIndex = newRow;
        newIndex = Mathf.Clamp(newIndex, 0, slotList.Count - 1);

        selectedIndex = newIndex;
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

    private int GetColumns() {
        // レイアウトに応じてカラム数を設定
        // 例えば、GridLayoutGroupを使用している場合
        GridLayoutGroup grid = itemsParent.GetComponent<GridLayoutGroup>();
        if (grid != null) {
            // 親のRectTransformから横幅を取得し、セル幅で割る
            int columns = Mathf.FloorToInt(itemsParent.GetComponent<RectTransform>().rect.width / (grid.cellSize.x + grid.spacing.x));
            return Mathf.Max(columns, 1);
        }
        return 1;
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

    private void OpenSubMenu() {
        if (slotList.Count == 0) return;

        // 選択中のアイテムを取得
        ItemSO selectedItem = itemSlots[selectedIndex];
        if (selectedItem == null) {
            Debug.LogError("選択されたアイテムが存在しません。");
            return;
        }

        // サブメニューが既に表示されている場合は無視
        if (itemActionMenuInstance != null) {
            return;
        }

        // サブメニューのインスタンスを生成
        itemActionMenuInstance = Instantiate(itemActionMenuPrefab, itemsParent.parent); // UIの構造に応じて親を調整
        itemActionMenuInstance.transform.SetAsLastSibling(); // 最前面に表示

        // ボタンの取得
        Button useButton = itemActionMenuInstance.transform.Find("UseButton").GetComponent<Button>();
        Button placeButton = itemActionMenuInstance.transform.Find("PlaceButton").GetComponent<Button>();
        Button throwButton = itemActionMenuInstance.transform.Find("ThrowButton").GetComponent<Button>();

        subMenuButtons.Clear();
        subMenuButtons.Add(useButton);
        subMenuButtons.Add(placeButton);
        subMenuButtons.Add(throwButton);

        // 最初のボタンを選択する
        subMenuSelectedIndex = 0;
        UpdateSubMenuCursor();

        // ボタンの設定
        useButton.onClick.AddListener(() => {
            UseItem(selectedItem);
            CloseSubMenu();
        });

        // 「置く」と「投げる」は後で実装
        placeButton.onClick.AddListener(() => {
            Debug.Log("置く機能は未実装です。");
            CloseSubMenu();
        });

        throwButton.onClick.AddListener(() => {
            Debug.Log("投げる機能は未実装です。");
            CloseSubMenu();
        });

        isSubMenuOpen = true;
    }

    // サブメニューを閉じるメソッド
    private void CloseSubMenu() {
        if (itemActionMenuInstance != null) {
            Destroy(itemActionMenuInstance);
            itemActionMenuInstance = null;
        }

        if (subMenuCursorInstance != null) {
            Destroy(subMenuCursorInstance);
            subMenuCursorInstance = null;
        }

        isSubMenuOpen = false;
        subMenuSelectedIndex = 0;
    }

    // サブメニューカーソルの位置を更新するメソッド
    private void UpdateSubMenuCursor() {
        if (subMenuCursorInstance == null) {
            subMenuCursorInstance = Instantiate(subMenuCursorPrefab, itemActionMenuInstance.transform);
            subMenuCursorInstance.transform.localPosition = new Vector3(-subMenuCursorInstance.GetComponent<RectTransform>().rect.width / 2, 0, 0);
            //subMenuCursorInstance.transform.SetAsFirstSibling(); // ボタンの下に表示
        }

        if (subMenuButtons.Count == 0 || subMenuCursorInstance == null) return;

        Button selectedButton = subMenuButtons[subMenuSelectedIndex];
        subMenuCursorInstance.transform.SetParent(selectedButton.transform, false);
        RectTransform cursorRect = subMenuCursorInstance.GetComponent<RectTransform>();
        if (cursorRect != null) {
            cursorRect.anchoredPosition = new Vector3(-selectedButton.GetComponent<RectTransform>().rect.width / 2, 0, 0); // ボタンの中心に配置
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