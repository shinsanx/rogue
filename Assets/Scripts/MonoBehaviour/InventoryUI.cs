using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Player player; // プレイヤーのインベントリへの参照
    public GameObject itemSlotPrefab;       // アイテムスロットのPrefab
    public Transform itemsParent;           // アイテムスロットを配置する親Transform
    public GameObject cursor;               // カーソルのオブジェクト

    private Dictionary<ItemSO, GameObject> itemSlots = new Dictionary<ItemSO, GameObject>();
    private List<GameObject> slotList = new List<GameObject>();
    private int selectedIndex = 0;

    public void Initialize()
    {
        PlayerInventory playerInventory = player.playerInventory;
        if (playerInventory != null)
        {
            playerInventory.OnInventoryUpdated += UpdateUI; // イベントを購読
        }
        UpdateUI();
        UpdateCursorPosition();
    }

    private void OnDestroy()
    {
        PlayerInventory playerInventory = player.playerInventory;
        if (playerInventory != null)
        {
            playerInventory.OnInventoryUpdated -= UpdateUI; // イベントの購読を解除
        }
    }

    // インベントリUIを更新するメソッド
    public void UpdateUI()
    {
        PlayerInventory playerInventory = player.playerInventory;
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory コンポーネントが見つかりません。");
            return;
        }

        // 既存のスロットをクリア
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();
        slotList.Clear();

        // インベントリ内の全アイテムを取得
        Dictionary<ItemSO, int> items = playerInventory.GetAllItems();

        foreach (var pair in items)
        {
            ItemSO item = pair.Key;
            int quantity = pair.Value;

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

            // スロットを辞書およびリストに追加
            itemSlots.Add(item, slot);
            slotList.Add(slot);
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
        int rows = Mathf.CeilToInt((float)slotList.Count / columns);

        int currentRow = selectedIndex / columns;
        int currentCol = selectedIndex % columns;

        int newCol = currentCol + direction.x;
        int newRow = currentRow + direction.y;

        newCol = Mathf.Clamp(newCol, 0, columns - 1);
        newRow = Mathf.Clamp(newRow, 0, rows - 1);

        int newIndex = newRow * columns + newCol;
        newIndex = Mathf.Clamp(newIndex, 0, slotList.Count - 1);

        selectedIndex = newIndex;
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        if (slotList.Count == 0 || cursor == null) return;

        GameObject selectedSlot = slotList[selectedIndex];
        cursor.transform.SetParent(selectedSlot.transform, false);
        cursor.transform.localPosition = Vector3.zero;
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
}