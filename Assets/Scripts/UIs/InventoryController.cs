using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryController : BaseMenuController {
    public Player player; // プレイヤーのインベントリへの参照
    private PlayerInventory playerInventory;
    public GameObject itemSlotPrefab;       // アイテムスロットのPrefab
    public Transform itemsParent;           // アイテムスロットを配置する親Transform            
    // シーン上でInventoryControllerをアタッチしたオブジェクトを取得    
    [SerializeField] private GameObject itemWindow;    

    private List<ItemSO> itemSlots = new List<ItemSO>();    

    private const int ItemsPerPage = 12; // 1ページに表示するアイテム数
    private int currentPage = 0; // 現在のページ


    protected override void InitializeMenu() {
        playerInventory = player.playerInventory;
        if (playerInventory != null) {
            playerInventory.OnInventoryUpdated += UpdateInventoryUI; // イベントを購読            
        }
        UpdateInventoryUI();
    }

    // インベントリUIを更新するメソッド
    public void UpdateInventoryUI() {
        ClearExistingSlots();
        DisplayItemsOnCurrentPage();        
    }

    // 既存のスロットをクリアするメソッド
    private void ClearExistingSlots() {
        foreach (Transform child in itemsParent) {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();
        menuItems.Clear();
    }

    // 現在のページに基づいてアイテムを表示するメソッド
    private void DisplayItemsOnCurrentPage() {
        List<ItemSO> items = playerInventory.GetAllItems();
        int startIndex = currentPage * ItemsPerPage;
        int endIndex = Mathf.Min(startIndex + ItemsPerPage, items.Count);

        for (int i = startIndex; i < endIndex; i++) {
            ItemSO item = items[i];
            GameObject slot = Instantiate(itemSlotPrefab, itemsParent);
            SetItemInformation(slot, item);
            menuItems.Add(slot);
            itemSlots.Add(item);
        }
    }

    // アイテム情報を設定するメソッド
    private void SetItemInformation(GameObject slot, ItemSO item) {
        Image icon = slot.transform.Find("Icon").GetComponent<Image>();
        if (item.icon != null) {
            icon.sprite = item.icon;
        } else {                
            Debug.LogError("アイテムにアイコンが設定されていません。");
        }

        TextMeshProUGUI itemName = slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        itemName.text = item.itemName;
    }

    // カーソルの位置を更新するメソッド
    public override void Navigate(Vector2Int direction) {
        if (menuItems.Count == 0) return;

        // ページ切り替えの処理をメソッドに分ける
        if (ChangePage(direction.x)) {
            UpdateInventoryUI();
            currentIndex = 0; // ページが変わったときのみ初期化
        }

        // 垂直移動のみ対応（左右移動不要な場合）
        if (direction.y > 0) {
            currentIndex--;
        } else if (direction.y < 0) {
            currentIndex++;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, menuItems.Count - 1);

        UpdateCursorPosition();
    }

    // アイテムを選択してサブメニューを表示するメソッド
    public override void Submit() {
        if(itemSlots.Count == 0) return;
        // サブメニューをアクティブにする
        SubMenuController subMenuController = MenuManager.Instance.SetActiveMenu<SubMenuController>();

        // 選択されているアイテムをサブメニューに渡す
        subMenuController.SetSubMenu(itemSlots[currentIndex]);
    }

    // ページ切り替えを行うメソッド
    private bool ChangePage(int horizontalDirection) {
        int previousPage = currentPage;
        if (horizontalDirection > 0) {
            currentPage++;
        } else if (horizontalDirection < 0) {
            currentPage--;
        }

        int totalPages = Mathf.CeilToInt((float)playerInventory.GetAllItems().Count / ItemsPerPage);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        // ページが変わったかどうかを返す
        return currentPage != previousPage;
    }

    public override void OpenMenu() {
        itemWindow.SetActive(true);
        // MainMenuを閉じる
        MenuManager.Instance.CloseSpecificMenu<MainMenuController>();
        isActive = true;
    }

    public override void CloseMenu() {
        itemWindow.SetActive(false);        
        isActive = false;        
    }
}