using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllItemList_SO", menuName ="Item/AllItemListSO", order =0)]
public class AllItemListSO : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public int id;
        public ItemSO itemSO;
    }
    public List<ItemData> itemDataList = new List<ItemData>();
}