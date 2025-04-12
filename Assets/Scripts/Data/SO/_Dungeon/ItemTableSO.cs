using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ItemTableSO", menuName = "Dungeon/ItemTableSO")]
public class ItemTableSO : ScriptableObject
{
    [SerializeField]
    private List<WeightedItem> weightedItems = new List<WeightedItem>();
    
    // 元のItemsプロパティとの互換性を保つためのプロパティ
    public List<BaseItemSO> Items 
    {
        get 
        {
            List<BaseItemSO> items = new List<BaseItemSO>();
            foreach (var weightedItem in weightedItems)
            {
                items.Add(weightedItem.item);
            }
            return items;
        }
    }
    
    // ウェイトに基づいてランダムにアイテムを取得するメソッド
    public BaseItemSO GetRandomItem()
    {
        float totalWeight = 0;
        foreach (var weightedItem in weightedItems)
        {
            totalWeight += weightedItem.weight;
        }
        
        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float currentWeight = 0;
        
        foreach (var weightedItem in weightedItems)
        {
            currentWeight += weightedItem.weight;
            if (randomValue <= currentWeight)
            {
                return weightedItem.item;
            }
        }
        
        // 万が一何も選ばれなかった場合は最初のアイテムを返す
        return weightedItems.Count > 0 ? weightedItems[0].item : null;
    }
    
    // ウェイト付きアイテムを表すシリアライズ可能なクラス
    [Serializable]
    public class WeightedItem
    {
        public BaseItemSO item;
        [Range(0, 100)]
        public float weight = 1;
    }
}
