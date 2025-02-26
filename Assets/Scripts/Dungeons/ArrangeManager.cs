using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ArrangeManager : MonoBehaviour {
    private static ArrangeManager _instance;
    public static ArrangeManager i {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<ArrangeManager>();
                if (_instance == null) {
                    Debug.LogError("ArrangeManagerがシーンに存在しません。");
                }
            }
            return _instance;
        }
    }

    [SerializeField] GameObject enemyParent;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject itemParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] int itemCount;
    [SerializeField] int enemyCount;
    //ランダムなポジションにアイテムを配置
    //itemPrefabはやがて置き換える
    public async Task ArrangeItemToRandomPosition() {
        // アイテムを取得                
        for (int i = 0; i < itemCount; i++) {
            PlaceItem(itemPrefab, TileManager.i.GetRandomPosition());
            await Task.Yield();            
        }
    }

    public void PlaceItem(GameObject itemPrefab, Vector2Int position) {
        // アイテムを配置
        GameObject itemObject = Instantiate(itemPrefab, position.ToVector2(), Quaternion.identity);
        itemObject.transform.SetParent(itemParent.transform);
        itemObject.GetComponent<Item>().Initialize();
        itemObject.GetComponent<IObjectData>().Position = position;
    }

    public async Task ArrangeEnemyToRandomPosition() {
        // 敵を配置
        for (int i = 0; i < enemyCount; i++) {
            PlaceEnemy(enemyPrefab, TileManager.i.GetRandomPosition());
            await Task.Yield();
        }
    }

    public void PlaceEnemy(GameObject enemyPrefab, Vector2Int position) {
        // 敵を配置
        GameObject enemyObject = Instantiate(enemyPrefab, position.ToVector2(), Quaternion.identity);
        enemyObject.transform.SetParent(enemyParent.transform);
        enemyObject.GetComponent<Enemy>().InitializeEnemy();
        enemyObject.GetComponent<IObjectData>().Position = position;
    }
}
