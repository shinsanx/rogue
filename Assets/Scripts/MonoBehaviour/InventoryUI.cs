using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Player player; // プレイヤーのインベントリへの参照
    private PlayerInventory playerInventory;
    public GameObject itemSlotPrefab;       // アイテムスロットのPrefab
    public Transform itemsParent;           // アイテムスロットを配置する親Transform
    public GameObject cursorPrefab;               // カーソルのプレハブ
    private GameObject cursorInstance; // カーソルのインスタンス

    private List<ItemSO> itemSlots = new List<ItemSO>();
    private List<GameObject> slotList = new List<GameObject>();
    private int selectedIndex = 0;

    private Vector2 offset = new Vector3(-200, 0);

    public void Initialize()
    {
        playerInventory = player.playerInventory;
        if (playerInventory != null)
        {
            playerInventory.OnInventoryUpdated += UpdateUI; // イベントを購読
        }
        UpdateUI();
    }
    

    // インベントリUIを更新するメソッド
    public void UpdateUI()
    {                
        // 既存のスロットをクリア
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();
        slotList.Clear();

        // インベントリ内の全アイテムを取得
        List<ItemSO> items = playerInventory.GetAllItems();

        foreach (ItemSO item in items)
        {
            // アイテムスロットのインスタンスを生成
            GameObject slot = Instantiate(itemSlotPrefab, itemsParent);
            // アイテムアイコンの設定
            Image icon = slot.transform.Find("Icon").GetComponent<Image>();
            if (item.icon != null)
            {
                icon.sprite = item.icon;
            }
            else
            {
                // デフォルトのアイコンを設定
                icon.sprite = null;
            }

            // アイテム名の設定
            TextMeshProUGUI itemName = slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            itemName.text = item.itemName;

            // 使用ボタンの設定
            Button useButton = slot.transform.Find("UseButton").GetComponent<Button>();
            useButton.onClick.AddListener(() => {
                // 使用操作（例：プレイヤーやターゲットを指定）
                playerInventory.UseItem(item, player);
                UpdateUI();
            });

            // スロットをリストに追加
            itemSlots.Add(item);
            slotList.Add(slot);
        }

        // カーソルの初期化または更新
        if (cursorInstance == null)
        {
            cursorInstance = Instantiate(cursorPrefab, itemsParent);
        }

        // 選択インデックスをリセット
        selectedIndex = 0;
        UpdateCursorPosition();
    }

    // カーソルの位置を更新するメソッド
    public void MoveCursor(Vector2Int direction)
    {           
        if (slotList.Count == 0) return;

        int columns = GetColumns();
        Debug.Log("totalColumns: " + columns);

        // 行数を計算
        int rows = slotList.Count;
        Debug.Log("rows: " + rows);

        // 現在の行と列を計算
        int currentRow = selectedIndex;
        int currentCol = selectedIndex % columns;
        Debug.Log("currentRow: " + currentRow);
        Debug.Log("currentCol: " + currentCol);

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
    private void UpdateCursorPosition()
    {
        if (slotList.Count == 0 || cursorInstance == null) return;

        GameObject selectedSlot = slotList[selectedIndex];
        cursorInstance.transform.SetParent(selectedSlot.transform, false);
        cursorInstance.transform.localPosition = new Vector3(-selectedSlot.GetComponent<RectTransform>().rect.width/2, 0, 0);
    }

    private int GetColumns()
    {
        // レイアウトに応じてカラム数を設定
        // 例えば、GridLayoutGroupを使用している場合
        GridLayoutGroup grid = itemsParent.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            // 親のRectTransformから横幅を取得し、セル幅で割る
            int columns = Mathf.FloorToInt(itemsParent.GetComponent<RectTransform>().rect.width / (grid.cellSize.x + grid.spacing.x));
            return Mathf.Max(columns, 1);
        }
        return 1;
    }

    private void OnDestroy()
    {
        PlayerInventory playerInventory = player.playerInventory;
        if (playerInventory != null)
        {
            playerInventory.OnInventoryUpdated -= UpdateUI; // イベントの購読を解除
        }
    }
}