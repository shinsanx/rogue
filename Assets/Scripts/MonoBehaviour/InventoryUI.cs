using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory; // プレイヤーのインベントリへの参照
    public GameObject itemSlotPrefab;       // アイテムスロットのPrefab
    public Transform itemsParent;           // アイテムスロットを配置する親Transform

    private Dictionary<ItemSO, GameObject> itemSlots = new Dictionary<ItemSO, GameObject>();

    private void Start()
    {
        UpdateUI();
        // インベントリの更新を監視する場合はイベントリスナーを追加
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
            Text itemName = slot.transform.Find("ItemName").GetComponent<Text>();
            itemName.text = item.itemName;

            // // アイテム数量の設定
            // Text itemQuantity = slot.transform.Find("Quantity").GetComponent<Text>();
            // itemQuantity.text = $"x{quantity}";

            // 使用ボタンの設定
            Button useButton = slot.transform.Find("UseButton").GetComponent<Button>();
            useButton.onClick.AddListener(() => {
                // 使用操作（例：プレイヤーやターゲットを指定）
                playerInventory.UseItem(item, this.GetComponent<Player>());
                UpdateUI();
            });

            // スロットを辞書に追加
            itemSlots.Add(item, slot);
        }
    }
}