using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Item/InventorySO")]
public class InventorySO : ScriptableObject
{
    public List<ItemSO> items;

    // インベントリ内の全アイテムを取得
    public List<ItemSO> GetAllItems() {
        return items;
    }

    public void ResetInventory() {
        items.Clear();
    }
}
