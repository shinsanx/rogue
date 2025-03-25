using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
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
    [SerializeField] GameObject objectParent;
    [SerializeField] GameObject stairPrefab;
    //ランダムなポジションにアイテムを配置
    //itemPrefabはやがて置き換える
    public async Task ArrangeItemToRandomPosition(List<ItemSO> itemSOs, int itemCount) {
        //itemSOsの中からアイテムをランダムに選択
        ItemSO selectedItem = itemSOs.OrderBy(item => Random.Range(0, int.MaxValue)).First();

        // アイテムを配置
        for (int i = 0; i < itemCount; i++) {
            PlaceItem(TileManager.i.GetRandomPosition(), selectedItem);
            await Task.Yield();
        }
    }

    public void PlaceItem(Vector2Int position , ItemSO itemSO) {
        // アイテムを配置
        GameObject itemObject = Instantiate(itemPrefab, position.ToVector2(), Quaternion.identity);
        Item item = itemObject.GetComponent<Item>();
        item.itemSO = itemSO;
        item.Initialize();
        itemObject.transform.SetParent(itemParent.transform);
        item.objectData.SetPosition(position);
    }

    //敵をランダムなポジションに配置
    public async Task ArrangeEnemyToRandomPosition(List<MonsterStatusSO> enemies, int enemyCount) {
        //enemiesの中から敵をランダムに選択
        MonsterStatusSO selectedEnemy = enemies.OrderBy(enemy => Random.Range(0, int.MaxValue)).First();
        
        // 敵を配置
        for (int i = 0; i < enemyCount; i++) {
            PlaceEnemy(enemyPrefab, TileManager.i.GetRandomPosition(), selectedEnemy);
            await Task.Yield();
        }
    }

    public void PlaceEnemy(GameObject enemyPrefab, Vector2Int position, MonsterStatusSO monsterSO) {
        // 敵を配置
        GameObject enemyObject = Instantiate(enemyPrefab, position.ToVector2(), Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.monsterSO = monsterSO;
        enemy.InitializeEnemy();
        enemyObject.transform.SetParent(enemyParent.transform);        
        enemyObject.GetComponent<ObjectData>().SetPosition(position);
    }

    public async Task ArrangeStairToRandomPosition() {
        PlaceStair(TileManager.i.GetRandomPosition());
        await Task.Yield();
    }

    private void PlaceStair(Vector2Int position) {
        GameObject stairObject = Instantiate(stairPrefab, position.ToVector2(), Quaternion.identity);
        stairObject.transform.SetParent(itemParent.transform);
        stairObject.GetComponent<Stair>().Initialize();
        stairObject.GetComponent<ObjectData>().SetPosition(position);
    }

    public void DestroyAllObjects() {
        foreach (Transform child in enemyParent.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in itemParent.transform) {
            Destroy(child.gameObject);
        }
    }
}
