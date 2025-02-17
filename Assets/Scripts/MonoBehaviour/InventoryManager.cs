using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    public List<ItemSO> items;

    public void UseItem(ItemSO item, IEffectReceiver receiver) {
        if (item is ConsumableSO consumable) {
            consumable.effect.ApplyEffect(receiver);
        } else {
            Debug.LogError("Invalid item type");
        }
    }
}
